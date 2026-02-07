using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
    private string _productSearchText = string.Empty;
    private ICollectionView? _productsView;

    /// <summary>Open (active) tables in the workspace.</summary>
    public ObservableCollection<Table> OpenTables { get; } = new();

    /// <summary>Completed tables (read-only history).</summary>
    public ObservableCollection<Table> CompletedTables { get; } = new();

    private int _tablesTabIndex;

    /// <summary>Selected tab index for the tables view (0=open, 1=completed).</summary>
    public int TablesTabIndex
    {
        get => _tablesTabIndex;
        set
        {
            if (_tablesTabIndex == value) return;
            _tablesTabIndex = value;
            OnPropertyChanged(nameof(TablesTabIndex));

            if (value == 0)
            {
                if (SelectedTable == null || !OpenTables.Contains(SelectedTable))
                    SelectedTable = OpenTables.Count > 0 ? OpenTables[0] : null;
            }
            else if (value == 1)
            {
                if (SelectedTable == null || !CompletedTables.Contains(SelectedTable))
                    SelectedTable = CompletedTables.Count > 0 ? CompletedTables[0] : null;
            }
        }
    }

    /// <summary>True when the current SelectedTable is an open table and can be modified.</summary>
    public bool CanModifySelectedTable => SelectedTable != null && OpenTables.Contains(SelectedTable);

    /// <summary>Available products loaded from JSON.</summary>
    public ObservableCollection<Product> Products { get; } = new();

    /// <summary>Filtered view of products for the ComboBox.</summary>
    public ICollectionView? ProductsView
    {
        get => _productsView;
        private set
        {
            if (_productsView == value) return;
            _productsView = value;
            OnPropertyChanged(nameof(ProductsView));
        }
    }

    /// <summary>Search text typed into the product ComboBox for filtering.</summary>
    public string ProductSearchText
    {
        get => _productSearchText;
        set
        {
            if (_productSearchText == value) return;
            _productSearchText = value;
            OnPropertyChanged(nameof(ProductSearchText));
            ProductsView?.Refresh();
        }
    }

    /// <summary>Currently selected product in the product picker.</summary>
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (_selectedProduct == value) return;
            _selectedProduct = value;
            OnPropertyChanged(nameof(SelectedProduct));
            CommandManager.InvalidateRequerySuggested();
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
            CommandManager.InvalidateRequerySuggested();
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
            CommandManager.InvalidateRequerySuggested();
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

            SelectedOrder = _selectedTable?.Orders.FirstOrDefault();
            SelectedOrderItem = null;

            OnPropertyChanged(nameof(SelectedTable));
            OnPropertyChanged(nameof(CanModifySelectedTable));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>Command to add a new table with a user-specified name.</summary>
    public ICommand CreateTableCommand { get; }

    /// <summary>Command to rename the selected table.</summary>
    public ICommand EditTableCommand { get; }

    /// <summary>Command to delete the selected table.</summary>
    public ICommand DeleteTableCommand { get; }

    /// <summary>Command to complete the selected table (moves it to completed).</summary>
    public ICommand CompleteTableCommand { get; }

    /// <summary>Adds selected product to the selected order.</summary>
    public ICommand AddOrderItemCommand { get; }

    /// <summary>Removes the selected order item from the selected order.</summary>
    public ICommand RemoveOrderItemCommand { get; }

    private CancellationTokenSource? _statusCts;
    private string _statusMessage = string.Empty;

    /// <summary>Transient status message shown to the user for a short time.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    /// <summary>
    /// Creates the ViewModel with the specified dialog service.
    /// </summary>
    /// <param name="dialogService">Dialog service for user prompts.</param>
    public WaiterWorkspaceViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        CreateTableCommand = new RelayCommand(CreateTable);
        EditTableCommand = new RelayCommand(EditTable, () => CanModifySelectedTable);
        DeleteTableCommand = new RelayCommand(DeleteTable, () => CanModifySelectedTable);
        CompleteTableCommand = new RelayCommand(CompleteTable, () => CanModifySelectedTable && SelectedOrder?.OrderItems.Any() == true);
        AddOrderItemCommand = new RelayCommand(AddOrderItem, () => CanModifySelectedTable);
        RemoveOrderItemCommand = new RelayCommand(RemoveOrderItem, () => CanModifySelectedTable && SelectedOrderItem != null);

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
            OpenTables.Add(table);
            TablesTabIndex = 0;
            SelectedTable = table;
            _ = ShowStatusAsync($"Created table \"{name}\"");
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
            _ = ShowStatusAsync($"Renamed table to \"{name}\"");
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
        return OpenTables.Any(t => t != excludeTable &&
            string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Confirms and deletes the selected table.
    /// </summary>
    private void DeleteTable()
    {
        if (!CanModifySelectedTable || SelectedTable == null) return;

        if (_dialogService.Confirm("Delete Table", $"Delete \"{SelectedTable.Name}\" and all its orders?"))
        {
            var name = SelectedTable.Name;
            OpenTables.Remove(SelectedTable);
            TablesTabIndex = 0;
            SelectedTable = OpenTables.Count > 0 ? OpenTables[0] : null;
            _ = ShowStatusAsync($"Deleted \"{name}\"");
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void CompleteTable()
    {
        if (!CanModifySelectedTable || SelectedTable == null) return;

        if (SelectedOrder?.OrderItems.Any() != true)
        {
            _dialogService.ShowMessage("Cannot complete", "Add at least one item before completing the table.");
            return;
        }

        var table = SelectedTable;

        OpenTables.Remove(table);
        CompletedTables.Add(table);

        TablesTabIndex = 1;
        OnPropertyChanged(nameof(CanModifySelectedTable));
        CommandManager.InvalidateRequerySuggested();

        _ = ShowStatusAsync($"Completed \"{table.Name}\"");
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

            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;
            SelectedProduct = Products.FirstOrDefault();
        }
        catch
        {
            // swallow load errors for now
        }
    }

    /// <summary>
    /// Filter predicate for the products collection view.
    /// Shows products whose name contains the search text (case-insensitive).
    /// </summary>
    private bool FilterProducts(object obj)
    {
        if (obj is not Product product) return false;
        if (string.IsNullOrEmpty(_productSearchText)) return true;
        return product.Name.Contains(_productSearchText, StringComparison.CurrentCultureIgnoreCase);
    }

    private void AddOrderItem()
    {
        try
        {
            if (!CanModifySelectedTable) return;

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

            // If the product already exists in the order, increase its quantity instead of adding a duplicate line.
            var existing = SelectedOrder.OrderItems.FirstOrDefault(i =>
                (i.ProductId != 0 && i.ProductId == SelectedProduct.Id) ||
                (i.ProductId == 0 && string.Equals(i.Name, SelectedProduct.Name, StringComparison.Ordinal)));

            if (existing != null)
            {
                existing.Quantity += 1;
                SelectedOrderItem = existing;
                _ = ShowStatusAsync($"Increased quantity of \"{existing.Name}\" to {existing.Quantity}");
            }
            else
            {
                var item = new OrderItem
                {
                    ProductId = SelectedProduct.Id,
                    Name = SelectedProduct.Name,
                    Price = SelectedProduct.Price,
                    Quantity = 1
                };

                SelectedOrder.OrderItems.Add(item);
                SelectedOrderItem = item;
                _ = ShowStatusAsync($"Added \"{item.Name}\" to order");
            }

            // Clear the ComboBox so the waiter can start a fresh search
            SelectedProduct = null;
            ProductSearchText = string.Empty;
        }
        catch (Exception ex)
        {
            try
            {
                _dialogService.ShowMessage("Error adding item", ex.Message + "\n\n" + ex.StackTrace);
            }
            catch
            {
                // If dialog service also fails, write to debug as last resort
                System.Diagnostics.Debug.WriteLine("Error adding item: " + ex);
            }
            try
            {
                TablesWPF.Services.FileLogger.Log("AddOrderItem exception: " + ex);
            }
            catch { }
        }
    }

    private void RemoveOrderItem()
    {
        if (!CanModifySelectedTable) return;
        if (SelectedOrder == null || SelectedOrderItem == null) return;

        var name = SelectedOrderItem.Name;
        SelectedOrder.OrderItems.Remove(SelectedOrderItem);
        SelectedOrderItem = null;
        _ = ShowStatusAsync($"Removed \"{name}\" from order");
        CommandManager.InvalidateRequerySuggested();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private async Task ShowStatusAsync(string message, int milliseconds = 3000)
    {
        try
        {
            _statusCts?.Cancel();
            _statusCts = new CancellationTokenSource();
            StatusMessage = message;
            var token = _statusCts.Token;
            await Task.Delay(milliseconds, token);
            if (!token.IsCancellationRequested)
            {
                StatusMessage = string.Empty;
            }
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
        catch
        {
            // ignore other issues
        }
    }
}
