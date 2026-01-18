namespace TokenVaultDesktop.Services;

/// <summary>
/// Request model for Postman collection generation
/// </summary>
public class PostmanGenerationRequest
{
    /// <summary>
    /// Name of the collection (will be the project name)
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Local port the backend runs on
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// Base URL for all API requests
    /// </summary>
    public string ApiBaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// List of endpoints to include in the collection
    /// </summary>
    public List<EndpointDefinition> Endpoints { get; set; } = new();
    
    /// <summary>
    /// Optional description for the collection
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether to include a pre-request script for TokenVault integration
    /// </summary>
    public bool IncludeTokenVaultScript { get; set; } = true;
}

/// <summary>
/// Definition of an API endpoint for collection generation
/// </summary>
public class EndpointDefinition
{
    /// <summary>
    /// Display name for the request
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// HTTP method
    /// </summary>
    public string Method { get; set; } = "GET";
    
    /// <summary>
    /// API path (relative to base URL)
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this endpoint requires authentication
    /// </summary>
    public bool RequiresAuth { get; set; } = true;
    
    /// <summary>
    /// Optional request body template (for POST/PUT/PATCH)
    /// </summary>
    public string? RequestBody { get; set; }
}
