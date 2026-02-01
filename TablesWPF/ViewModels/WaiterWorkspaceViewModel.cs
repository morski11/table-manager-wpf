using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Text.Json;
using System.IO;
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
    private Order? _selectedOrder;
    private OrderItem? _selectedOrderItem;
    private Product? _selectedProduct;

    /// <summary>All tables in the workspace.</summary>
    public ObservableCollection<Table> Tables { get; } = new();

    /// <summary>Available products loaded from JSON.</summary>
    public ObservableCollection<Product> Products { get; } = new();

    /// <summary>Currently selected product in the product picker.</summary>
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (_selectedProduct == value) return;
            _selectedProduct = value;
            OnPropertyChanged(nameof(SelectedProduct));
        }
    }

    /// <summary>Currently selected order (from the selected table).</summary>
    public Order? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (_selectedOrder == value) return;
            _selectedOrder = value;
            OnPropertyChanged(nameof(SelectedOrder));
        }
    }

    /// <summary>Currently selected order item (row selection).</summary>
    public OrderItem? SelectedOrderItem
    {
        get => _selectedOrderItem;
        set
        {
            if (_selectedOrderItem == value) return;
            _selectedOrderItem = value;
            OnPropertyChanged(nameof(SelectedOrderItem));
        }
    }

    /// <summary>Currently selected table (null if none).</summary>
    public Table? SelectedTable
    {
        get => _selectedTable;
        set
        {
            if (_selectedTable == value) return;
            _selectedTable = value;
            // When a table is selected, ensure SelectedOrder points to its single order (if any)
            SelectedOrder = _selectedTable?.Orders.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedTable));
        }
    }

    /// <summary>Command to add a new table with a user-specified name.</summary>
    public ICommand CreateTableCommand { get; }

    /// <summary>Command to rename the selected table.</summary>
    public ICommand EditTableCommand { get; }

    /// <summary>Command to delete the selected table.</summary>
    public ICommand DeleteTableCommand { get; }
    
    /// <summary>Adds selected product to the selected order.</summary>
    public ICommand AddOrderItemCommand { get; }

    /// <summary>Removes the selected order item from the selected order.</summary>
    public ICommand RemoveOrderItemCommand { get; }

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
        AddOrderItemCommand = new RelayCommand(AddOrderItem);
        RemoveOrderItemCommand = new RelayCommand(RemoveOrderItem);

        _ = LoadProductsAsync();
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

    private async Task LoadProductsAsync()
    {
        try
        {
            var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");
            if (!File.Exists(dataPath))
            {
                // Try relative path in development
                dataPath = Path.Combine(Directory.GetCurrentDirectory(), "TablesWPF", "Data", "products.json");
            }

            if (!File.Exists(dataPath)) return;

            using var stream = File.OpenRead(dataPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var docs = await JsonSerializer.DeserializeAsync<Product[]>(stream, options);
            if (docs == null) return;

            Products.Clear();
            foreach (var p in docs) Products.Add(p);
            SelectedProduct = Products.FirstOrDefault();
        }
        catch
        {
            // swallow load errors for now
        }
    }

    private void AddOrderItem()
    {
        if (SelectedProduct == null)
        {
            _dialogService.ShowMessage("No product selected", "Please select a product to add.");
            return;
        }

        // If there's no selected order, create one for the current table.
        if (SelectedOrder == null)
        {
            if (SelectedTable == null)
            {
                _dialogService.ShowMessage("No table selected", "Please select a table before adding items.");
                return;
            }

            // Enforce single-order-per-table: create an order only if the table has none.
            if (!SelectedTable.Orders.Any())
            {
                var order = new Order();
                SelectedTable.Orders.Add(order);
                SelectedOrder = order;
            }
            else
            {
                SelectedOrder = SelectedTable.Orders.First();
            }
        }

        var item = new OrderItem
        {
            Name = SelectedProduct.Name,
            Price = SelectedProduct.Price,
            Quantity = 1
        };

        SelectedOrder.OrderItems.Add(item);
        SelectedOrderItem = item;
    }

    private void RemoveOrderItem()
    {
        if (SelectedOrder == null || SelectedOrderItem == null) return;
        SelectedOrder.OrderItems.Remove(SelectedOrderItem);
        SelectedOrderItem = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
