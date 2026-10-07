using O365ODTTool.Core.Setup;

namespace O365ODTTool.Core.Tests.Setup;

public sealed class AuthenticodeSignatureVerifierTests {

	private readonly AuthenticodeSignatureVerifier verifier = new();

	[Fact]
	public void IsSignedBy_AcceptsFileSignedByPublisher() {

		var path = typeof(object).Assembly.Location;

		Assert.True(verifier.IsSignedBy(path, SetupProvider.ExpectedPublisher));

	}

	[Fact]
	public void IsSignedBy_RejectsOtherPublisher() {

		var path = typeof(object).Assembly.Location;

		Assert.False(verifier.IsSignedBy(path, "Foo"));

	}

	[Fact]
	public void IsSignedBy_RejectsUnsignedFile() {

		using var folder = new TemporaryFolder();
		var path = folder.Combine("unsigned.exe");

		File.WriteAllBytes(path, [0x4D, 0x5A]);

		Assert.False(verifier.IsSignedBy(path, SetupProvider.ExpectedPublisher));

	}

	[Fact]
	public void IsSignedBy_RejectsMissingFile() => Assert.False(verifier.IsSignedBy(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")), SetupProvider.ExpectedPublisher));

}
