namespace TokenVaultDesktop.Models;

/// <summary>
/// Represents an API endpoint for Postman collection generation.
/// </summary>
public class Endpoint
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to the owning project
    /// </summary>
    public int ProjectId { get; set; }
    
    /// <summary>
    /// Display name for the request in Postman (e.g., "Get All Users")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP method (GET, POST, PUT, DELETE, PATCH)
    /// </summary>
    public string Method { get; set; } = "GET";
    
    /// <summary>
    /// API path relative to base URL (e.g., /api/users, /api/products/{id})
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description of what the endpoint does
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this endpoint requires authentication
    /// </summary>
    public bool RequiresAuth { get; set; } = true;
    
    /// <summary>
    /// Optional request body template (for POST/PUT/PATCH)
    /// </summary>
    public string? RequestBodyTemplate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Project? Project { get; set; }
}
