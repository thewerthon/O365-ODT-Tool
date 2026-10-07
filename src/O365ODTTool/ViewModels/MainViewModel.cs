using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using O365ODTTool.Core.Configuration;
using O365ODTTool.Core.Deployment;
using O365ODTTool.Core.Options;
using O365ODTTool.Core.Setup;
using O365ODTTool.Core.Validation;
using O365ODTTool.Core.Xml;
using O365ODTTool.Services;
using Wpf.Ui.Controls;

namespace O365ODTTool.ViewModels;

/// <summary>
/// Coordinates the form, the configuration files and the setup operations.
/// </summary>
public sealed partial class MainViewModel : ObservableObject {

	private const string defaultsSource = "Defaults";
	private const string exportFileName = "configuration.xml";
	private const string downloadTitle = "Download";
	private const string installTitle = "Install";
	private const string importTitle = "Import XML";
	private const string exportTitle = "Export XML";
	private const string validationTitle = "Invalid configuration";
	private const string logTimeFormat = "HH:mm:ss";

	private readonly IConfigurationReader reader;
	private readonly IConfigurationWriter writer;
	private readonly IConfigurationValidator validator;
	private readonly IDeploymentOptionsMapper mapper;
	private readonly IDeploymentService deploymentService;
	private readonly ISetupProvider setupProvider;
	private readonly IDialogService dialogs;
	private readonly AppPaths paths;
	private readonly StringBuilder log = new();

	private OfficeConfiguration? baseConfiguration;
	private CancellationTokenSource? cancellation;

	/// <summary>
	/// Initializes the view model with its services.
	/// </summary>
	public MainViewModel(IConfigurationReader reader, IConfigurationWriter writer, IConfigurationValidator validator, IDeploymentOptionsMapper mapper, IDeploymentService deploymentService, ISetupProvider setupProvider, IDialogService dialogs, AppPaths paths) {

		this.reader = reader;
		this.writer = writer;
		this.validator = validator;
		this.mapper = mapper;
		this.deploymentService = deploymentService;
		this.setupProvider = setupProvider;
		this.dialogs = dialogs;
		this.paths = paths;

		DownloadFolder = paths.DefaultDownloadFolder;
		Form.Changed += (_, _) => UpdatePreview();

	}

	/// <summary>Gets the editable deployment choices.</summary>
	public DeploymentFormViewModel Form { get; } = new();

	/// <summary>Gets or sets the folder that receives downloaded installation files.</summary>
	[ObservableProperty]
	public partial string DownloadFolder { get; set; }

	/// <summary>Gets the XML generated from the current choices.</summary>
	[ObservableProperty]
	public partial string XmlPreview { get; private set; } = string.Empty;

	/// <summary>Gets the activity log.</summary>
	[ObservableProperty]
	public partial string LogText { get; private set; } = string.Empty;

	/// <summary>Gets the origin of the current configuration.</summary>
	[ObservableProperty]
	public partial string SourceDescription { get; private set; } = defaultsSource;

	/// <summary>Gets the setup executable status.</summary>
	[ObservableProperty]
	public partial string SetupStatus { get; private set; } = string.Empty;

	/// <summary>Gets the number of imported user settings preserved without being displayed.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasPreservedContent), nameof(PreservedSummary))]
	public partial int PreservedSettingsCount { get; private set; }

	/// <summary>Gets the imported products preserved without being displayed.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasPreservedContent), nameof(PreservedSummary))]
	public partial string PreservedProducts { get; private set; } = string.Empty;

	/// <summary>Gets a description of the imported content preserved without being displayed.</summary>
	public string PreservedSummary => PreservedProducts.Length == 0
		? $"Preserved from import: {PreservedSettingsCount} app preference(s)"
		: $"Preserved from import: {PreservedSettingsCount} app preference(s), other products: {PreservedProducts}";

	/// <summary>Gets the validation summary of the current choices.</summary>
	[ObservableProperty]
	public partial string ValidationMessage { get; private set; } = string.Empty;

	/// <summary>Gets whether the current choices have validation issues.</summary>
	[ObservableProperty]
	public partial bool HasValidationIssues { get; private set; }

	/// <summary>Gets the severity of the validation issues.</summary>
	[ObservableProperty]
	public partial InfoBarSeverity ValidationSeverity { get; private set; }

	/// <summary>Gets whether a setup operation is running.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsIdle))]
	[NotifyCanExecuteChangedFor(nameof(ImportCommand), nameof(ExportCommand), nameof(SaveAsAutoImportCommand), nameof(ResetCommand), nameof(BrowseDownloadFolderCommand), nameof(DownloadCommand), nameof(InstallCommand), nameof(CancelCommand))]
	public partial bool IsBusy { get; private set; }

	/// <summary>Gets whether no setup operation is running.</summary>
	public bool IsIdle => !IsBusy;

	/// <summary>Gets whether the imported configuration has content preserved without being displayed.</summary>
	public bool HasPreservedContent => PreservedSettingsCount > 0 || PreservedProducts.Length > 0;

