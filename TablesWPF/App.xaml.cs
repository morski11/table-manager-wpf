using System.Windows;
using TablesWPF.Services;

namespace TablesWPF;

/// <summary>
/// Application entry point for the Restaurant Table Manager.
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		DispatcherUnhandledException += App_DispatcherUnhandledException;
	}

	private void App_DispatcherUnhandledException(object? sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
	{
		try
		{
			FileLogger.Log("DispatcherUnhandledException: " + e.Exception);
			MessageBox.Show("An unexpected error occurred:\n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}
		catch
		{
			// ignore
		}
	}

	private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
	{
		try
		{
			if (e.ExceptionObject is Exception ex)
			{
				FileLogger.Log("UnhandledException: " + ex);
			}
			else
			{
				FileLogger.Log("UnhandledException: " + e.ExceptionObject?.ToString());
			}
		}
		catch
		{
			// ignore
		}
	}
}
