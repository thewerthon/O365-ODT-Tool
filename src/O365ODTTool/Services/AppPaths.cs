namespace O365ODTTool.Services;

/// <summary>
/// Provides the file system locations used by the application.
/// </summary>
/// <param name="toolFolder">The folder that contains the application executable.</param>
public sealed class AppPaths(string toolFolder) {

	/// <summary>The configuration file name imported automatically at startup.</summary>
	public const string AutoImportFileName = "O365ODTTool.xml";

	private const string defaultDownloadFolderName = "Office365";
	private const string temporaryFolderName = "O365ODTTool";

	/// <summary>Gets the folder that contains the application executable.</summary>
	public string ToolFolder { get; } = Path.GetFullPath(toolFolder);

	/// <summary>Gets the configuration file path imported automatically at startup.</summary>
	public string AutoImportPath => Path.Combine(ToolFolder, AutoImportFileName);

	/// <summary>Gets the default download folder.</summary>
	public string DefaultDownloadFolder => Path.Combine(ToolFolder, defaultDownloadFolderName);

	/// <summary>Gets the folder for transient files.</summary>
	public string TemporaryFolder { get; } = Path.Combine(Path.GetTempPath(), temporaryFolderName);

}