	/// <summary>
	/// Loads the automatic import file when present, or the default choices otherwise.
	/// </summary>
	public void Initialize() {

		UpdateSetupStatus();

		if (!File.Exists(paths.AutoImportPath)) {

			AppendLog($"No {AppPaths.AutoImportFileName} found next to the tool; using defaults.");
			UpdatePreview();

			return;

		}

		TryImport(paths.AutoImportPath);

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private void Import() {

		var path = dialogs.OpenXmlFile(paths.ToolFolder);

		if (path is not null) TryImport(path);

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private async Task ExportAsync() {

		var path = dialogs.SaveXmlFile(paths.ToolFolder, exportFileName);

		if (path is not null) await SaveAsync(path, exportTitle);

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private Task SaveAsAutoImportAsync() => SaveAsync(paths.AutoImportPath, exportTitle);

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private void Reset() {

		baseConfiguration = null;
		DownloadFolder = paths.DefaultDownloadFolder;
		SourceDescription = defaultsSource;
		PreservedSettingsCount = 0;
		PreservedProducts = string.Empty;
		Form.Load(new DeploymentOptions());

		AppendLog("Reset to defaults.");

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private void BrowseDownloadFolder() {

		var folder = dialogs.PickFolder(Directory.Exists(DownloadFolder) ? DownloadFolder : paths.ToolFolder);

		if (folder is not null) DownloadFolder = folder;

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private Task DownloadAsync() {

		if (string.IsNullOrWhiteSpace(DownloadFolder)) return dialogs.ShowMessageAsync(downloadTitle, "Choose a download folder.");

		var folder = DownloadFolder;

		return RunSetupAsync(downloadTitle, (configuration, progress, token) => deploymentService.DownloadAsync(configuration, folder, progress, token));

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private async Task InstallAsync() {

		if (!await dialogs.ConfirmAsync(installTitle, "Office will be installed or reconfigured on this computer from the Office CDN. Continue?", installTitle)) return;

		await RunSetupAsync(installTitle, deploymentService.InstallAsync);

	}

	[RelayCommand(CanExecute = nameof(IsBusy))]
	private void Cancel() {

		cancellation?.Cancel();

		AppendLog("Cancellation requested.");

	}

	[RelayCommand]
	private void ClearLog() {

		log.Clear();

		LogText = string.Empty;

	}

	private OfficeConfiguration BuildConfiguration() => mapper.ToConfiguration(Form.ToOptions(), baseConfiguration);

	private void TryImport(string path) {

		try {

			var configuration = reader.ReadFile(path);

			LoadConfiguration(configuration, path);
			AppendLog($"Imported {path}");

		} catch (ConfigurationFormatException exception) {

			AppendLog(exception.Message);
			_ = dialogs.ShowMessageAsync(importTitle, exception.Message);

		}

	}

	private void LoadConfiguration(OfficeConfiguration configuration, string source) {

		baseConfiguration = configuration;
		SourceDescription = source;
		PreservedSettingsCount = configuration.AppSettings.Count(static setting => setting.Scope == AppSettingScope.User);
		PreservedProducts = string.Join(", ", configuration.Products.Where(product => !mapper.IsManagedProduct(product.Id)).Select(static product => product.Id));

		if (configuration.SourcePath is { Length: > 0 } sourcePath) {

			DownloadFolder = sourcePath;

			AppendLog($"Imported SourcePath '{sourcePath}' is used as the download folder; installation always uses the Office CDN.");

		}

		Form.Load(mapper.ToOptions(configuration));

	}

	private async Task SaveAsync(string path, string title) {

		var configuration = await ValidateAsync(title);

		if (configuration is null) return;

		try {

			writer.WriteFile(configuration, path);

			AppendLog($"Saved {path}");

		} catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) {

			AppendLog(exception.Message);

			await dialogs.ShowMessageAsync(title, exception.Message);

		}

	}

	private async Task<OfficeConfiguration?> ValidateAsync(string title) {

		var configuration = BuildConfiguration();
		var issues = validator.Validate(configuration);

		foreach (var issue in issues) AppendLog(issue.ToString());

		var errors = issues.Where(static issue => issue.Severity == Core.Validation.ValidationSeverity.Error).Select(static issue => issue.Message).ToList();

		if (errors.Count == 0) return configuration;

		await dialogs.ShowMessageAsync($"{title}: {validationTitle}", string.Join(Environment.NewLine, errors));

		return null;

	}

	private async Task RunSetupAsync(string title, Func<OfficeConfiguration, IProgress<string>, CancellationToken, Task<int>> operation) {

		var configuration = await ValidateAsync(title);

		if (configuration is null) return;

		IsBusy = true;
		cancellation = new CancellationTokenSource();

		var progress = new Progress<string>(AppendLog);

		try {

			var exitCode = await operation(configuration, progress, cancellation.Token);

			await dialogs.ShowMessageAsync(title, SetupExitCodes.Describe(exitCode));

		} catch (OperationCanceledException) {

			AppendLog($"{title} cancelled.");

		} catch (Exception exception) when (exception is SetupException or IOException or UnauthorizedAccessException) {

			AppendLog(exception.Message);

			await dialogs.ShowMessageAsync(title, exception.Message);

		} finally {

			cancellation.Dispose();
			cancellation = null;
			IsBusy = false;

			UpdateSetupStatus();

		}

	}

	private void UpdatePreview() {

		var configuration = BuildConfiguration();
		var issues = validator.Validate(configuration);

		XmlPreview = writer.Write(configuration);
		HasValidationIssues = issues.Count > 0;
		ValidationMessage = string.Join(Environment.NewLine, issues.Select(static issue => issue.Message));
		ValidationSeverity = issues.Any(static issue => issue.Severity == Core.Validation.ValidationSeverity.Error) ? InfoBarSeverity.Error : InfoBarSeverity.Warning;

	}

	private void UpdateSetupStatus() {

		var setupPath = setupProvider.GetSetupPath(paths.ToolFolder);

		SetupStatus = File.Exists(setupPath) ? $"{SetupProvider.SetupFileName} found next to the tool" : $"{SetupProvider.SetupFileName} will be downloaded on first use";

	}

	private void AppendLog(string message) {

		log.Append(DateTime.Now.ToString(logTimeFormat)).Append("  ").AppendLine(message);

		LogText = log.ToString();

	}

}
