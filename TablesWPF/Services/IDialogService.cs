namespace TablesWPF.Services;

/// <summary>
/// Abstraction for UI dialogs to keep ViewModels testable.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an input dialog and returns the user's input.
    /// </summary>
    /// <param name="title">Dialog window title.</param>
    /// <param name="prompt">Prompt text shown to the user.</param>
    /// <param name="initialValue">Initial value in the text box (null for empty).</param>
    /// <param name="result">The trimmed input text if user confirmed.</param>
    /// <returns>True if user confirmed with non-empty input; false otherwise.</returns>
    bool TryGetInput(string title, string prompt, string? initialValue, out string result);

    /// <summary>
    /// Shows a confirmation dialog with Yes/No options.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Message to display.</param>
    /// <returns>True if user clicked Yes; false otherwise.</returns>
    bool Confirm(string title, string message);

    /// <summary>
    /// Shows an informational or warning message to the user.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Message to display.</param>
    void ShowMessage(string title, string message);
}
