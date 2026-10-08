using O365ODTTool.Core.Deployment;

namespace O365ODTTool.Core.Tests.Deployment;

public sealed class SetupExitCodesTests {

	[Fact]
	public void Describe_SuccessOmitsCode() => Assert.Equal("Completed successfully.", SetupExitCodes.Describe(SetupExitCodes.Success));

	[Theory]
	[InlineData(17002)]
	[InlineData(17004)]
	[InlineData(12345)]
	public void Describe_FailureIncludesCode(int exitCode) => Assert.EndsWith($"(error {exitCode})", SetupExitCodes.Describe(exitCode), StringComparison.Ordinal);

}
