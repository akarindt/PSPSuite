using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using AvaloniaEdit;
using DialogHostAvalonia;
using Microsoft.Extensions.DependencyInjection;
using PSPSuite.Data;
using PSPSuite.Helpers;
using PSPSuite.Modules;
using PSPSuite.Views.Windows;
using System;
using System.Linq;

namespace PSPSuite;

class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        ErrorHandler.RegisterGlobalExceptionHandling();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        Services = serviceCollection.BuildServiceProvider();

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
                builder.Instance.Styles.Add(new DialogHostStyles());
                var avaloniaEditStyle = (IStyle)AvaloniaXamlLoader.Load(new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml"));
                builder.Instance.Styles.Add(avaloniaEditStyle);

                builder.Instance.RequestedThemeVariant = ThemeVariant.Light;
            })
            .SetupWithLifetime(lifetime);


        lifetime.MainWindow = Services.GetRequiredService<MainWindow>();
        lifetime.Start(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var baseTypesToScan = new[]
        {
            typeof(DependencyItem),
            typeof(GenericWindow),
            typeof(GenericModule)
        };

        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm =>
            {
                try { return asm.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(type => !type.IsInterface && !type.IsAbstract)
            .ToList();

        foreach (var type in allTypes)
        {
            foreach (var baseType in baseTypesToScan)
            {
                if (baseType.IsAssignableFrom(type))
                {
                    if (baseType == typeof(DependencyItem))
                    {
                        services.AddTransient(typeof(DependencyItem), type);
                        continue;
                    }

                    if (baseType == typeof(GenericModule))
                    {
                        services.AddTransient(typeof(GenericModule), type);
                        services.AddTransient(type);
                        continue;
                    }

                    services.AddTransient(type);
                    continue;
                }
            }
        }

        services.AddTransient<IDependencyLoader, DependencyLoader>();
    }
}
