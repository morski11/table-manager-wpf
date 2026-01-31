using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TablesWPF.Models;

/// <summary>
/// A restaurant table with a name and a collection of orders.
/// </summary>
public class Table : INotifyPropertyChanged
{
    private string _name = string.Empty;

    /// <summary>Unique identifier for the table.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Display name (e.g. "Table 1").</summary>
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

    /// <summary>Orders for this table.</summary>
    public ObservableCollection<Order> Orders { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
