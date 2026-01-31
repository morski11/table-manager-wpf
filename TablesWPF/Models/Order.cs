using System.Collections.ObjectModel;
using System.ComponentModel;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
