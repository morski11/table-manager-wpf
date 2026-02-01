using System.Windows;
using TablesWPF.Views;

namespace TablesWPF.Services;

/// <summary>
/// WPF implementation of <see cref="IDialogService"/>.
/// </summary>
public class DialogService : IDialogService
{
    /// <inheritdoc />
    public bool TryGetInput(string title, string prompt, string? initialValue, out string result)
    {
        var dialog = new InputDialogWindow
        {
            Title = title,
            PromptText = prompt,
            InputText = initialValue ?? string.Empty,
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            var trimmed = dialog.InputText?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(trimmed))
            {
                result = trimmed;
                return true;
            }
        }

        result = string.Empty;
        return false;
    }

    /// <inheritdoc />
    public bool Confirm(string title, string message)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    /// <inheritdoc />
    public void ShowMessage(string title, string message)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
