using System.Net.Http;

namespace O365ODTTool.Core.Setup;

/// <summary>
/// Provides the Office Deployment Tool setup executable, downloading it from the official content delivery network when missing.
/// </summary>
public sealed class SetupProvider(HttpClient httpClient, ISignatureVerifier signatureVerifier) : ISetupProvider {

	/// <summary>The setup executable file name.</summary>
	public const string SetupFileName = "setup.exe";

	/// <summary>The publisher expected in the setup executable signature.</summary>
	public const string ExpectedPublisher = "Microsoft Corporation";

	/// <summary>The official setup executable location.</summary>
	public static readonly Uri DownloadUri = new("https://officecdn.microsoft.com/pr/wsus/setup.exe");

	private const string partialFileExtension = ".download";
	private const int copyBufferSize = 81920;

	/// <inheritdoc />
	public string GetSetupPath(string folder) => Path.Combine(folder, SetupFileName);

	/// <inheritdoc />
	public async Task<string> EnsureSetupAsync(string folder, IProgress<string> log, CancellationToken cancellationToken) {

		var setupPath = GetSetupPath(folder);

		if (File.Exists(setupPath)) {

			EnsureTrusted(setupPath);
			log.Report($"Using existing {setupPath}");

			return setupPath;

		}

		log.Report($"Downloading {SetupFileName} from {DownloadUri}");

		var partialPath = setupPath + partialFileExtension;

		try {

			await DownloadAsync(partialPath, cancellationToken).ConfigureAwait(false);
			EnsureTrusted(partialPath);
			File.Move(partialPath, setupPath);

		} catch (Exception exception) when (exception is HttpRequestException or IOException or UnauthorizedAccessException) {

			throw new SetupException($"Unable to download {SetupFileName}: {exception.Message}", exception);

		} finally {

			if (File.Exists(partialPath)) File.Delete(partialPath);

		}

		log.Report($"Downloaded {setupPath}");

		return setupPath;

	}

	private async Task DownloadAsync(string path, CancellationToken cancellationToken) {

		using var response = await httpClient.GetAsync(DownloadUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

		response.EnsureSuccessStatusCode();

		using var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var target = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, copyBufferSize, useAsync: true);

		await source.CopyToAsync(target, copyBufferSize, cancellationToken).ConfigureAwait(false);

	}

	private void EnsureTrusted(string path) {

		if (signatureVerifier.IsSignedBy(path, ExpectedPublisher)) return;

		throw new SetupException($"'{path}' is not signed by {ExpectedPublisher}; delete it and try again.");

	}

}
