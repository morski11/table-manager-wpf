using System.ComponentModel;

namespace TablesWPF.Models;

/// <summary>
/// A single line item in an order (name, price, quantity).
/// </summary>
public class OrderItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private decimal _price;
    private int _quantity;

    /// <summary>Unique identifier for the order item.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Display name of the item.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    /// <summary>Unit price.</summary>
    public decimal Price
    {
        get => _price;
        set
        {
            if (_price == value) return;
            _price = value;
            OnPropertyChanged(nameof(Price));
        }
    }

    /// <summary>Quantity ordered.</summary>
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value) return;
            _quantity = value;
            OnPropertyChanged(nameof(Quantity));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
