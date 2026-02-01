using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Specialized;

namespace TablesWPF.Models;

/// <summary>
/// An order belonging to a table; contains a collection of order items.
/// </summary>
public class Order : INotifyPropertyChanged
{
    /// <summary>Unique identifier for the order.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Items in this order.</summary>
    public ObservableCollection<OrderItem> OrderItems { get; } = new();

    private decimal _total;

    /// <summary>Total for the order (sum of all item totals).</summary>
    public decimal Total
    {
        get => _total;
        private set
        {
            if (_total == value) return;
            _total = value;
            OnPropertyChanged(nameof(Total));
        }
    }

    public Order()
    {
        OrderItems.CollectionChanged += OrderItems_CollectionChanged;
    }

    private void OrderItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (OrderItem? item in e.OldItems)
            {
                if (item != null) item.PropertyChanged -= OrderItem_PropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (OrderItem? item in e.NewItems)
            {
                if (item != null) item.PropertyChanged += OrderItem_PropertyChanged;
            }
        }

        RecalculateTotal();
    }

    private void OrderItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Any change to an item's Total/Price/Quantity should update the order total
        if (e.PropertyName == nameof(OrderItem.Total) || e.PropertyName == nameof(OrderItem.Price) || e.PropertyName == nameof(OrderItem.Quantity))
        {
            RecalculateTotal();
        }
    }

    private void RecalculateTotal()
    {
        Total = OrderItems.Sum(i => i.Total);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
