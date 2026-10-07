using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Tests.Configuration;

public sealed class OdtBooleanTests {

	[Theory]
	[InlineData("TRUE", true)]
	[InlineData("true", true)]
	[InlineData(" 1 ", true)]
	[InlineData("FALSE", false)]
	[InlineData("False", false)]
	[InlineData("0", false)]
	[InlineData("yes", null)]
	[InlineData(null, null)]
	public void Parse_ReturnsExpectedValue(string? value, bool? expected) => Assert.Equal(expected, OdtBoolean.Parse(value));

	[Fact]
	public void Format_ReturnsTextualValues() {

		Assert.Equal("TRUE", OdtBoolean.Format(true));
		Assert.Equal("FALSE", OdtBoolean.Format(false));

	}

	[Fact]
	public void FormatNumeric_ReturnsNumericValues() {

		Assert.Equal("1", OdtBoolean.FormatNumeric(true));
		Assert.Equal("0", OdtBoolean.FormatNumeric(false));

	}

}
