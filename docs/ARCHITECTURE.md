# Architecture — Audit Restore

A point-in-time restore tool for Dataverse, built as **one self-contained HTML file** that runs
in three hosts from a single source of truth.

## Hosts (one file, three runtimes)

`audit-restore.html` detects its host at load and adapts both **data access** and **theme**:

| Host | Detected by | Data access | Theme |
|------|-------------|-------------|-------|
| Power Platform Toolbox | `window.dataverseAPI` | `dataverseAPI.queryData` / `fetchXmlQuery` / `update` / `execute` | **Dark** (default) |
| XrmToolBox (WebView2) | `window.XTB_CONFIG` | `fetch` the Web API with the injected OAuth bearer token | **Windows 95** |
| D365 web resource / standalone | neither | same-origin `fetch` (session auth) | Light (OS dark honoured) |

```js
const PPTB = !!window.dataverseAPI;   // Power Platform Toolbox
const XTB  = !!window.XTB_CONFIG;     // XrmToolBox WebView2 plugin
document.body.dataset.host = PPTB ? "pptb" : XTB ? "xtb" : "web";
```

The theme is keyed off `body[data-host]` / `body.theme-dark`, so the **same markup** re-skins per
host. PPTB opens dark even on a light OS (dark is a class, not just `prefers-color-scheme`).

## Data layer (`apiGet` / `patchRecord` / `baseUrl` / `headers`)

A thin abstraction hides the host differences:

- **Reads** go through `apiGet(url)`. In PPTB it routes to `queryData` / `fetchXmlQuery`; otherwise
  it `fetch`es. `baseUrl()` returns the XTB absolute org URL, or a same-origin relative path.
  `headers()` adds the `Authorization: Bearer` header in XTB.
- **Writes** go through `patchRecord(eset, id, patch)` — `dataverseAPI.update` in PPTB, otherwise an
  HTTP `PATCH`. Each record is written independently (a concurrency pool of 8), so one failure never
  blocks the rest, and `204 No Content` is correctly counted as success.

## How point-in-time restore works

1. **Find records** — either a user FetchXML, or the built-in *audit-history builder* that queries
   the `audit` table (translated to an OData filter on `audits`) by table + user + date window.
2. **Scan the audit log** for `Update` events on those records since the recovery point
   (`audits` filtered by `_objectid_value` chunks — only the selected records, not the whole table).
3. **Reconstruct** each field's value-at-cutoff = the **`OldValue` of the earliest change after the
   cutoff** (via `RetrieveAuditDetails`). Truncated (>5 KB) values and empty lookups are flagged, not
   written. Calculated/rollup columns are never set.
4. **Preview** before → after (paged 100/page), then **apply** via Web API `PATCH`.

There is **no Dataverse backup/restore** involved — the tool reconstructs values from audit history
and writes them with ordinary Web API updates, so plugins, business rules and security all apply, and
each write itself creates a new audit entry.

## Scale

- **Batch mode** processes the whole matched set in chunks of 1000 (find → reconstruct → apply per
  chunk) with per-batch progress (updated / to-go / %), colour-coded console output, and an ETA.
- **Exclude recently reverted** skips records modified within the last *N* hours so re-runs don't
  re-touch already-fixed records.

## Build & distribution

- `build.js` copies `audit-restore.html` → `dist/index.html` and `icon.svg` → `dist/icon.svg`.
- `package.json` `files: ["dist", "npm-shrinkwrap.json"]` publishes the **`dist/` folder** plus the
  shrinkwrap (both required by the PPTB submission validator). Published from the package **root**.
- The XrmToolBox build (`xrmtoolbox/`) is a `PluginControlBase` hosting a `WebView2` that injects
  `window.XTB_CONFIG` and loads the same HTML — see `xrmtoolbox/README.md`.

## Accessibility

WCAG 2.1 AA across all three themes: AA-contrast palettes, visible focus rings, keyboard-operable
comboboxes (ARIA combobox + listbox), `role="status"`/`aria-live` log, labelled controls, and
`prefers-reduced-motion` honoured.
