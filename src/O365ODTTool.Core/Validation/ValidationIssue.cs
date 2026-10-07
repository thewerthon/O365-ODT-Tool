namespace O365ODTTool.Core.Validation;

/// <summary>
/// Represents a problem found in a configuration.
/// </summary>
/// <param name="Severity">The issue severity.</param>
/// <param name="Message">The issue description.</param>
public sealed record ValidationIssue(ValidationSeverity Severity, string Message) {

	/// <inheritdoc />
	public override string ToString() => $"{Severity}: {Message}";

}
