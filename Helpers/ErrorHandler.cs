using System;
using System.Threading.Tasks;

namespace PSPSuite.Helpers;

public static class ErrorHandler
{
    public static void RegisterGlobalExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine($"[Exception]::Error: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Console.WriteLine($"[AsyncTaskError]::Error: {e.Exception.InnerException?.Message ?? e.Exception.Message}");
            Console.WriteLine(e.Exception.StackTrace);
            e.SetObserved();
        };

        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            Console.WriteLine($"[UIError]::Error: {e.Exception.Message}");
            Console.WriteLine(e.Exception.StackTrace);
            e.Handled = true;
        };
    }
}