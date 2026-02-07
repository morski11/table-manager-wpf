using System.Windows.Controls;
using System.Windows.Input;
using TablesWPF.Services;
using TablesWPF.ViewModels;

namespace TablesWPF.Views;

/// <summary>
/// Waiter workspace view: create table, table list, and orders for selected table.
/// </summary>
public partial class WaiterWorkspaceView : UserControl
{
    public WaiterWorkspaceView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext == null)
            DataContext = new WaiterWorkspaceViewModel(new DialogService());
    }

    /// <summary>
    /// Opens the ComboBox dropdown when the user starts typing directly in the field.
    /// </summary>
    private void ProductComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            comboBox.IsDropDownOpen = true;
        }
    }
}
