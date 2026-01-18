namespace TokenVaultDesktop.Models;

/// <summary>
/// Represents an authentication token stored for a project.
/// </summary>
public class Token
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to the owning project
    /// </summary>
    public int ProjectId { get; set; }
    
    /// <summary>
    /// The actual token value (JWT, API key, etc.)
    /// </summary>
    public string TokenValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Token type (Bearer, Basic, API-Key, etc.)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// Optional expiration time (parsed from JWT if available)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Project? Project { get; set; }
    
    /// <summary>
    /// Checks if the token is expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    
    /// <summary>
    /// Returns a masked version of the token for display
    /// </summary>
    public string MaskedValue
    {
        get
        {
            if (string.IsNullOrEmpty(TokenValue)) return string.Empty;
            if (TokenValue.Length <= 20) return new string('*', TokenValue.Length);
            return TokenValue[..10] + "..." + TokenValue[^10..];
        }
    }
}
