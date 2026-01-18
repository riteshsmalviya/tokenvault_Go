using System.ComponentModel;
using System.Windows;
using TokenVaultDesktop.ViewModels;

namespace TokenVaultDesktop.Views;

/// <summary>
/// Main application window with navigation and system tray support
/// </summary>
public partial class MainWindow : Window
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private bool _isClosing;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeSystemTray();
    }
    
    private void InitializeSystemTray()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "TokenVault Desktop - Server Running",
            Visible = false
        };
        
        // Create icon from embedded resource or use default
        try
        {
            var iconStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/Resources/Icons/tokenvault.ico"));
            
            if (iconStream != null)
            {
                _notifyIcon.Icon = new System.Drawing.Icon(iconStream.Stream);
            }
            else
            {
                // Use system icon as fallback
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }
        
        _notifyIcon.DoubleClick += (s, e) => ShowFromTray();
        
        // Context menu
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open TokenVault", null, (s, e) => ShowFromTray());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());
        
        _notifyIcon.ContextMenuStrip = contextMenu;
    }
    
    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            var viewModel = DataContext as MainViewModel;
            
            if (viewModel?.MinimizeToTray == true)
            {
                Hide();
                _notifyIcon!.Visible = true;
                
                _notifyIcon.ShowBalloonTip(
                    2000,
                    "TokenVault Desktop",
                    "Running in background. Token server is active.",
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }
    }
    
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        
        // If minimize to tray is enabled and not force closing, minimize instead
        if (viewModel?.MinimizeToTray == true && !_isClosing)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            return;
        }
        
        // Clean up notify icon
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
    
    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _notifyIcon!.Visible = false;
    }
    
    private void ExitApplication()
    {
        _isClosing = true;
        _notifyIcon?.Dispose();
        Application.Current.Shutdown();
    }
}
