using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using O365ODTTool.Core.Deployment;
using O365ODTTool.Core.Options;
using O365ODTTool.Core.Setup;
using O365ODTTool.Core.Validation;
using O365ODTTool.Core.Xml;
using O365ODTTool.Services;
using O365ODTTool.ViewModels;
using O365ODTTool.Views;

namespace O365ODTTool;

/// <summary>
/// Composes the services and shows the main window.
/// </summary>
public partial class App : Application {

	private const string unexpectedErrorTitle = "Unexpected error";

	private readonly HttpClient httpClient = new();

	/// <inheritdoc />
	protected override void OnStartup(StartupEventArgs e) {

		base.OnStartup(e);

		DispatcherUnhandledException += OnDispatcherUnhandledException;

		var paths = new AppPaths(AppDomain.CurrentDomain.BaseDirectory);
		var writer = new ConfigurationWriter();
		var setupProvider = new SetupProvider(httpClient, new AuthenticodeSignatureVerifier(), paths.ToolFolder, paths.TemporaryFolder);
		var deploymentService = new DeploymentService(setupProvider, new ProcessRunner(), writer, paths.TemporaryFolder);
		var viewModel = new MainViewModel(new ConfigurationReader(), writer, new ConfigurationValidator(), new DeploymentOptionsMapper(), deploymentService, new DialogService(), paths);
		var window = new MainWindow(viewModel);

		MainWindow = window;
		window.Show();
		viewModel.Initialize();

	}

	/// <inheritdoc />
	protected override void OnExit(ExitEventArgs e) {

		httpClient.Dispose();

		base.OnExit(e);

	}

	private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {

		System.Windows.MessageBox.Show(e.Exception.Message, unexpectedErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

		e.Handled = true;

	}

}
