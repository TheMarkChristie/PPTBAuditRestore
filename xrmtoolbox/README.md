# Audit Restore — XrmToolBox plugin

*by Mark Christie*

The XrmToolBox build of **Audit Restore**. It's a `PluginControlBase` that hosts a **WebView2**
control and loads the *same* `audit-restore.html` used by the Power Platform ToolBox tool and the
D365 web resource — so there is one UI and one set of logic across all three hosts.

## How it works

- `AuditRestorePlugin.cs` — MEF registration (Name, Description, **Author = Mark Christie**, icons).
- `AuditRestoreControl.cs` — the WebView2 host. On every connection change it injects
  `window.XTB_CONFIG = { baseUrl, token }` (org URL + OAuth bearer token from the active
  connection), then loads `app/index.html`.
- The HTML detects `window.XTB_CONFIG`, switches to the **Windows 95** theme (`data-host="xtb"`),
  and calls the Dataverse Web API directly with the bearer token.
- `AuditRestore.csproj` copies `../../audit-restore.html` → `app/index.html` before each build, so
  the plugin always ships the current UI. No style-swap step is needed — the HTML self-themes.

## Build & debug

```powershell
cd xrmtoolbox/AuditRestore
dotnet build -c Debug
```

To debug, set the project's **Start external program** to `XrmToolBox.exe` in your build output and
pass `/overridepath:.` so it runs from the build folder. Press F5; open **Audit Restore** from the
tool list, connect to an OAuth/MFA org.

> Use an **OAuth/MFA connection** — the page needs `ServiceClient.CurrentAccessToken`. Connections
> without a bearer token will show the "no token" warning in the XrmToolBox log.

## Package for the Tool Library

```powershell
dotnet build -c Release
nuget pack AuditRestore.nuspec
```

Then publish the `.nupkg` to nuget.org and register the package id at
<https://www.xrmtoolbox.com/plugins/new/>. The `.nuspec` ships only `AuditRestore.dll` +
`app/index.html` into a `Plugins/` folder (XrmToolBox provides WebView2 + the Dataverse SDK).

## TODO before publishing

- Replace the placeholder base64 icons in `IconData.cs` with proper 32×32 + 80×80 PNG renders of
  `icon.svg`, and add `logo.png` to the repo root for the `.nuspec` `iconUrl`.
- Bump `<Version>` in both `AuditRestore.csproj` and `AuditRestore.nuspec` (must match) per release.
