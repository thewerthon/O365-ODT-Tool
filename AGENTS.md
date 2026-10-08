# O365-ODT-Tool

WPF app that edits Office Deployment Tool (ODT) configurations in a single form and runs `setup.exe /download` or `/configure`.

## Stack

- MUST target `net48` only; the executable MUST run without any runtime install on Windows 10 1903+ and Windows 11.
- MUST keep the output as one `O365ODTTool.exe`: dependencies are embedded by Costura.Fody, and no `.exe.config` is generated.
- MUST keep `requireAdministrator` in `src/O365ODTTool/app.manifest`.
- Modern C# is enabled through `LangVersion=latest` and PolySharp; shared build settings live in `Directory.Build.props`, and warnings are errors.
- The SDK is pinned by `global.json`.

## Structure

- **`src/O365ODTTool.Core`**: all logic, with no WPF dependency.
  - **`Configuration`**: `OfficeConfiguration` is a typed facade over the XML element tree. Unknown elements and attributes MUST survive import and export.
  - **`Catalogs`**: products, channels, apps, and languages from the official ODT documentation.
  - **`Options`**: `DeploymentOptions` holds the form choices. `DeploymentOptionsMapper` converts between those choices and a configuration, applying them over the imported base configuration.
  - **`Validation`**: rules checked before export, download, and install.
  - **`Xml`**: reader (DTD prohibited) and writer.
  - **`Setup`**: `setup.exe` download with Authenticode verification, and the process runner.
  - **`Deployment`**: orchestrates the `/download` and `/configure` runs.
- **`src/O365ODTTool`**: MVVM with CommunityToolkit.Mvvm and the WPF-UI theme.
  - **Composition root**: `App.xaml.cs` creates the services by hand, without a DI container.
  - **`MainViewModel`**: coordinates files, setup runs, and dialogs.
  - **`DeploymentFormViewModel`**: holds the form fields.
  - **`Controls`**: reusable controls, such as the multi-select drop-down.
  - **`Assets`**: the application icon.
- **`tests/O365ODTTool.Core.Tests`**: xUnit tests for the core library. Fixtures live in `Fixtures/`.

## Rules

- MUST keep the user interface a single form with no wizard or multi-step flow.
- MUST preserve every option not shown in the form (for example `AppSettings/User`, `Version`, `SourcePath`, `Logging`) exactly as imported. Only download overrides `SourcePath`, on a copy.
- MUST NOT expose implementation details (setup executable, CDN, temporary files) in labels, tooltips, or messages.
- MUST put new logic in Core with tests; view models only bind and coordinate.
- MUST validate with `dotnet test` and `dotnet publish src\O365ODTTool -c Release -o publish`.
