namespace O365ODTTool.Services;

/// <summary>
/// Shows dialogs to the user.
/// </summary>
public interface IDialogService {

	/// <summary>
	/// Asks for an XML file to open, returning <see langword="null"/> when cancelled.
	/// </summary>
	string? OpenXmlFile(string initialFolder);

	/// <summary>
	/// Asks for an XML file to save, returning <see langword="null"/> when cancelled.
	/// </summary>
	string? SaveXmlFile(string initialFolder, string defaultFileName);

	/// <summary>
	/// Shows an informational message.
	/// </summary>
	Task ShowMessageAsync(string title, string message);

	/// <summary>
	/// Shows a read-only text document.
	/// </summary>
	Task ShowTextAsync(string title, string text);

	/// <summary>
	/// Asks the user to confirm an action.
	/// </summary>
	Task<bool> ConfirmAsync(string title, string message, string confirmText);

}
