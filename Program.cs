using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using PSPSuite.Helpers;
using PSPSuite.Views.Windows;
using System;

namespace PSPSuite;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        ErrorHandler.RegisterGlobalExceptionHandling();

        var lifetime = new ClassicDesktopStyleApplicationLifetime
        {
            Args = args,
            ShutdownMode = ShutdownMode.OnLastWindowClose,
        };

        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .AfterSetup(builder =>
            {
                if (builder.Instance is null) return;
                builder.Instance.Styles.Add(new FluentTheme());
            })
            .SetupWithLifetime(lifetime);

        lifetime.MainWindow = new MainWindow();
        lifetime.Start(args);
    }
}
