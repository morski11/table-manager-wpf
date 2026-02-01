using System.Windows;

namespace TablesWPF.Views;

/// <summary>
/// A simple input dialog window with a prompt and text box.
/// </summary>
public partial class InputDialogWindow : Window
{
    /// <summary>
    /// Gets or sets the prompt text displayed above the input box.
    /// </summary>
    public string PromptText
    {
        get => PromptTextBlock.Text;
        set => PromptTextBlock.Text = value;
    }

    /// <summary>
    /// Gets or sets the text in the input box.
    /// </summary>
    public string InputText
    {
        get => InputTextBox.Text;
        set => InputTextBox.Text = value;
    }

    public InputDialogWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        };
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
