using System.Windows.Controls;
using O365ODTTool.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace O365ODTTool.Views;

/// <summary>
/// Shows every deployment choice in a single form.
/// </summary>
public partial class MainWindow : FluentWindow {

	/// <summary>
	/// Initializes the window bound to its view model, following the system theme.
	/// </summary>
	public MainWindow(MainViewModel viewModel) {

		DataContext = viewModel;

		ApplicationThemeManager.ApplySystemTheme();
		InitializeComponent();
		SystemThemeWatcher.Watch(this, WindowBackdropType.Mica, true);

	}

	private void OnLogTextChanged(object sender, TextChangedEventArgs e) => LogTextBox.ScrollToEnd();

}
