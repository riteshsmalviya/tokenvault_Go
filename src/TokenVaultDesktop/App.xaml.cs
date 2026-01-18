using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokenVaultDesktop.Data;
using TokenVaultDesktop.Server;
using TokenVaultDesktop.Services;
using TokenVaultDesktop.ViewModels;
using TokenVaultDesktop.Views;

namespace TokenVaultDesktop;

/// <summary>
/// Application entry point and dependency injection configuration
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private TokenVaultHttpServer? _httpServer;
    
    /// <summary>
    /// Gets the service provider for dependency injection
    /// </summary>
    public static IServiceProvider? Services { get; private set; }
    
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        // Build host with dependency injection
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
        
        Services = _host.Services;
        
        try
        {
            // Initialize database
            var dbContext = _host.Services.GetRequiredService<TokenVaultDbContext>();
            await dbContext.InitializeDatabaseAsync();
            
            // Get and show main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
            
            // Initialize after window is shown
            await mainViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start TokenVault Desktop:\n\n{ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            Shutdown(1);
        }
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddSingleton<TokenVaultDbContext>();
        
        // Repositories
        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<ITokenRepository, TokenRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        
        // Services
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPostmanGenerator, PostmanGenerator>();
        
        // Server
        services.AddSingleton<ServerConfiguration>();
        services.AddSingleton<TokenVaultHttpServer>();
        
        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PostmanGeneratorViewModel>();
        services.AddSingleton<ProjectsViewModel>();
        services.AddSingleton<TokensViewModel>();
        services.AddSingleton<SettingsViewModel>();
        
        // Views
        services.AddSingleton<MainWindow>();
    }
    
    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        if (_host != null)
        {
            // Stop HTTP server gracefully
            var server = _host.Services.GetService<TokenVaultHttpServer>();
            if (server != null)
            {
                await server.StopAsync();
            }
            
            // Dispose host
            _host.Dispose();
        }
    }
}
