using System.Windows.Controls;
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
            DataContext = new WaiterWorkspaceViewModel();
    }
}
