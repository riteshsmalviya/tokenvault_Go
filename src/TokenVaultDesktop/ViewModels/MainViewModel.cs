using System.Collections.ObjectModel;
using System.Windows.Input;
using TokenVaultDesktop.Models;
using TokenVaultDesktop.Server;
using TokenVaultDesktop.Services;

namespace TokenVaultDesktop.ViewModels;

/// <summary>
/// Main ViewModel for the application shell
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly ITokenService _tokenService;
    private readonly TokenVaultHttpServer _httpServer;
    
    private ViewModelBase? _currentView;
    private bool _isServerRunning;
    private string _serverStatus = "Server stopped";
    private string _lastTokenProject = string.Empty;
    private DateTime? _lastTokenTime;
    private bool _minimizeToTray = true;
    
    public MainViewModel(
        IProjectService projectService,
        ITokenService tokenService,
        TokenVaultHttpServer httpServer,
        PostmanGeneratorViewModel postmanGeneratorViewModel,
        ProjectsViewModel projectsViewModel,
        TokensViewModel tokensViewModel,
        SettingsViewModel settingsViewModel)
    {
        _projectService = projectService;
        _tokenService = tokenService;
        _httpServer = httpServer;
        
        // Store view models
        PostmanGeneratorViewModel = postmanGeneratorViewModel;
        ProjectsViewModel = projectsViewModel;
        TokensViewModel = tokensViewModel;
        SettingsViewModel = settingsViewModel;
        
        // Set default view
        CurrentView = PostmanGeneratorViewModel;
        
        // Subscribe to server events
        _httpServer.TokenReceived += OnTokenReceived;
        _httpServer.StatusChanged += OnServerStatusChanged;
        
        // Initialize commands
        NavigateCommand = new RelayCommand(Navigate);
        StartServerCommand = new AsyncRelayCommand(StartServerAsync, _ => !IsServerRunning);
        StopServerCommand = new AsyncRelayCommand(StopServerAsync, _ => IsServerRunning);
        
        // Recent tokens collection
        RecentTokens = new ObservableCollection<TokenActivity>();
    }
    
    #region Properties
    
    public PostmanGeneratorViewModel PostmanGeneratorViewModel { get; }
    public ProjectsViewModel ProjectsViewModel { get; }
    public TokensViewModel TokensViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }
    
    public bool IsServerRunning
    {
        get => _isServerRunning;
        set => SetProperty(ref _isServerRunning, value);
    }
    
    public string ServerStatus
    {
        get => _serverStatus;
        set => SetProperty(ref _serverStatus, value);
    }
    
    public int ServerPort => _httpServer.Port;
    
    public string LastTokenProject
    {
        get => _lastTokenProject;
        set => SetProperty(ref _lastTokenProject, value);
    }
    
    public DateTime? LastTokenTime
    {
        get => _lastTokenTime;
        set => SetProperty(ref _lastTokenTime, value);
    }
    
    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => SetProperty(ref _minimizeToTray, value);
    }
    
    public ObservableCollection<TokenActivity> RecentTokens { get; }
    
    #endregion
    
    #region Commands
    
    public ICommand NavigateCommand { get; }
    public ICommand StartServerCommand { get; }
    public ICommand StopServerCommand { get; }
    
    #endregion
    
    #region Methods
    
    private void Navigate(object? parameter)
    {
        CurrentView = parameter switch
        {
            "Generator" => PostmanGeneratorViewModel,
            "Projects" => ProjectsViewModel,
            "Tokens" => TokensViewModel,
            "Settings" => SettingsViewModel,
            _ => CurrentView
        };
    }
    
    public async Task InitializeAsync()
    {
        // Start HTTP server
        await StartServerAsync(null);
        
        // Load initial data
        await ProjectsViewModel.LoadProjectsAsync();
        await TokensViewModel.LoadTokensAsync();
    }
    
    private async Task StartServerAsync(object? parameter)
    {
        try
        {
            // Run server in background
            _ = Task.Run(async () => await _httpServer.StartAsync());
            
            // Wait a bit for startup
            await Task.Delay(500);
            
            IsServerRunning = _httpServer.IsRunning;
            ServerStatus = $"🟢 Server running on localhost:{ServerPort}";
        }
        catch (Exception ex)
        {
            ServerStatus = $"🔴 Server error: {ex.Message}";
        }
    }
    
    private async Task StopServerAsync(object? parameter)
    {
        try
        {
            await _httpServer.StopAsync();
            IsServerRunning = false;
            ServerStatus = "🔴 Server stopped";
        }
        catch (Exception ex)
        {
            ServerStatus = $"Error stopping server: {ex.Message}";
        }
    }
    
    private void OnTokenReceived(object? sender, TokenReceivedEventArgs e)
    {
        // Update on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            LastTokenProject = e.ProjectName;
            LastTokenTime = e.ReceivedAt;
            
            // Add to recent tokens (keep last 10)
            RecentTokens.Insert(0, new TokenActivity
            {
                ProjectName = e.ProjectName,
                ReceivedAt = e.ReceivedAt
            });
            
            while (RecentTokens.Count > 10)
            {
                RecentTokens.RemoveAt(RecentTokens.Count - 1);
            }
            
            // Refresh tokens view
            _ = TokensViewModel.LoadTokensAsync();
        });
    }
    
    private void OnServerStatusChanged(object? sender, ServerStatusEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            IsServerRunning = e.IsRunning;
            ServerStatus = e.IsRunning 
                ? $"🟢 {e.Message}" 
                : $"🔴 {e.Message}";
        });
    }
    
    #endregion
}

/// <summary>
/// Represents a token activity for the activity log
/// </summary>
public class TokenActivity
{
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - ReceivedAt;
            
            if (diff.TotalSeconds < 60)
                return "just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            
            return ReceivedAt.ToString("MMM dd HH:mm");
        }
    }
}
