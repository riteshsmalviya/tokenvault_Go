using System.Collections.ObjectModel;
using System.Windows.Input;
using TokenVaultDesktop.Models;
using TokenVaultDesktop.Services;

namespace TokenVaultDesktop.ViewModels;

/// <summary>
/// ViewModel for the Tokens viewer
/// </summary>
public class TokensViewModel : ViewModelBase
{
    private readonly ITokenService _tokenService;
    
    private ObservableCollection<TokenDisplayItem> _tokens = new();
    private TokenDisplayItem? _selectedToken;
    private bool _isLoading;
    private string _searchFilter = string.Empty;
    
    public TokensViewModel(ITokenService tokenService)
    {
        _tokenService = tokenService;
        
        LoadTokensCommand = new AsyncRelayCommand(LoadTokensAsync);
        DeleteTokenCommand = new AsyncRelayCommand(DeleteTokenAsync);
        CopyTokenCommand = new RelayCommand(CopyToken);
        RefreshCommand = new AsyncRelayCommand(LoadTokensAsync);
    }
    
    #region Properties
    
    public ObservableCollection<TokenDisplayItem> Tokens
    {
        get => _tokens;
        set => SetProperty(ref _tokens, value);
    }
    
    public TokenDisplayItem? SelectedToken
    {
        get => _selectedToken;
        set => SetProperty(ref _selectedToken, value);
    }
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    public string SearchFilter
    {
        get => _searchFilter;
        set
        {
            if (SetProperty(ref _searchFilter, value))
            {
                _ = LoadTokensAsync();
            }
        }
    }
    
    public bool HasTokens => Tokens.Count > 0;
    
    #endregion
    
    #region Commands
    
    public ICommand LoadTokensCommand { get; }
    public ICommand DeleteTokenCommand { get; }
    public ICommand CopyTokenCommand { get; }
    public ICommand RefreshCommand { get; }
    
    #endregion
    
    #region Methods
    
    public async Task LoadTokensAsync()
    {
        IsLoading = true;
        
        try
        {
            var tokens = await _tokenService.GetAllTokensAsync();
            
            // Apply filter if set
            if (!string.IsNullOrWhiteSpace(SearchFilter))
            {
                tokens = tokens.Where(t => 
                    t.Project?.Name.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) == true ||
                    t.TokenValue.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            Tokens.Clear();
            foreach (var token in tokens)
            {
                Tokens.Add(new TokenDisplayItem(token));
            }
            
            OnPropertyChanged(nameof(HasTokens));
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task DeleteTokenAsync(object? parameter)
    {
        if (parameter is not TokenDisplayItem item) return;
        
        var result = System.Windows.MessageBox.Show(
            $"Delete token for '{item.ProjectName}'?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            await _tokenService.DeleteTokenAsync(item.Id);
            await LoadTokensAsync();
        }
    }
    
    private void CopyToken(object? parameter)
    {
        if (parameter is TokenDisplayItem item)
        {
            System.Windows.Clipboard.SetText(item.FullTokenValue);
            
            System.Windows.MessageBox.Show(
                "Token copied to clipboard!",
                "Copied",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
    
    #endregion
}

/// <summary>
/// Display wrapper for Token with additional UI properties
/// </summary>
public class TokenDisplayItem : ViewModelBase
{
    private readonly Token _token;
    private bool _isExpanded;
    
    public TokenDisplayItem(Token token)
    {
        _token = token;
    }
    
    public int Id => _token.Id;
    public string ProjectName => _token.Project?.Name ?? "Unknown";
    public string TokenType => _token.TokenType;
    public DateTime UpdatedAt => _token.UpdatedAt;
    
    public string MaskedValue => _token.MaskedValue;
    public string FullTokenValue => _token.TokenValue;
    
    public bool IsExpired => _token.IsExpired;
    public DateTime? ExpiresAt => _token.ExpiresAt;
    
    public string UpdatedAtDisplay => _token.UpdatedAt.ToString("MMM dd, yyyy HH:mm:ss");
    
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - _token.UpdatedAt;
            
            if (diff.TotalSeconds < 60)
                return "just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            
            return _token.UpdatedAt.ToString("MMM dd");
        }
    }
    
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
    
    public string StatusIcon => IsExpired ? "⚠️" : "✓";
    public string StatusText => IsExpired ? "Expired" : "Valid";
}
