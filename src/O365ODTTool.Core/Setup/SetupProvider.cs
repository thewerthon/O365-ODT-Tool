using System.Net.Http;

namespace O365ODTTool.Core.Setup;

/// <summary>
/// Provides the Office Deployment Tool setup executable, preferring a local copy and otherwise downloading it from the official content delivery network into a cache folder.
/// </summary>
/// <param name="httpClient">The client used to download the executable.</param>
/// <param name="signatureVerifier">The verifier of the executable signature.</param>
/// <param name="localFolder">The folder checked first for an existing executable.</param>
/// <param name="cacheFolder">The folder that receives the downloaded executable.</param>
public sealed class SetupProvider(HttpClient httpClient, ISignatureVerifier signatureVerifier, string localFolder, string cacheFolder) : ISetupProvider {

	/// <summary>The setup executable file name.</summary>
	public const string SetupFileName = "setup.exe";

	/// <summary>The publisher expected in the setup executable signature.</summary>
	public const string ExpectedPublisher = "Microsoft Corporation";

	/// <summary>The official setup executable location.</summary>
	public static readonly Uri DownloadUri = new("https://officecdn.microsoft.com/pr/wsus/setup.exe");

	private const string partialFileExtension = ".download";
	private const int copyBufferSize = 81920;

	/// <inheritdoc />
	public async Task<string> EnsureSetupAsync(IProgress<string> log, CancellationToken cancellationToken) {

		var localPath = Path.Combine(localFolder, SetupFileName);

		if (File.Exists(localPath)) return UseExisting(localPath, log);

		var cachedPath = Path.Combine(cacheFolder, SetupFileName);

		if (File.Exists(cachedPath)) return UseExisting(cachedPath, log);

		await DownloadToAsync(cachedPath, log, cancellationToken).ConfigureAwait(false);

		return cachedPath;

	}

	private string UseExisting(string path, IProgress<string> log) {

		EnsureTrusted(path);
		log.Report($"Using {path}");

		return path;

	}

	private async Task DownloadToAsync(string path, IProgress<string> log, CancellationToken cancellationToken) {

		log.Report($"Downloading {SetupFileName} from {DownloadUri}");

		var partialPath = path + partialFileExtension;

		try {

			Directory.CreateDirectory(cacheFolder);
			await DownloadAsync(partialPath, cancellationToken).ConfigureAwait(false);
			EnsureTrusted(partialPath);
			File.Move(partialPath, path);

		} catch (Exception exception) when (exception is HttpRequestException or IOException or UnauthorizedAccessException) {

			throw new SetupException($"Unable to download {SetupFileName}: {exception.Message}", exception);

		} finally {

			if (File.Exists(partialPath)) File.Delete(partialPath);

		}

		log.Report($"Downloaded {path}");

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
