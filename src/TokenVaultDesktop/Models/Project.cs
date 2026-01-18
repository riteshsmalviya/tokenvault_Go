namespace TokenVaultDesktop.Models;

/// <summary>
/// Represents a project/application that stores tokens in TokenVault.
/// Maps backend applications to their ports and configuration.
/// </summary>
public class Project
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique project identifier (e.g., "my-api", "ecommerce-backend")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Local port the backend runs on (e.g., 5000, 3000, 8080)
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// Base URL for API requests (e.g., http://localhost:5000)
    /// </summary>
    public string? ApiBaseUrl { get; set; }
    
    /// <summary>
    /// Optional description of the project
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<Token> Tokens { get; set; } = new();
    public List<Endpoint> Endpoints { get; set; } = new();
}
