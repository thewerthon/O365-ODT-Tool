using System.Collections;
using System.Windows;
using System.Windows.Controls;
using O365ODTTool.ViewModels;

namespace O365ODTTool.Controls;

/// <summary>
/// Shows a drop-down list of check boxes with a summary of the checked items.
/// </summary>
public partial class CheckComboBox : UserControl {

	/// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
	public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(CheckComboBox));

	/// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(CheckComboBox));

	/// <summary>
	/// Initializes the control.
	/// </summary>
	public CheckComboBox() => InitializeComponent();

	/// <summary>Gets or sets the selectable items.</summary>
	public IEnumerable? ItemsSource {
		get => (IEnumerable?)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	/// <summary>Gets or sets the summary shown while the list is closed.</summary>
	public string? Text {
		get => (string?)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	// Choosing a row outside its check box or with the keyboard toggles it, and the list never keeps a selected item, so only the summary is shown.
	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {

		if (Selector.SelectedItem is not SelectableItem item) return;

		item.IsSelected = !item.IsSelected;
		Selector.SelectedIndex = -1;

	}

}
