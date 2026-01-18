using System.Windows.Input;
using TokenVaultDesktop.Data;
using TokenVaultDesktop.Server;

namespace TokenVaultDesktop.ViewModels;

/// <summary>
/// ViewModel for the Settings view
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ServerConfiguration _serverConfiguration;
    
    private int _serverPort = 9999;
    private bool _minimizeToTray = true;
    private bool _startWithWindows;
    private bool _autoStartServer = true;
    private string _databasePath = string.Empty;
    private bool _isDirty;
    private string _statusMessage = string.Empty;
    
    public SettingsViewModel(
        ISettingsRepository settingsRepository,
        ServerConfiguration serverConfiguration)
    {
        _settingsRepository = settingsRepository;
        _serverConfiguration = serverConfiguration;
        
        SaveCommand = new AsyncRelayCommand(SaveSettingsAsync, _ => IsDirty);
        ResetCommand = new AsyncRelayCommand(LoadSettingsAsync);
        OpenDatabaseFolderCommand = new RelayCommand(OpenDatabaseFolder);
        
        // Load settings
        _ = LoadSettingsAsync();
    }
    
    #region Properties
    
    public int ServerPort
    {
        get => _serverPort;
        set
        {
            if (SetProperty(ref _serverPort, value))
            {
                IsDirty = true;
            }
        }
    }
    
    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set
        {
            if (SetProperty(ref _minimizeToTray, value))
            {
                IsDirty = true;
            }
        }
    }
    
    public bool StartWithWindows
    {
        get => _startWithWindows;
        set
        {
            if (SetProperty(ref _startWithWindows, value))
            {
                IsDirty = true;
            }
        }
    }
    
    public bool AutoStartServer
    {
        get => _autoStartServer;
        set
        {
            if (SetProperty(ref _autoStartServer, value))
            {
                IsDirty = true;
            }
        }
    }
    
    public string DatabasePath
    {
        get => _databasePath;
        set => SetProperty(ref _databasePath, value);
    }
    
    public bool IsDirty
    {
        get => _isDirty;
        set => SetProperty(ref _isDirty, value);
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    
    public string AppVersion => "1.0.0";
    
    #endregion
    
    #region Commands
    
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand OpenDatabaseFolderCommand { get; }
    
    #endregion
    
    #region Methods
    
    private async Task LoadSettingsAsync()
    {
        ServerPort = await _settingsRepository.GetAsync("ServerPort", 9999);
        MinimizeToTray = await _settingsRepository.GetAsync("MinimizeToTray", true);
        StartWithWindows = await _settingsRepository.GetAsync("StartWithWindows", false);
        AutoStartServer = await _settingsRepository.GetAsync("AutoStartServer", true);
        
        // Get database path
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DatabasePath = Path.Combine(localAppData, "TokenVault", "tokenvault.db");
        
        IsDirty = false;
    }
    
    private async Task SaveSettingsAsync(object? parameter)
    {
        try
        {
            await _settingsRepository.SetAsync("ServerPort", ServerPort.ToString());
            await _settingsRepository.SetAsync("MinimizeToTray", MinimizeToTray.ToString().ToLower());
            await _settingsRepository.SetAsync("StartWithWindows", StartWithWindows.ToString().ToLower());
            await _settingsRepository.SetAsync("AutoStartServer", AutoStartServer.ToString().ToLower());
            
            // Update server configuration
            _serverConfiguration.Port = ServerPort;
            
            IsDirty = false;
            StatusMessage = "✓ Settings saved successfully";
            
            // Handle Windows startup registry
            UpdateStartupRegistry(StartWithWindows);
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Error saving settings: {ex.Message}";
        }
    }
    
    private void OpenDatabaseFolder(object? parameter)
    {
        var folder = Path.GetDirectoryName(DatabasePath);
        
        if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
    }
    
    private void UpdateStartupRegistry(bool enable)
    {
        try
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            
            if (key == null) return;
            
            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue("TokenVaultDesktop", $"\"{exePath}\"");
                }
            }
            else
            {
                key.DeleteValue("TokenVaultDesktop", false);
            }
            
            key.Close();
        }
        catch
        {
            // Ignore registry errors
        }
    }
    
    #endregion
}
