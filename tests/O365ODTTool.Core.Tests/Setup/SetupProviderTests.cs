using System.Net;
using System.Net.Http;
using O365ODTTool.Core.Setup;

namespace O365ODTTool.Core.Tests.Setup;

public sealed class SetupProviderTests {

	private static readonly byte[] content = [1, 2, 3];

	[Fact]
	public async Task EnsureSetupAsync_UsesExistingTrustedFile() {

		using var folder = new TemporaryFolder();
		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, content);
		var provider = new SetupProvider(new HttpClient(handler), new FakeSignatureVerifier(trusted: true));

		File.WriteAllBytes(folder.Combine(SetupProvider.SetupFileName), content);

		var path = await provider.EnsureSetupAsync(folder.Path, new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(folder.Combine(SetupProvider.SetupFileName), path);
		Assert.Equal(0, handler.RequestCount);

	}

	[Fact]
	public async Task EnsureSetupAsync_RejectsExistingUntrustedFile() {

		using var folder = new TemporaryFolder();
		var provider = new SetupProvider(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, content)), new FakeSignatureVerifier(trusted: false));

		File.WriteAllBytes(folder.Combine(SetupProvider.SetupFileName), content);

		await Assert.ThrowsAsync<SetupException>(() => provider.EnsureSetupAsync(folder.Path, new SynchronousProgress(), CancellationToken.None));

	}

	[Fact]
	public async Task EnsureSetupAsync_DownloadsMissingFile() {

		using var folder = new TemporaryFolder();
		var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, content);
		var verifier = new FakeSignatureVerifier(trusted: true);
		var provider = new SetupProvider(new HttpClient(handler), verifier);

		var path = await provider.EnsureSetupAsync(folder.Path, new SynchronousProgress(), CancellationToken.None);

		Assert.Equal(content, File.ReadAllBytes(path));
		Assert.Equal(1, handler.RequestCount);
		Assert.Single(verifier.VerifiedPaths);
		Assert.Single(Directory.GetFiles(folder.Path));

	}

	[Fact]
	public async Task EnsureSetupAsync_DiscardsUntrustedDownload() {

		using var folder = new TemporaryFolder();
		var provider = new SetupProvider(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, content)), new FakeSignatureVerifier(trusted: false));

		await Assert.ThrowsAsync<SetupException>(() => provider.EnsureSetupAsync(folder.Path, new SynchronousProgress(), CancellationToken.None));

		Assert.Empty(Directory.GetFiles(folder.Path));

	}

	[Fact]
	public async Task EnsureSetupAsync_WrapsHttpFailures() {

		using var folder = new TemporaryFolder();
		var provider = new SetupProvider(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.NotFound, [])), new FakeSignatureVerifier(trusted: true));

		await Assert.ThrowsAsync<SetupException>(() => provider.EnsureSetupAsync(folder.Path, new SynchronousProgress(), CancellationToken.None));

		Assert.Empty(Directory.GetFiles(folder.Path));

	}

}
