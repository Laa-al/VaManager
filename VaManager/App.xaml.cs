using System.Windows;
using Serilog;
using VaManager.Models;
using VaManager.Services;

namespace VaManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public App()
    {
    }

    [STAThread]
    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Async(u => u.Console())
            .WriteTo.Async(u => u.File("log.txt"))
            .CreateLogger();
        try
        {
            Log.Information("Starting application");
            ConfigModel.Instance.LoadConfig();
            FileManager.Instance.ReanalyzeFromConfig();

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Unhandled exception");
        }
        Log.Information("Application terminated");
        Log.CloseAndFlush();
    }
}