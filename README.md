# Audit Restore — Power Platform Toolbox tool

<img src="https://raw.githubusercontent.com/TheMarkChristie/PPTBAuditRestore/main/icon.svg" alt="Audit Restore icon" width="88" />

*by Mark Christie*

Restore Dataverse records to their field state at a point in time, sourced from the **audit log**.
Find the affected records by table / user / date window, preview the exact **before → after**
changes, then apply them in pages of 100.

## What this does

For every record returned by your FetchXML, it **rolls back every change made after the date/time
you specify**, restoring each field to the value it held as of that moment. Anything dated past your
cutoff is discarded — it is not restored.

> Mechanically: it takes the **earliest audit entry on-or-after your date**; that entry's "before"
> snapshot is the state at your date.

**Always run Preview first and review before applying. There is no automatic undo.**

> ⚠️ **How the restore is written:** this tool changes your data by **updating each record through
> the Dataverse Web API** — an HTTP **`PATCH`** per record (`dataverseAPI.update` inside Toolbox; an
> authenticated same-origin `PATCH` as a web resource). It writes the reconstructed point-in-time
> field values straight onto the live records, field by field. There is **no Dataverse backup or
> platform "restore" involved** — it's ordinary Web API writes, so every change is subject to your
> plugins, business rules, and security, and each write itself **creates a new audit entry**.

### At a glance
1. **Records & recovery point** — pick an audited table + user + date window (or paste FetchXML), and set the recovery date/time.
2. **Run** — choose how many records (10 / 100 / 1000 / all), optionally exclude recently-reverted ones, then **Preview**.
3. **Review** the before → after list (paged 100 at a time), then **Apply this page** or **Apply all**.

This is the [Power Platform Toolbox](https://docs.powerplatformtoolbox.com/tool-development)
packaging of the single-file `audit-restore.html`. The same file also runs as a Dataverse
**web resource** — it auto-detects its host (Toolbox vs web resource) and uses the right APIs.

## How it talks to Dataverse

Tools on Power Platform Toolbox never handle tokens. The host injects:

- **`window.dataverseAPI`** — `queryData`, `fetchXmlQuery`, `update`, `retrieve`, `getEntityMetadata`, `execute`.
- **`window.toolboxAPI`** — `connections.getActiveConnection()`, `events.on(...)`, `utils.showNotification(...)`.

The tool detects `window.dataverseAPI` and routes **all** reads/writes through it, so it
automatically uses the **logged-in connection's token** and targets whichever **instance**
is currently selected in Toolbox. On `connection:updated` it reloads against the new instance.
The connected environment name is shown in the header; the org version is read via
`RetrieveVersion`.

| Tool operation | Web resource (fetch) | Toolbox (`dataverseAPI`) |
|---|---|---|
| OData query (audits, systemusers, exclude) | `GET /api/data/v9.2/...` | `queryData(path)` |
| FetchXML (record finder) | `?fetchXml=` | `fetchXmlQuery(xml)` |
| Audit detail / metadata / version | `GET ...` | `queryData(...)` / `execute(...)` |
| Restore write | `PATCH` | `update(logicalName, id, patch)` |

## Build

```bash
cd pptb-audit-restore
node build.js
```

This produces `dist/` with `index.html` (the tool), `icon.svg`, `package.json`, `LICENSE`, and
`README.md` — the layout Toolbox expects (`main` and `icon` are relative to the dist root).
`npm publish` is run **from `dist/`**.

## Publish to the public Power Platform Toolbox catalog

The catalog is npm-based: publish to npm, then submit the npm package name to the registry.

**1. Public GitHub mirror** — the catalog review requires a publicly accessible repo.
Create `https://github.com/TheMarkChristie/pptb-audit-restore` (public) and push this folder:
```bash
git init && git add . && git commit -m "Audit Restore PPTB tool"
git branch -M main
git remote add origin https://github.com/TheMarkChristie/pptb-audit-restore.git
git push -u origin main
```

**2. Build + validate**
```bash
node build.js
npx pptb-validate            # checks package.json against the review rules
```

**3. Publish to npm** (`npm login` first — no org needed for an unscoped name)
```bash
cd dist
npm publish
```

**4. Smoke-test the published build** — Toolbox → **Debug** → **Install from npm** →
`proximo3-audit-restore`. Pick an environment connection; the header should show the
connected environment + org version.

**5. Submit to the catalog** — [Tool Submission Form](https://www.powerplatformtoolbox.com/submit-tool):
npm package name `proximo3-audit-restore`, categories **Data**, **Troubleshooting**, **Migration**.
Automated checks run, then manual review (~48–72h).

> Canonical source lives in the private Azure DevOps **Proximo 3 Core** repo; the GitHub repo above
> is the public mirror required for catalog listing.

## Notes / limitations under Toolbox

- **Formatted value labels:** if the host's `queryData` doesn't return
  `odata.include-annotations`, the preview shows raw values (e.g. option-set integers, lookup
  GUIDs) instead of friendly labels. The restore itself is unaffected — `lookuplogicalname`
  (needed to rebind lookups) is returned by default.
- **`cspExceptions`:** not required, because the tool calls Dataverse only through the injected
  `dataverseAPI` (no direct cross-origin requests). If you later add direct calls, declare the
  origins under `cspExceptions` in `package.json`.
- Everything else (point-in-time reconstruction, scope, exclude-recently-reverted, pagination,
  Apply this page / Apply all, record deep-links) works identically to the web-resource build.

See `../audit-restore-guide.md` for the full functional guide.
