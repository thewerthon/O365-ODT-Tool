using System.Net;
using System.Net.Http;
using O365ODTTool.Core.Setup;

namespace O365ODTTool.Core.Tests.Setup;

public sealed class SetupProviderTests : IDisposable {

	private static readonly byte[] content = [1, 2, 3];

	private readonly TemporaryFolder folder = new();

	[Fact]
	public async Task EnsureSetupAsync_PrefersTrustedLocalFile() {

		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, content);

		Directory.CreateDirectory(LocalFolder);
		File.WriteAllBytes(LocalSetupPath, content);

		var path = await CreateProvider(handler, trusted: true).EnsureSetupAsync(new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(LocalSetupPath, path);
		Assert.Equal(0, handler.RequestCount);

	}

	[Fact]
	public async Task EnsureSetupAsync_UsesCachedFile() {

		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, content);

		Directory.CreateDirectory(CacheFolder);
		File.WriteAllBytes(CachedSetupPath, content);

		var path = await CreateProvider(handler, trusted: true).EnsureSetupAsync(new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(CachedSetupPath, path);
		Assert.Equal(0, handler.RequestCount);

	}

	[Fact]
	public async Task EnsureSetupAsync_RejectsUntrustedLocalFile() {

		Directory.CreateDirectory(LocalFolder);
		File.WriteAllBytes(LocalSetupPath, content);

		var provider = CreateProvider(new FakeHttpMessageHandler(HttpStatusCode.OK, content), trusted: false);

		await Assert.ThrowsAsync<SetupException>(() => provider.EnsureSetupAsync(new SynchronousProgress(), CancellationToken.None));

	}

	[Fact]
	public async Task EnsureSetupAsync_DownloadsMissingFileIntoCache() {

		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, content);

		var path = await CreateProvider(handler, trusted: true).EnsureSetupAsync(new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(CachedSetupPath, path);
		Assert.Equal(content, File.ReadAllBytes(path));
		Assert.Equal(1, handler.RequestCount);
		Assert.Single(Directory.GetFiles(CacheFolder));
		Assert.False(Directory.Exists(LocalFolder));

	}

	[Fact]
	public async Task EnsureSetupAsync_DiscardsUntrustedDownload() {

		var provider = CreateProvider(new FakeHttpMessageHandler(HttpStatusCode.OK, content), trusted: false);

		await Assert.ThrowsAsync<SetupException>(() => provider.EnsureSetupAsync(new SynchronousProgress(), CancellationToken.None));

		Assert.Empty(Directory.GetFiles(CacheFolder));

	}

	[Fact]
	public async Task EnsureSetupAsync_WrapsHttpFailures() {

		var provider = CreateProvider(new FakeHttpMessageHandler(HttpStatusCode.NotFound, []), trusted: true);

		await Assert.ThrowsAsync<SetupException>(() => provider.EnsureSetupAsync(new SynchronousProgress(), CancellationToken.None));

		Assert.Empty(Directory.GetFiles(CacheFolder));

	}

	public void Dispose() => folder.Dispose();

	private string LocalFolder => folder.Combine("local");

	private string CacheFolder => folder.Combine("cache");

	private string LocalSetupPath => Path.Combine(LocalFolder, SetupProvider.SetupFileName);

	private string CachedSetupPath => Path.Combine(CacheFolder, SetupProvider.SetupFileName);

	private SetupProvider CreateProvider(FakeHttpMessageHandler handler, bool trusted) => new(new HttpClient(handler), new FakeSignatureVerifier(trusted), LocalFolder, CacheFolder);

}
