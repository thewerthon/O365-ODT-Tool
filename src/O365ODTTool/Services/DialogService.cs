using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;
using ControlAppearance = Wpf.Ui.Controls.ControlAppearance;

namespace O365ODTTool.Services;

/// <summary>
/// Shows dialogs to the user using the system file pickers and themed message boxes.
/// </summary>
public sealed class DialogService : IDialogService {

	private const string xmlFilter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
	private const string xmlExtension = ".xml";
	private const string closeText = "Close";
	private const string cancelText = "Cancel";
	private const string copyText = "Copy";
	private const string monospaceFonts = "Cascadia Mono, Consolas";
	private const double textDialogWidth = 820;
	private const double textViewerHeight = 480;
	private const double monospaceFontSize = 12;

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
	public async Task ShowTextAsync(string title, string text) {

		var viewer = new TextBox {
			Text = text,
			IsReadOnly = true,
			AcceptsReturn = true,
			TextWrapping = TextWrapping.NoWrap,
			FontFamily = new FontFamily(monospaceFonts),
			FontSize = monospaceFontSize,
			Height = textViewerHeight,
			VerticalContentAlignment = VerticalAlignment.Top,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
		};
		var messageBox = new MessageBox {
			Title = title,
			Content = viewer,
			Width = textDialogWidth,
			MaxWidth = textDialogWidth,
			PrimaryButtonText = copyText,
			CloseButtonText = closeText,
		};

		if (await messageBox.ShowDialogAsync() == MessageBoxResult.Primary) Clipboard.SetText(text);

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
