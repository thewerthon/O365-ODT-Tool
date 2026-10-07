namespace O365ODTTool.Core.Validation;

/// <summary>
/// Defines the severities of a validation issue.
/// </summary>
public enum ValidationSeverity {

	/// <summary>The configuration works but may behave unexpectedly.</summary>
	Warning,

	/// <summary>The configuration is rejected or fails during setup.</summary>
	Error,

}
