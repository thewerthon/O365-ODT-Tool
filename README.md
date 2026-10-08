# O365-ODT-Tool

A single-file Windows app that configures, downloads, and installs Microsoft 365 Apps through the Office Deployment Tool (ODT), with no need for the web customization tool, manual XML editing, or manual `setup.exe` calls.

## Features

All deployment choices sit in one form window that follows the Windows light or dark theme:

- **Form**: architecture, products (Office, Visio, Project), applications, languages and proofing tools, update channel, installation, licensing, and general options.
- **Import**: loads any ODT configuration XML into the form. Options not shown in the form (app preferences, version, source path, and so on) are preserved and passed to setup.
- **Auto-import**: loads `O365ODTTool.xml` from the executable folder at startup.
- **Export, Preview, Save**: saves the configuration XML anywhere, shows it, or saves it as `O365ODTTool.xml` next to the executable.
- **Download**: asks for a folder, runs `setup.exe /download` into it, then adds `setup.exe`, `configuration.xml`, and `Install.cmd` so the folder installs offline from wherever it is copied.
- **Install**: runs `setup.exe /configure`. When the executable folder holds downloaded files (`Office\Data`), they are reused and only missing content comes from the Office CDN.
- **Setup**: uses `setup.exe` next to the executable when present; otherwise downloads the official one into `%TEMP%\O365ODTTool`. Only files signed by Microsoft are accepted.

The app stays open while setup runs, shows its result, and allows cancelling.

> [!NOTE]
> The executable requires administrator rights and the .NET Framework 4.8, which ships with Windows 10 (1903 or later) and Windows 11.

## Folder Structure

The solution has three projects:

- **`src/O365ODTTool.Core`**: configuration model, catalogs, XML, validation, and setup services.
- **`src/O365ODTTool`**: WPF user interface built with WPF-UI.
- **`tests/O365ODTTool.Core.Tests`**: xUnit tests for the core library.

## Building

Build, test, and publish the single executable to the `publish` folder:

```powershell
dotnet test
dotnet publish src\O365ODTTool -c Release -o publish
```
