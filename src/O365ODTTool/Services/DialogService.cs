using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace O365ODTTool.Services;

/// <summary>
/// Shows dialogs to the user using the system file pickers and themed message boxes.
/// </summary>
public sealed class DialogService : IDialogService {

	private const string xmlFilter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
	private const string xmlExtension = ".xml";
	private const string closeText = "Close";
	private const string cancelText = "Cancel";

	/// <inheritdoc />
	public string? OpenXmlFile(string initialFolder) {

		var dialog = new OpenFileDialog {
			Filter = xmlFilter,
			DefaultExt = xmlExtension,
			InitialDirectory = initialFolder,
			CheckFileExists = true,
		};

		return dialog.ShowDialog() == true ? dialog.FileName : null;

	}

	/// <inheritdoc />
	public string? SaveXmlFile(string initialFolder, string defaultFileName) {

		var dialog = new SaveFileDialog {
			Filter = xmlFilter,
			DefaultExt = xmlExtension,
			InitialDirectory = initialFolder,
			FileName = defaultFileName,
			OverwritePrompt = true,
		};

		return dialog.ShowDialog() == true ? dialog.FileName : null;

	}

	/// <inheritdoc />
	public string? PickFolder(string initialFolder) => FolderPicker.Pick(initialFolder);

	/// <inheritdoc />
	public async Task ShowMessageAsync(string title, string message) {

		var messageBox = new MessageBox {
			Title = title,
			Content = message,
			CloseButtonText = closeText,
		};

		await messageBox.ShowDialogAsync();

	}

	/// <inheritdoc />
	public async Task<bool> ConfirmAsync(string title, string message, string confirmText) {

		var messageBox = new MessageBox {
			Title = title,
			Content = message,
			PrimaryButtonText = confirmText,
			PrimaryButtonAppearance = ControlAppearance.Primary,
			CloseButtonText = cancelText,
		};

		var result = await messageBox.ShowDialogAsync();

		return result == MessageBoxResult.Primary;

	}

}
