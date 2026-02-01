using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TablesWPF.Models;
using TablesWPF.Services;

namespace TablesWPF.ViewModels;

/// <summary>
/// ViewModel for the waiter workspace: tables list, selection, and table management actions.
/// </summary>
public class WaiterWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly IDialogService _dialogService;
    private Table? _selectedTable;

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

    /// <summary>Command to add a new table with a user-specified name.</summary>
    public ICommand CreateTableCommand { get; }

    /// <summary>Command to rename the selected table.</summary>
    public ICommand EditTableCommand { get; }

    /// <summary>Command to delete the selected table.</summary>
    public ICommand DeleteTableCommand { get; }

    /// <summary>
    /// Creates the ViewModel with the specified dialog service.
    /// </summary>
    /// <param name="dialogService">Dialog service for user prompts.</param>
    public WaiterWorkspaceViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        CreateTableCommand = new RelayCommand(CreateTable);
        EditTableCommand = new RelayCommand(EditTable, () => SelectedTable != null);
        DeleteTableCommand = new RelayCommand(DeleteTable, () => SelectedTable != null);
    }

    /// <summary>
    /// Prompts for a name and adds a new table.
    /// </summary>
    private void CreateTable()
    {
        if (_dialogService.TryGetInput("New Table", "Table name:", null, out var name))
        {
            if (TableNameExists(name))
            {
                _dialogService.ShowMessage("Duplicate Name", $"A table named \"{name}\" already exists.");
                return;
            }

            var table = new Table { Name = name };
            Tables.Add(table);
            SelectedTable = table;
        }
    }

    /// <summary>
    /// Prompts for a new name and renames the selected table.
    /// </summary>
    private void EditTable()
    {
        if (SelectedTable == null) return;

        if (_dialogService.TryGetInput("Rename Table", "Table name:", SelectedTable.Name, out var name))
        {
            if (TableNameExists(name, SelectedTable))
            {
                _dialogService.ShowMessage("Duplicate Name", $"A table named \"{name}\" already exists.");
                return;
            }

            SelectedTable.Name = name;
        }
    }

    /// <summary>
    /// Checks if a table with the given name already exists.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <param name="excludeTable">Optional table to exclude from the check (for rename scenarios).</param>
    /// <returns>True if a table with this name exists; false otherwise.</returns>
    private bool TableNameExists(string name, Table? excludeTable = null)
    {
        return Tables.Any(t => t != excludeTable && 
            string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Confirms and deletes the selected table.
    /// </summary>
    private void DeleteTable()
    {
        if (SelectedTable == null) return;

        if (_dialogService.Confirm("Delete Table", $"Delete \"{SelectedTable.Name}\" and all its orders?"))
        {
            Tables.Remove(SelectedTable);
            SelectedTable = Tables.Count > 0 ? Tables[0] : null;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
