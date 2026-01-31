using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TablesWPF.Models;

namespace TablesWPF.ViewModels;

/// <summary>
/// ViewModel for the waiter workspace: tables list, selection, and create-table action.
/// </summary>
public class WaiterWorkspaceViewModel : INotifyPropertyChanged
{
    private Table? _selectedTable;
    private int _tableCounter;

    /// <summary>All tables in the workspace.</summary>
    public ObservableCollection<Table> Tables { get; } = new();

    /// <summary>Currently selected table (null if none).</summary>
    public Table? SelectedTable
    {
        get => _selectedTable;
        set
        {
            if (_selectedTable == value) return;
            _selectedTable = value;
            OnPropertyChanged(nameof(SelectedTable));
        }
    }

    /// <summary>Command to add a new table.</summary>
    public ICommand CreateTableCommand { get; }

    public WaiterWorkspaceViewModel()
    {
        CreateTableCommand = new RelayCommand(CreateTable);
    }

    /// <summary>Adds a new table with a generated name (e.g. "Table 1", "Table 2").</summary>
    private void CreateTable()
    {
        _tableCounter++;
        var table = new Table { Name = $"Table {_tableCounter}" };
        Tables.Add(table);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
