using System;
using System.IO;

namespace TablesWPF.Services;

public static class FileLogger
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "app-errors.log");

    public static void Log(string message)
    {
        try
        {
            var text = DateTime.Now.ToString("s") + " " + message + Environment.NewLine;
            File.AppendAllText(LogPath, text);
        }
        catch
        {
            // ignore logging failures
        }
    }
}
