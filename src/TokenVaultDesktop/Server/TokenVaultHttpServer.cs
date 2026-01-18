using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokenVaultDesktop.Data;

namespace TokenVaultDesktop.Server;

/// <summary>
/// Embedded HTTP server for token ingestion.
/// Runs within the WPF application process and accepts tokens from backend applications.
/// </summary>
public class TokenVaultHttpServer : IDisposable
{
    private WebApplication? _app;
    private readonly ITokenRepository _tokenRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ServerConfiguration _configuration;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    
    /// <summary>
    /// Indicates whether the server is currently running
    /// </summary>
    public bool IsRunning { get; private set; }
    
    /// <summary>
    /// The port the server is listening on
    /// </summary>
    public int Port => _configuration.Port;
    
    /// <summary>
    /// Event raised when a token is received from a backend application
    /// </summary>
    public event EventHandler<TokenReceivedEventArgs>? TokenReceived;
    
    /// <summary>
    /// Event raised when the server status changes
    /// </summary>
    public event EventHandler<ServerStatusEventArgs>? StatusChanged;
    
    public TokenVaultHttpServer(
        ITokenRepository tokenRepository,
        IProjectRepository projectRepository,
        ServerConfiguration? configuration = null)
    {
        _tokenRepository = tokenRepository;
        _projectRepository = projectRepository;
        _configuration = configuration ?? new ServerConfiguration();
    }
    
    /// <summary>
    /// Starts the HTTP server asynchronously
    /// </summary>
    public async Task StartAsync()
    {
        if (IsRunning)
        {
            return;
        }
        
        _cts = new CancellationTokenSource();
        
        try
        {
            var builder = WebApplication.CreateSlimBuilder();
            
            // Configure Kestrel to listen only on localhost
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(_configuration.Port);
            });
            
            // Disable server header for security
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.AddServerHeader = false;
            });
            
            // Configure CORS for local development (any origin on localhost)
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            
            _app = builder.Build();
            _app.UseCors();
            
            // Map all endpoints
            MapEndpoints(_app);
            
            IsRunning = true;
            StatusChanged?.Invoke(this, new ServerStatusEventArgs(true, $"Server started on port {_configuration.Port}"));
            
            // Run the server (StartAsync + WaitForShutdownAsync to support cancellation)
            await _app.StartAsync(_cts.Token);
            await _app.WaitForShutdownAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            IsRunning = false;
            StatusChanged?.Invoke(this, new ServerStatusEventArgs(false, $"Server error: {ex.Message}"));
            throw;
        }
    }
    
    /// <summary>
    /// Maps all HTTP endpoints
    /// </summary>
    private void MapEndpoints(WebApplication app)
    {
        // GET /ping - Health check
        app.MapGet("/ping", () => Results.Ok(new
        {
            message = "pong",
            status = "TokenVault Desktop is running",
            version = "2.0.0",
            timestamp = DateTime.UtcNow
        }));
        
        // POST /store - Store a token
        app.MapPost("/store", async (HttpContext context) =>
        {
            try
            {
                var request = await JsonSerializer.DeserializeAsync<StoreTokenRequest>(
                    context.Request.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (request == null || 
                    string.IsNullOrWhiteSpace(request.Project) || 
                    string.IsNullOrWhiteSpace(request.Token))
                {
                    return Results.BadRequest(new { error = "Project and token are required" });
                }
                
                // Store the token
                await _tokenRepository.UpsertByProjectNameAsync(request.Project, request.Token);
                
                // Raise event for UI notification
                TokenReceived?.Invoke(this, new TokenReceivedEventArgs(request.Project, DateTime.Now));
                
                return Results.Ok(new 
                { 
                    status = "saved", 
                    project = request.Project,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (JsonException)
            {
                return Results.BadRequest(new { error = "Invalid JSON format" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to save token: {ex.Message}");
            }
        });
        
        // GET /fetch/{project} - Get token for a project
        app.MapGet("/fetch/{project}", async (string project) =>
        {
            var token = await _tokenRepository.GetLatestByProjectNameAsync(project);
            
            if (token == null)
            {
                return Results.NotFound(new { error = "Token not found" });
            }
            
            return Results.Ok(new { token = token.TokenValue });
        });
        
        // GET /projects - List all projects
        app.MapGet("/projects", async () =>
        {
            var projects = await _projectRepository.GetAllAsync();
            return Results.Ok(projects.Select(p => new
            {
                p.Name,
                p.Port,
                p.ApiBaseUrl,
                p.Description
            }));
        });
        
        // GET /status - Server status
        app.MapGet("/status", () => Results.Ok(new
        {
            running = IsRunning,
            port = _configuration.Port,
            uptime = DateTime.UtcNow
        }));
    }
    
    /// <summary>
    /// Stops the HTTP server
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsRunning || _app == null)
        {
            return;
        }
        
        try
        {
            _cts?.Cancel();
            
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _app.StopAsync(timeoutCts.Token);
            
            IsRunning = false;
            StatusChanged?.Invoke(this, new ServerStatusEventArgs(false, "Server stopped"));
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, new ServerStatusEventArgs(false, $"Error stopping server: {ex.Message}"));
        }
    }
    
    /// <summary>
    /// Restarts the HTTP server
    /// </summary>
    public async Task RestartAsync()
    {
        await StopAsync();
        await Task.Delay(500); // Small delay to ensure port is released
        await StartAsync();
    }
    
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        
        if (_app != null)
        {
            _app.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(5));
        }
    }
}

/// <summary>
/// Request model for storing tokens
/// </summary>
public record StoreTokenRequest(string Project, string Token);

/// <summary>
/// Event arguments for token received events
/// </summary>
public class TokenReceivedEventArgs : EventArgs
{
    public string ProjectName { get; }
    public DateTime ReceivedAt { get; }
    
    public TokenReceivedEventArgs(string projectName, DateTime receivedAt)
    {
        ProjectName = projectName;
        ReceivedAt = receivedAt;
    }
}

/// <summary>
/// Event arguments for server status changes
/// </summary>
public class ServerStatusEventArgs : EventArgs
{
    public bool IsRunning { get; }
    public string Message { get; }
    
    public ServerStatusEventArgs(bool isRunning, string message)
    {
        IsRunning = isRunning;
        Message = message;
    }
}
