using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Validation;

/// <summary>
/// Detects problems in configurations before they reach setup.
/// </summary>
public interface IConfigurationValidator {

	/// <summary>
	/// Lists the problems found in a configuration, or an empty list when there are none.
	/// </summary>
	IReadOnlyList<ValidationIssue> Validate(OfficeConfiguration configuration);

}
