using O365ODTTool.Core.Configuration;

namespace O365ODTTool.Core.Options;

/// <summary>
/// Converts between configurations and the deployment choices editable by the user.
/// </summary>
public interface IDeploymentOptionsMapper {

	/// <summary>
	/// Extracts the editable choices from a configuration.
	/// </summary>
	DeploymentOptions ToOptions(OfficeConfiguration configuration);

	/// <summary>
	/// Builds a configuration from the editable choices, preserving every non-editable content of the base configuration.
	/// </summary>
	OfficeConfiguration ToConfiguration(DeploymentOptions options, OfficeConfiguration? baseConfiguration = null);

	/// <summary>
	/// Determines whether a product is represented by the editable choices.
	/// </summary>
	bool IsManagedProduct(string productId);

}
