using CommunityToolkit.Mvvm.ComponentModel;

namespace O365ODTTool.ViewModels;

/// <summary>
/// Represents a choice displayed as a check box.
/// </summary>
/// <param name="id">The choice identifier.</param>
/// <param name="displayName">The choice display name.</param>
public sealed partial class SelectableItem(string id, string displayName) : ObservableObject {

	/// <summary>Gets the choice identifier.</summary>
	public string Id { get; } = id;

	/// <summary>Gets the choice display name.</summary>
	public string DisplayName { get; } = displayName;

	/// <summary>Gets or sets whether the choice is selected.</summary>
	[ObservableProperty]
	public partial bool IsSelected { get; set; }

}
