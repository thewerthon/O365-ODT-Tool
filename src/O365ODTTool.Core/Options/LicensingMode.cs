namespace O365ODTTool.Core.Options;

/// <summary>
/// Defines the activation modes.
/// </summary>
public enum LicensingMode {

	/// <summary>Activation tied to the signed in user.</summary>
	UserBased,

	/// <summary>Activation for computers shared by multiple users.</summary>
	SharedComputer,

	/// <summary>Activation tied to the device.</summary>
	DeviceBased,

}
