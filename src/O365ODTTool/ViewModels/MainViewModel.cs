using System.Diagnostics;
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

	private const string exportFileName = "configuration.xml";
	private const string downloadTitle = "Download";
	private const string installTitle = "Install";
	private const string importTitle = "Import";
	private const string exportTitle = "Export";
	private const string saveTitle = "Save";
	private const string previewTitle = "Preview";
	private const string invalidTitle = "Invalid configuration";
	private const string downloadingText = "Downloading…";
	private const string installingText = "Installing…";
	private const string cancelingText = "Canceling…";
	private const string downloadedMessage = "Download completed. Installations will reuse the downloaded files.";
	private const string installedMessage = "Installation completed.";
	private const string savedMessage = "Saved to {0}";

	private readonly IConfigurationReader reader;
	private readonly IConfigurationWriter writer;
	private readonly IConfigurationValidator validator;
	private readonly IDeploymentOptionsMapper mapper;
	private readonly IDeploymentService deploymentService;
	private readonly IDialogService dialogs;
	private readonly AppPaths paths;
	private readonly IProgress<string> setupLog = new Progress<string>(static message => Trace.WriteLine(message));

	private OfficeConfiguration? baseConfiguration;
	private CancellationTokenSource? cancellation;

	/// <summary>
	/// Initializes the view model with its services.
	/// </summary>
	public MainViewModel(IConfigurationReader reader, IConfigurationWriter writer, IConfigurationValidator validator, IDeploymentOptionsMapper mapper, IDeploymentService deploymentService, IDialogService dialogs, AppPaths paths) {

		this.reader = reader;
		this.writer = writer;
		this.validator = validator;
		this.mapper = mapper;
		this.deploymentService = deploymentService;
		this.dialogs = dialogs;
		this.paths = paths;

		Form.Changed += (_, _) => UpdateValidation();

	}

	/// <summary>Gets the editable deployment choices.</summary>
	public DeploymentFormViewModel Form { get; } = new();

	/// <summary>Gets the validation summary of the current choices.</summary>
	[ObservableProperty]
	public partial string ValidationMessage { get; private set; } = string.Empty;

	/// <summary>Gets whether the current choices have validation issues.</summary>
	[ObservableProperty]
	public partial bool HasValidationIssues { get; private set; }

	/// <summary>Gets the severity of the validation issues.</summary>
	[ObservableProperty]
	public partial InfoBarSeverity ValidationSeverity { get; private set; }

	/// <summary>Gets the description of the running operation.</summary>
	[ObservableProperty]
	public partial string BusyText { get; private set; } = string.Empty;

	/// <summary>Gets whether a setup operation is running.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsIdle), nameof(CanCancel))]
	[NotifyCanExecuteChangedFor(nameof(ImportCommand), nameof(ExportCommand), nameof(PreviewCommand), nameof(SaveCommand), nameof(ResetCommand), nameof(DownloadCommand), nameof(InstallCommand), nameof(CancelCommand))]
	public partial bool IsBusy { get; private set; }

	/// <summary>Gets whether cancellation of the running operation was requested.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CanCancel))]
	[NotifyCanExecuteChangedFor(nameof(CancelCommand))]
	public partial bool IsCanceling { get; private set; }

	/// <summary>Gets whether no setup operation is running.</summary>
	public bool IsIdle => !IsBusy;

	/// <summary>Gets whether the running operation can be cancelled.</summary>
	public bool CanCancel => IsBusy && !IsCanceling;

	/// <summary>
	/// Loads the automatic import file when present, or the default choices otherwise.
	/// </summary>
	public void Initialize() {

		if (File.Exists(paths.AutoImportPath)) {

			TryImport(paths.AutoImportPath);

			return;

		}

		UpdateValidation();

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private void Import() {

		var path = dialogs.OpenXmlFile(paths.ToolFolder);

		if (path is not null) TryImport(path);

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private async Task ExportAsync() {

		var path = dialogs.SaveXmlFile(paths.ToolFolder, exportFileName);

		if (path is not null) await SaveToAsync(path, exportTitle);

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private Task PreviewAsync() => dialogs.ShowTextAsync(previewTitle, writer.Write(BuildConfiguration()));

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private Task SaveAsync() => SaveToAsync(paths.AutoImportPath, saveTitle);

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private void Reset() {

		baseConfiguration = null;
		Form.Load(new DeploymentOptions());

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private async Task DownloadAsync() {

		var configuration = await ValidateAsync(downloadTitle);

		if (configuration is null) return;

		await RunSetupAsync(downloadTitle, downloadingText, downloadedMessage, token => deploymentService.DownloadAsync(configuration, setupLog, token));

	}

	[RelayCommand(CanExecute = nameof(IsIdle))]
	private async Task InstallAsync() {

		var configuration = await ValidateAsync(installTitle);

		if (configuration is null) return;

		if (!await dialogs.ConfirmAsync(installTitle, "Office will be installed or reconfigured on this computer. Continue?", installTitle)) return;

		await RunSetupAsync(installTitle, installingText, installedMessage, token => deploymentService.InstallAsync(configuration, setupLog, token));

	}

	[RelayCommand(CanExecute = nameof(CanCancel))]
	private void Cancel() {

		BusyText = cancelingText;
		IsCanceling = true;

		cancellation?.Cancel();

	}

	private OfficeConfiguration BuildConfiguration() => mapper.ToConfiguration(Form.ToOptions(), baseConfiguration);

	private void TryImport(string path) {

		try {

			baseConfiguration = reader.ReadFile(path);
			Form.Load(mapper.ToOptions(baseConfiguration));

		} catch (ConfigurationFormatException exception) {

			_ = dialogs.ShowMessageAsync(importTitle, exception.Message);

		}

	}

	private async Task SaveToAsync(string path, string title) {

		var configuration = await ValidateAsync(title);

		if (configuration is null) return;

		try {

			writer.WriteFile(configuration, path);

			await dialogs.ShowMessageAsync(title, string.Format(savedMessage, path));

		} catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) {

			await dialogs.ShowMessageAsync(title, exception.Message);

		}

	}

	private async Task<OfficeConfiguration?> ValidateAsync(string title) {

		var configuration = BuildConfiguration();
		var errors = validator.Validate(configuration).Where(static issue => issue.Severity == Core.Validation.ValidationSeverity.Error).Select(static issue => issue.Message).ToList();

		if (errors.Count == 0) return configuration;

		await dialogs.ShowMessageAsync($"{title}: {invalidTitle}", string.Join(Environment.NewLine, errors));

		return null;

	}

	private async Task RunSetupAsync(string title, string busyText, string successMessage, Func<CancellationToken, Task<int>> operation) {

		IsBusy = true;
		BusyText = busyText;
		cancellation = new CancellationTokenSource();

		try {

			var exitCode = await operation(cancellation.Token);

			await dialogs.ShowMessageAsync(title, exitCode == SetupExitCodes.Success ? successMessage : SetupExitCodes.Describe(exitCode));

		} catch (OperationCanceledException) {

			await dialogs.ShowMessageAsync(title, $"{title} cancelled.");

		} catch (Exception exception) when (exception is SetupException or IOException or UnauthorizedAccessException) {

			await dialogs.ShowMessageAsync(title, exception.Message);

		} finally {

			cancellation.Dispose();
			cancellation = null;
			BusyText = string.Empty;
			IsCanceling = false;
			IsBusy = false;

		}

	}

	private void UpdateValidation() {

		var issues = validator.Validate(BuildConfiguration());

		HasValidationIssues = issues.Count > 0;
		ValidationMessage = string.Join(Environment.NewLine, issues.Select(static issue => issue.Message));
		ValidationSeverity = issues.Any(static issue => issue.Severity == Core.Validation.ValidationSeverity.Error) ? InfoBarSeverity.Error : InfoBarSeverity.Warning;

	}

}
