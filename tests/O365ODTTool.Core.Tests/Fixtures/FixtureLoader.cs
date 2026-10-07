namespace O365ODTTool.Core.Tests.Fixtures;

internal static class FixtureLoader {

	public const string OfficeCustomizationTool = "OfficeCustomizationTool.xml";

	public static string GetPath(string fileName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fixtures", fileName);

	public static string Read(string fileName) => File.ReadAllText(GetPath(fileName));

}
