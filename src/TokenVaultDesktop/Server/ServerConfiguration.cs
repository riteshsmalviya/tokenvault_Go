namespace TokenVaultDesktop.Server;

/// <summary>
/// Configuration for the embedded HTTP server
/// </summary>
public class ServerConfiguration
{
    /// <summary>
    /// Port number for the HTTP server (default: 9999)
    /// </summary>
    public int Port { get; set; } = 9999;
    
    /// <summary>
    /// Host to bind to (always localhost for security)
    /// </summary>
    public string Host { get; set; } = "localhost";
    
    /// <summary>
    /// Whether the server should start automatically with the application
    /// </summary>
    public bool AutoStart { get; set; } = true;
    
    /// <summary>
    /// Gets the base URL for the server
    /// </summary>
    public string BaseUrl => $"http://{Host}:{Port}";
}
