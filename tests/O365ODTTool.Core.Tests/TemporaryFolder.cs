namespace O365ODTTool.Core.Tests;

internal sealed class TemporaryFolder : IDisposable {

	public TemporaryFolder() {

		Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "O365ODTTool.Tests", Guid.NewGuid().ToString("N"));

		Directory.CreateDirectory(Path);

	}

	public string Path { get; }

	public string Combine(params string[] parts) => System.IO.Path.Combine([Path, .. parts]);

	public void Dispose() {

		if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);

	}

}
