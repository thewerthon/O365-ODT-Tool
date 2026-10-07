# O365-ODT-Tool

A single-file Windows app that configures, downloads, and installs Microsoft 365 Apps through the Office Deployment Tool (ODT), with no need for the web customization tool, manual XML editing, or manual `setup.exe` calls.

## Features

All deployment choices sit in one form window that follows the Windows light or dark theme:

- **Form**: architecture, products (suite, Visio, Project), channel and version, applications, languages and proofing tools, installation, updates, licensing, and general options.
- **Import**: loads any ODT configuration XML into the form. App preferences (`AppSettings`) and any other unsupported content are preserved and passed to setup.
- **Auto-import**: loads `O365ODTTool.xml` from the executable folder at startup.
- **Export**: saves the configuration XML anywhere or as `O365ODTTool.xml` next to the executable.
- **Download**: runs `setup.exe /download` into a folder and leaves `setup.exe` and `configuration.xml` there for offline installation.
- **Install**: runs `setup.exe /configure` from the Office CDN.
- **Setup**: downloads the official `setup.exe` next to the executable when missing and accepts only files signed by Microsoft.

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
