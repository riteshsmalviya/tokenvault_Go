# TokenVault Desktop - Native Windows Application Design

## Executive Summary

This document outlines the complete architecture and implementation plan for **TokenVault Desktop**, a native Windows GUI application that replaces the existing TokenVault CLI. The application will be built using **C# with WPF (Windows Presentation Foundation)** targeting **.NET 8**, providing a modern, maintainable, and extensible Windows desktop experience.

---

## 1. Technology Stack Recommendation

### Recommended: C# + WPF + .NET 8

| Aspect | Justification |
|--------|---------------|
| **Native Windows Integration** | WPF is a first-class Windows citizen with deep OS integration (system tray, notifications, registry, Windows services) |
| **Modern Development** | .NET 8 offers excellent performance, long-term support (LTS), and modern C# language features |
| **UI Richness** | XAML-based declarative UI with data binding, styles, templates, and MVVM architecture support |
| **HTTP Server Capability** | Built-in `HttpListener` or ASP.NET Core Kestrel can run embedded within a desktop app |
| **SQLite Support** | Microsoft.Data.Sqlite provides excellent, well-maintained SQLite integration |
| **Tooling** | Visual Studio provides world-class debugging, designers, and packaging tools |
| **Distribution** | MSIX packaging, ClickOnce, or self-contained executables for easy installation |
| **Future Extensibility** | Can migrate to WinUI 3 later if needed; same C# codebase |

### Alternatives Considered

| Stack | Verdict |
|-------|---------|
| **WinUI 3** | More modern UI, but less mature ecosystem; WPF is more stable for developer tools |
| **WinForms** | Dated UI paradigm; lacks modern styling capabilities |
| **C++ Win32** | Maximum performance but significantly higher development complexity |
| **C++ WinUI** | Excellent but C# developer velocity is 3-5x faster for this type of application |

### Final Decision: **C# + WPF + .NET 8**

The optimal balance of developer productivity, native Windows integration, modern UI capabilities, and long-term maintainability.

---

## 2. High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        TokenVault Desktop Application                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                         PRESENTATION LAYER                              │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌────────────┐  │ │
│  │  │  MainWindow  │  │ProjectsView  │  │ TokensView   │  │SettingsView│  │ │
│  │  │   (Shell)    │  │              │  │              │  │            │  │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └────────────┘  │ │
│  │                              │                                          │ │
│  │                    ┌─────────▼─────────┐                               │ │
│  │                    │   ViewModels      │ ◄── MVVM Pattern              │ │
│  │                    │  (Data Binding)   │                               │ │
│  │                    └─────────┬─────────┘                               │ │
│  └──────────────────────────────┼──────────────────────────────────────────┘ │
│                                 │                                            │
│  ┌──────────────────────────────▼──────────────────────────────────────────┐ │
│  │                         APPLICATION LAYER                               │ │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────────────────┐ │ │
│  │  │ ProjectService │  │  TokenService  │  │ PostmanCollectionGenerator │ │ │
│  │  └────────────────┘  └────────────────┘  └────────────────────────────┘ │ │
│  │                                                                          │ │
│  │  ┌────────────────────────────────────────────────────────────────────┐ │ │
│  │  │              EmbeddedHttpServer (Token Ingestion API)              │ │ │
│  │  │                    Runs on localhost:9999                          │ │ │
│  │  └────────────────────────────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
│                                 │                                            │
│  ┌──────────────────────────────▼──────────────────────────────────────────┐ │
│  │                        INFRASTRUCTURE LAYER                             │ │
│  │  ┌────────────────────────────────────────────────────────────────────┐ │ │
│  │  │                    SQLite Database                                 │ │ │
│  │  │              %LOCALAPPDATA%\TokenVault\tokenvault.db               │ │ │
│  │  └────────────────────────────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Project Structure

```
TokenVaultDesktop/
├── TokenVaultDesktop.sln                 # Visual Studio Solution
│
├── src/
│   └── TokenVaultDesktop/
│       ├── TokenVaultDesktop.csproj      # Main WPF Application Project
│       │
│       ├── App.xaml                       # Application Entry & Resources
│       ├── App.xaml.cs                    # Startup, DI Container, HTTP Server Init
│       │
│       ├── Views/                         # XAML UI Views
│       │   ├── MainWindow.xaml            # Application Shell
│       │   ├── MainWindow.xaml.cs
│       │   ├── ProjectsView.xaml          # Project Management
│       │   ├── TokensView.xaml            # Token Viewer
│       │   ├── SettingsView.xaml          # Settings/Configuration
│       │   └── PostmanGeneratorView.xaml  # Postman Collection Generator
│       │
│       ├── ViewModels/                    # MVVM ViewModels
│       │   ├── ViewModelBase.cs           # INotifyPropertyChanged Base
│       │   ├── MainViewModel.cs
│       │   ├── ProjectsViewModel.cs
│       │   ├── TokensViewModel.cs
│       │   ├── SettingsViewModel.cs
│       │   └── PostmanGeneratorViewModel.cs
│       │
│       ├── Models/                        # Domain Models
│       │   ├── Project.cs
│       │   ├── Token.cs
│       │   └── PostmanCollection.cs       # Postman JSON Schema Models
│       │
│       ├── Services/                      # Business Logic
│       │   ├── IProjectService.cs
│       │   ├── ProjectService.cs
│       │   ├── ITokenService.cs
│       │   ├── TokenService.cs
│       │   ├── IPostmanGenerator.cs
│       │   └── PostmanGenerator.cs
│       │
│       ├── Server/                        # Embedded HTTP Server
│       │   ├── TokenVaultHttpServer.cs    # HTTP Listener Implementation
│       │   ├── TokenController.cs         # Request Handlers
│       │   └── ServerConfiguration.cs
│       │
│       ├── Data/                          # Data Access Layer
│       │   ├── TokenVaultDbContext.cs     # SQLite Context
│       │   ├── IRepository.cs
│       │   ├── ProjectRepository.cs
│       │   └── TokenRepository.cs
│       │
│       ├── Infrastructure/                # Cross-cutting Concerns
│       │   ├── DependencyInjection.cs     # Service Registration
│       │   └── AppSettings.cs
│       │
│       └── Resources/                     # Styles, Icons, Assets
│           ├── Styles/
│           │   └── AppStyles.xaml
│           └── Icons/
│               └── tokenvault.ico
│
├── tests/
│   └── TokenVaultDesktop.Tests/
│       └── TokenVaultDesktop.Tests.csproj
│
└── installer/
    └── TokenVaultDesktop.Installer/       # MSIX/Installer Project
```

---

## 4. Detailed Component Design

### 4.1 Database Schema (SQLite)

```sql
-- Projects Table
CREATE TABLE Projects (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT NOT NULL UNIQUE,
    Port            INTEGER NOT NULL,
    ApiBaseUrl      TEXT,
    Description     TEXT,
    CreatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Tokens Table  
CREATE TABLE Tokens (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId       INTEGER NOT NULL,
    TokenValue      TEXT NOT NULL,
    TokenType       TEXT DEFAULT 'Bearer',
    ExpiresAt       DATETIME,
    CreatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);

-- API Endpoints Table (for Postman collection generation)
CREATE TABLE Endpoints (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId       INTEGER NOT NULL,
    Name            TEXT NOT NULL,
    Method          TEXT NOT NULL,      -- GET, POST, PUT, DELETE, etc.
    Path            TEXT NOT NULL,      -- e.g., /api/users
    Description     TEXT,
    RequiresAuth    BOOLEAN DEFAULT 1,
    CreatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);

-- Settings Table
CREATE TABLE Settings (
    Key             TEXT PRIMARY KEY,
    Value           TEXT NOT NULL
);

-- Indexes for performance
CREATE INDEX idx_tokens_projectid ON Tokens(ProjectId);
CREATE INDEX idx_endpoints_projectid ON Endpoints(ProjectId);
CREATE INDEX idx_projects_port ON Projects(Port);
```

### 4.2 Domain Models

```csharp
// Models/Project.cs
namespace TokenVaultDesktop.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? ApiBaseUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public List<Token> Tokens { get; set; } = new();
    public List<Endpoint> Endpoints { get; set; } = new();
}

// Models/Token.cs
namespace TokenVaultDesktop.Models;

public class Token
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string TokenValue { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Project? Project { get; set; }
}

// Models/Endpoint.cs
namespace TokenVaultDesktop.Models;

public class Endpoint
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string Path { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresAuth { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Project? Project { get; set; }
}
```

---

## 5. Embedded HTTP Server Design

### 5.1 Architecture Overview

The application runs an **embedded HTTP server** within the same process as the GUI. This server listens on `localhost:9999` (configurable) and accepts token storage requests from backend applications.

```
┌─────────────────────────────────────────────────────────────────┐
│                    TokenVault Desktop Process                    │
│                                                                  │
│  ┌─────────────────────┐     ┌────────────────────────────────┐ │
│  │    WPF GUI Thread   │     │   HTTP Server Thread (Kestrel) │ │
│  │                     │     │                                │ │
│  │  ┌───────────────┐  │     │  Endpoints:                    │ │
│  │  │ MainWindow    │  │     │  POST /store                   │ │
│  │  │ Projects      │  │     │  GET  /fetch/{project}         │ │
│  │  │ Settings      │  │     │  GET  /ping                    │ │
│  │  └───────────────┘  │     │                                │ │
│  │         │           │     │         │                      │ │
│  └─────────┼───────────┘     └─────────┼──────────────────────┘ │
│            │                           │                        │
│            └───────────┬───────────────┘                        │
│                        ▼                                        │
│            ┌───────────────────────┐                            │
│            │   SQLite Database     │                            │
│            │   (Thread-Safe)       │                            │
│            └───────────────────────┘                            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

                              ▲
                              │ HTTP Requests
                              │
┌─────────────────────────────┴─────────────────────────────────────┐
│                     Backend Applications                          │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│  │ Node.js    │  │ Python     │  │ Go         │  │ .NET       │  │
│  │ Express    │  │ FastAPI    │  │ Gin        │  │ ASP.NET    │  │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘  │
└───────────────────────────────────────────────────────────────────┘
```

### 5.2 Implementation Using ASP.NET Core Minimal APIs

The embedded HTTP server uses **ASP.NET Core Minimal APIs** running on Kestrel—the same high-performance web server used in production ASP.NET applications—but embedded within our desktop app.

```csharp
// Server/TokenVaultHttpServer.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TokenVaultDesktop.Server;

public class TokenVaultHttpServer : IDisposable
{
    private WebApplication? _app;
    private readonly ITokenService _tokenService;
    private readonly IProjectService _projectService;
    private readonly int _port;
    private CancellationTokenSource? _cts;
    
    public bool IsRunning { get; private set; }
    public event EventHandler<TokenReceivedEventArgs>? TokenReceived;

    public TokenVaultHttpServer(
        ITokenService tokenService, 
        IProjectService projectService,
        int port = 9999)
    {
        _tokenService = tokenService;
        _projectService = projectService;
        _port = port;
    }

    public async Task StartAsync()
    {
        if (IsRunning) return;
        
        _cts = new CancellationTokenSource();
        
        var builder = WebApplication.CreateSlimBuilder();
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(_port);
        });

        // Configure CORS for local development
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

        // Map endpoints
        MapEndpoints(_app);
        
        IsRunning = true;
        await _app.RunAsync(_cts.Token);
    }

    private void MapEndpoints(WebApplication app)
    {
        // Health check endpoint
        app.MapGet("/ping", () => Results.Ok(new 
        { 
            message = "pong", 
            status = "TokenVault Desktop is running",
            version = "2.0.0"
        }));

        // Store token endpoint
        app.MapPost("/store", async (StoreTokenRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Project) || 
                string.IsNullOrWhiteSpace(request.Token))
            {
                return Results.BadRequest(new { error = "Project and token are required" });
            }

            try
            {
                await _tokenService.StoreTokenAsync(request.Project, request.Token);
                
                // Raise event for UI notification
                TokenReceived?.Invoke(this, new TokenReceivedEventArgs(request.Project));
                
                return Results.Ok(new { status = "saved", project = request.Project });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to save token: {ex.Message}");
            }
        });

        // Fetch token endpoint
        app.MapGet("/fetch/{project}", async (string project) =>
        {
            var token = await _tokenService.GetTokenAsync(project);
            
            if (token == null)
            {
                return Results.NotFound(new { error = "Token not found" });
            }

            return Results.Ok(new { token = token.TokenValue });
        });

        // List all projects endpoint (new feature)
        app.MapGet("/projects", async () =>
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Results.Ok(projects);
        });
    }

    public async Task StopAsync()
    {
        if (!IsRunning || _app == null) return;
        
        _cts?.Cancel();
        await _app.StopAsync();
        IsRunning = false;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _app?.DisposeAsync().AsTask().Wait();
    }
}

public record StoreTokenRequest(string Project, string Token);

public class TokenReceivedEventArgs : EventArgs
{
    public string ProjectName { get; }
    public TokenReceivedEventArgs(string projectName) => ProjectName = projectName;
}
```

### 5.3 Server Lifecycle Management

The HTTP server starts automatically when the application launches and runs in the background:

```csharp
// App.xaml.cs
public partial class App : Application
{
    private TokenVaultHttpServer? _httpServer;
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Build dependency injection container
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
        
        // Initialize database
        var dbContext = _host.Services.GetRequiredService<TokenVaultDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        
        // Start HTTP server
        _httpServer = _host.Services.GetRequiredService<TokenVaultHttpServer>();
        _ = Task.Run(async () => await _httpServer.StartAsync());
        
        // Show main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_httpServer != null)
        {
            await _httpServer.StopAsync();
        }
        
        _host?.Dispose();
        base.OnExit(e);
    }
}
```

---

## 6. Postman Collection Generator

### 6.1 Postman Collection Schema (v2.1)

The generator creates valid Postman Collection v2.1 JSON files that can be directly imported into Postman.

```csharp
// Models/PostmanCollection.cs
namespace TokenVaultDesktop.Models.Postman;

using System.Text.Json.Serialization;

public class PostmanCollection
{
    [JsonPropertyName("info")]
    public CollectionInfo Info { get; set; } = new();
    
    [JsonPropertyName("item")]
    public List<PostmanItem> Items { get; set; } = new();
    
    [JsonPropertyName("auth")]
    public PostmanAuth? Auth { get; set; }
    
    [JsonPropertyName("variable")]
    public List<PostmanVariable> Variables { get; set; } = new();
}

public class CollectionInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("schema")]
    public string Schema { get; set; } = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json";
    
    [JsonPropertyName("_postman_id")]
    public string PostmanId { get; set; } = Guid.NewGuid().ToString();
}

public class PostmanItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("request")]
    public PostmanRequest Request { get; set; } = new();
    
    [JsonPropertyName("response")]
    public List<object> Response { get; set; } = new();
}

public class PostmanRequest
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "GET";
    
    [JsonPropertyName("header")]
    public List<PostmanHeader> Headers { get; set; } = new();
    
    [JsonPropertyName("url")]
    public PostmanUrl Url { get; set; } = new();
    
    [JsonPropertyName("auth")]
    public PostmanAuth? Auth { get; set; }
    
    [JsonPropertyName("body")]
    public PostmanBody? Body { get; set; }
}

public class PostmanUrl
{
    [JsonPropertyName("raw")]
    public string Raw { get; set; } = string.Empty;
    
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = "http";
    
    [JsonPropertyName("host")]
    public List<string> Host { get; set; } = new();
    
    [JsonPropertyName("port")]
    public string? Port { get; set; }
    
    [JsonPropertyName("path")]
    public List<string> Path { get; set; } = new();
}

public class PostmanAuth
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "bearer";
    
    [JsonPropertyName("bearer")]
    public List<PostmanAuthParam> Bearer { get; set; } = new();
}

public class PostmanAuthParam
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";
}

public class PostmanVariable
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";
}

public class PostmanHeader
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";
}

public class PostmanBody
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "raw";
    
    [JsonPropertyName("raw")]
    public string Raw { get; set; } = string.Empty;
    
    [JsonPropertyName("options")]
    public PostmanBodyOptions? Options { get; set; }
}

public class PostmanBodyOptions
{
    [JsonPropertyName("raw")]
    public PostmanRawOptions Raw { get; set; } = new();
}

public class PostmanRawOptions
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = "json";
}
```

### 6.2 Generator Implementation

```csharp
// Services/PostmanGenerator.cs
namespace TokenVaultDesktop.Services;

using System.Text.Json;
using TokenVaultDesktop.Models;
using TokenVaultDesktop.Models.Postman;

public interface IPostmanGenerator
{
    PostmanCollection GenerateCollection(PostmanGenerationRequest request);
    string GenerateJson(PostmanCollection collection);
    Task SaveToFileAsync(PostmanCollection collection, string filePath);
}

public class PostmanGenerationRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public int Port { get; set; }
    public string ApiBaseUrl { get; set; } = string.Empty;
    public List<EndpointDefinition> Endpoints { get; set; } = new();
}

public class EndpointDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = string.Empty;
    public bool RequiresAuth { get; set; } = true;
    public string? RequestBody { get; set; }
}

public class PostmanGenerator : IPostmanGenerator
{
    public PostmanCollection GenerateCollection(PostmanGenerationRequest request)
    {
        var collection = new PostmanCollection
        {
            Info = new CollectionInfo
            {
                Name = request.ProjectName,
                Description = $"API Collection for {request.ProjectName} - Generated by TokenVault Desktop",
                PostmanId = Guid.NewGuid().ToString()
            },
            
            // Collection-level Bearer Token auth
            Auth = new PostmanAuth
            {
                Type = "bearer",
                Bearer = new List<PostmanAuthParam>
                {
                    new() { Key = "token", Value = "{{token}}", Type = "string" }
                }
            },
            
            // Collection-level variables
            Variables = new List<PostmanVariable>
            {
                new() { Key = "token", Value = "", Type = "string" },
                new() { Key = "baseUrl", Value = request.ApiBaseUrl, Type = "string" }
            }
        };

        // Generate request items for each endpoint
        foreach (var endpoint in request.Endpoints)
        {
            var item = CreateRequestItem(request, endpoint);
            collection.Items.Add(item);
        }

        // Add default endpoints if none provided
        if (!request.Endpoints.Any())
        {
            collection.Items.Add(CreateRequestItem(request, new EndpointDefinition
            {
                Name = "Sample GET Request",
                Method = "GET",
                Path = "/api/sample",
                RequiresAuth = true
            }));
        }

        return collection;
    }

    private PostmanItem CreateRequestItem(PostmanGenerationRequest request, EndpointDefinition endpoint)
    {
        var baseUrl = request.ApiBaseUrl.TrimEnd('/');
        var path = endpoint.Path.TrimStart('/');
        var fullUrl = $"{baseUrl}/{path}";
        
        var urlParts = ParseUrl(fullUrl);

        var item = new PostmanItem
        {
            Name = endpoint.Name,
            Request = new PostmanRequest
            {
                Method = endpoint.Method.ToUpperInvariant(),
                Url = new PostmanUrl
                {
                    Raw = fullUrl,
                    Protocol = urlParts.Protocol,
                    Host = urlParts.Host,
                    Port = urlParts.Port,
                    Path = urlParts.Path
                },
                Headers = new List<PostmanHeader>
                {
                    new() { Key = "Content-Type", Value = "application/json", Type = "text" }
                }
            }
        };

        // Add auth if required
        if (endpoint.RequiresAuth)
        {
            item.Request.Auth = new PostmanAuth
            {
                Type = "bearer",
                Bearer = new List<PostmanAuthParam>
                {
                    new() { Key = "token", Value = "{{token}}", Type = "string" }
                }
            };
        }

        // Add request body for POST/PUT/PATCH
        if (endpoint.Method.ToUpperInvariant() is "POST" or "PUT" or "PATCH")
        {
            item.Request.Body = new PostmanBody
            {
                Mode = "raw",
                Raw = endpoint.RequestBody ?? "{\n  \n}",
                Options = new PostmanBodyOptions
                {
                    Raw = new PostmanRawOptions { Language = "json" }
                }
            };
        }

        return item;
    }

    private (string Protocol, List<string> Host, string? Port, List<string> Path) ParseUrl(string url)
    {
        var uri = new Uri(url);
        
        return (
            Protocol: uri.Scheme,
            Host: uri.Host.Split('.').ToList(),
            Port: uri.IsDefaultPort ? null : uri.Port.ToString(),
            Path: uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList()
        );
    }

    public string GenerateJson(PostmanCollection collection)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        return JsonSerializer.Serialize(collection, options);
    }

    public async Task SaveToFileAsync(PostmanCollection collection, string filePath)
    {
        var json = GenerateJson(collection);
        await File.WriteAllTextAsync(filePath, json);
    }
}
```

---

## 7. User Interface Design

### 7.1 Main Window Layout

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  TokenVault Desktop                                              ─  □  ✕    │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────┐  ┌─────────────────────────────────────────────────────────┐│
│  │             │  │  POSTMAN COLLECTION GENERATOR                           ││
│  │  📁 Projects│  │  ─────────────────────────────────────────────────────  ││
│  │             │  │                                                         ││
│  │  🔑 Tokens  │  │  Project Name        [ My API Project          ]        ││
│  │             │  │                                                         ││
│  │  📤 Generate│  │  Local Backend Port  [ 5000                    ]        ││
│  │             │  │                                                         ││
│  │  ⚙️ Settings│  │  API Base URL        [ http://localhost:5000   ]        ││
│  │             │  │                                                         ││
│  └─────────────┘  │  ┌────────────────────────────────────────────────────┐ ││
│                   │  │  ENDPOINTS                                         │ ││
│                   │  │  ───────────────────────────────────────────────── │ ││
│                   │  │  [+] Add Endpoint                                  │ ││
│                   │  │                                                    │ ││
│                   │  │  ┌──────────────────────────────────────────────┐  │ ││
│                   │  │  │ GET    /api/users      Get All Users    [✕]  │  │ ││
│                   │  │  └──────────────────────────────────────────────┘  │ ││
│                   │  │  ┌──────────────────────────────────────────────┐  │ ││
│                   │  │  │ POST   /api/users      Create User      [✕]  │  │ ││
│                   │  │  └──────────────────────────────────────────────┘  │ ││
│                   │  │  ┌──────────────────────────────────────────────┐  │ ││
│                   │  │  │ GET    /api/users/{id} Get User By ID   [✕]  │  │ ││
│                   │  │  └──────────────────────────────────────────────┘  │ ││
│                   │  └────────────────────────────────────────────────────┘ ││
│                   │                                                         ││
│                   │        ┌─────────────────────────────────────────┐      ││
│                   │        │  🚀 GENERATE POSTMAN COLLECTION          │      ││
│                   │        └─────────────────────────────────────────┘      ││
│                   │                                                         ││
│                   └─────────────────────────────────────────────────────────┘│
├──────────────────────────────────────────────────────────────────────────────┤
│  🟢 Server running on localhost:9999  │  Last token: "my-api" @ 2:34 PM     │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 XAML Implementation for Postman Generator View

```xml
<!-- Views/PostmanGeneratorView.xaml -->
<UserControl x:Class="TokenVaultDesktop.Views.PostmanGeneratorView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:TokenVaultDesktop.ViewModels"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance Type=vm:PostmanGeneratorViewModel}">
    
    <UserControl.Resources>
        <Style x:Key="InputLabelStyle" TargetType="TextBlock">
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Margin" Value="0,0,0,4"/>
            <Setter Property="Foreground" Value="#333333"/>
        </Style>
        
        <Style x:Key="InputTextBoxStyle" TargetType="TextBox">
            <Setter Property="Padding" Value="8,6"/>
            <Setter Property="BorderBrush" Value="#CCCCCC"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Background" Value="White"/>
        </Style>
        
        <Style x:Key="PrimaryButtonStyle" TargetType="Button">
            <Setter Property="Background" Value="#FF6D28"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="Padding" Value="24,12"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="Bold"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
        </Style>
    </UserControl.Resources>
    
    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,24">
            <TextBlock Text="Postman Collection Generator" 
                       FontSize="24" 
                       FontWeight="Bold"
                       Foreground="#1A1A2E"/>
            <TextBlock Text="Create a ready-to-import Postman collection with Bearer token authentication"
                       Foreground="#666666"
                       Margin="0,4,0,0"/>
        </StackPanel>
        
        <!-- Form -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <StackPanel>
                <!-- Project Name -->
                <StackPanel Margin="0,0,0,16">
                    <TextBlock Text="Project Name" Style="{StaticResource InputLabelStyle}"/>
                    <TextBox Text="{Binding ProjectName, UpdateSourceTrigger=PropertyChanged}"
                             Style="{StaticResource InputTextBoxStyle}"
                             ToolTip="This will be the name of your Postman collection"/>
                </StackPanel>
                
                <!-- Port -->
                <StackPanel Margin="0,0,0,16">
                    <TextBlock Text="Local Backend Port" Style="{StaticResource InputLabelStyle}"/>
                    <TextBox Text="{Binding Port, UpdateSourceTrigger=PropertyChanged}"
                             Style="{StaticResource InputTextBoxStyle}"
                             ToolTip="The port your backend runs on (e.g., 5000, 3000, 8080)"/>
                </StackPanel>
                
                <!-- Base URL -->
                <StackPanel Margin="0,0,0,16">
                    <TextBlock Text="API Base URL" Style="{StaticResource InputLabelStyle}"/>
                    <TextBox Text="{Binding ApiBaseUrl, UpdateSourceTrigger=PropertyChanged}"
                             Style="{StaticResource InputTextBoxStyle}"
                             ToolTip="The base URL for API requests (e.g., http://localhost:5000)"/>
                </StackPanel>
                
                <!-- Endpoints Section -->
                <Border BorderBrush="#E0E0E0" BorderThickness="1" CornerRadius="4" Padding="16" Margin="0,8,0,16">
                    <StackPanel>
                        <Grid Margin="0,0,0,12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <TextBlock Text="API Endpoints" 
                                       FontWeight="SemiBold" 
                                       FontSize="14"
                                       VerticalAlignment="Center"/>
                            
                            <Button Grid.Column="1" 
                                    Content="+ Add Endpoint"
                                    Command="{Binding AddEndpointCommand}"
                                    Padding="12,6"
                                    Background="#4CAF50"
                                    Foreground="White"
                                    BorderThickness="0"
                                    Cursor="Hand"/>
                        </Grid>
                        
                        <!-- Endpoints List -->
                        <ItemsControl ItemsSource="{Binding Endpoints}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background="#F8F9FA" 
                                            CornerRadius="4" 
                                            Padding="12" 
                                            Margin="0,0,0,8">
                                        <Grid>
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition Width="80"/>
                                                <ColumnDefinition Width="*"/>
                                                <ColumnDefinition Width="*"/>
                                                <ColumnDefinition Width="Auto"/>
                                            </Grid.ColumnDefinitions>
                                            
                                            <ComboBox Grid.Column="0"
                                                      SelectedValue="{Binding Method}"
                                                      Margin="0,0,8,0">
                                                <ComboBoxItem Content="GET"/>
                                                <ComboBoxItem Content="POST"/>
                                                <ComboBoxItem Content="PUT"/>
                                                <ComboBoxItem Content="DELETE"/>
                                                <ComboBoxItem Content="PATCH"/>
                                            </ComboBox>
                                            
                                            <TextBox Grid.Column="1" 
                                                     Text="{Binding Path}"
                                                     Margin="0,0,8,0"
                                                     ToolTip="API path (e.g., /api/users)"/>
                                            
                                            <TextBox Grid.Column="2" 
                                                     Text="{Binding Name}"
                                                     Margin="0,0,8,0"
                                                     ToolTip="Request name for Postman"/>
                                            
                                            <Button Grid.Column="3" 
                                                    Content="✕"
                                                    Command="{Binding DataContext.RemoveEndpointCommand, 
                                                              RelativeSource={RelativeSource AncestorType=UserControl}}"
                                                    CommandParameter="{Binding}"
                                                    Background="#FF5252"
                                                    Foreground="White"
                                                    Width="28"
                                                    BorderThickness="0"
                                                    Cursor="Hand"/>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        
                        <TextBlock Text="No endpoints added. Click 'Add Endpoint' or the collection will include a sample request."
                                   Foreground="#999999"
                                   FontStyle="Italic"
                                   Margin="0,8,0,0"
                                   Visibility="{Binding HasNoEndpoints, Converter={StaticResource BoolToVisibilityConverter}}"/>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>
        
        <!-- Generate Button -->
        <Button Grid.Row="2"
                Content="🚀 Generate Postman Collection"
                Command="{Binding GenerateCollectionCommand}"
                Style="{StaticResource PrimaryButtonStyle}"
                HorizontalAlignment="Center"
                Margin="0,24,0,0"/>
    </Grid>
</UserControl>
```

### 7.3 ViewModel for Postman Generator

```csharp
// ViewModels/PostmanGeneratorViewModel.cs
namespace TokenVaultDesktop.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TokenVaultDesktop.Models;
using TokenVaultDesktop.Services;

public partial class PostmanGeneratorViewModel : ViewModelBase
{
    private readonly IPostmanGenerator _postmanGenerator;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private int _port = 5000;

    [ObservableProperty]
    private string _apiBaseUrl = "http://localhost:5000";

    [ObservableProperty]
    private ObservableCollection<EndpointViewModel> _endpoints = new();

    public bool HasNoEndpoints => Endpoints.Count == 0;

    public PostmanGeneratorViewModel(
        IPostmanGenerator postmanGenerator, 
        IProjectService projectService)
    {
        _postmanGenerator = postmanGenerator;
        _projectService = projectService;
    }

    partial void OnPortChanged(int value)
    {
        // Auto-update base URL when port changes
        if (ApiBaseUrl.StartsWith("http://localhost:"))
        {
            ApiBaseUrl = $"http://localhost:{value}";
        }
    }

    [RelayCommand]
    private void AddEndpoint()
    {
        Endpoints.Add(new EndpointViewModel
        {
            Method = "GET",
            Path = "/api/",
            Name = $"Request {Endpoints.Count + 1}"
        });
        OnPropertyChanged(nameof(HasNoEndpoints));
    }

    [RelayCommand]
    private void RemoveEndpoint(EndpointViewModel endpoint)
    {
        Endpoints.Remove(endpoint);
        OnPropertyChanged(nameof(HasNoEndpoints));
    }

    [RelayCommand]
    private async Task GenerateCollectionAsync()
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            MessageBox.Show("Please enter a project name.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Create generation request
        var request = new PostmanGenerationRequest
        {
            ProjectName = ProjectName,
            Port = Port,
            ApiBaseUrl = ApiBaseUrl,
            Endpoints = Endpoints.Select(e => new EndpointDefinition
            {
                Name = e.Name,
                Method = e.Method,
                Path = e.Path,
                RequiresAuth = e.RequiresAuth
            }).ToList()
        };

        // Generate collection
        var collection = _postmanGenerator.GenerateCollection(request);

        // Show save dialog
        var saveDialog = new SaveFileDialog
        {
            FileName = $"{ProjectName.Replace(" ", "_")}_postman_collection.json",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Save Postman Collection"
        };

        if (saveDialog.ShowDialog() == true)
        {
            await _postmanGenerator.SaveToFileAsync(collection, saveDialog.FileName);
            
            MessageBox.Show(
                $"Postman collection saved successfully!\n\n" +
                $"File: {saveDialog.FileName}\n\n" +
                $"To use:\n" +
                $"1. Open Postman\n" +
                $"2. Click 'Import'\n" +
                $"3. Select the generated file\n" +
                $"4. Your collection is ready with Bearer token auth!",
                "Collection Generated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            // Optionally save project to database
            await _projectService.SaveProjectAsync(new Project
            {
                Name = ProjectName,
                Port = Port,
                ApiBaseUrl = ApiBaseUrl
            });
        }
    }
}

public partial class EndpointViewModel : ObservableObject
{
    [ObservableProperty]
    private string _method = "GET";

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _requiresAuth = true;
}
```

---

## 8. Application Lifecycle & System Tray Integration

### 8.1 System Tray Support

The application can minimize to the system tray to run the HTTP server in the background without cluttering the taskbar:

```csharp
// Views/MainWindow.xaml.cs
public partial class MainWindow : Window
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        InitializeSystemTray();
    }

    private void InitializeSystemTray()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = new System.Drawing.Icon("Resources/Icons/tokenvault.ico"),
            Visible = false,
            Text = "TokenVault Desktop - Server Running"
        };

        _notifyIcon.DoubleClick += (s, e) => ShowWindow();
        
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open TokenVault", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("Server Status", null, (s, e) => ShowServerStatus());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());
        
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized && _viewModel.MinimizeToTray)
        {
            Hide();
            _notifyIcon!.Visible = true;
            _notifyIcon.ShowBalloonTip(
                2000, 
                "TokenVault Desktop", 
                "Running in background. Token server active on port 9999.", 
                System.Windows.Forms.ToolTipIcon.Info);
        }
        
        base.OnStateChanged(e);
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _notifyIcon!.Visible = false;
    }

    private void ShowServerStatus()
    {
        MessageBox.Show(
            $"HTTP Server: {(_viewModel.IsServerRunning ? "Running" : "Stopped")}\n" +
            $"Port: {_viewModel.ServerPort}\n" +
            $"Active Projects: {_viewModel.ProjectCount}",
            "Server Status",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ExitApplication()
    {
        _notifyIcon?.Dispose();
        Application.Current.Shutdown();
    }
}
```

---

## 9. Installation & Distribution

### 9.1 Self-Contained Deployment

Build as a single-file, self-contained executable:

```xml
<!-- TokenVaultDesktop.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Resources\Icons\tokenvault.ico</ApplicationIcon>
    
    <!-- Publish Settings -->
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.App" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### 9.2 MSIX Installer

For Windows Store or enterprise distribution:

```
TokenVaultDesktop.Installer/
├── Package.appxmanifest
├── Assets/
│   ├── Square44x44Logo.png
│   ├── Square150x150Logo.png
│   ├── Wide310x150Logo.png
│   └── StoreLogo.png
└── TokenVaultDesktop.Installer.wapproj
```

---

## 10. Security Considerations

| Concern | Mitigation |
|---------|------------|
| **Token Storage** | Tokens stored in SQLite with file-system permissions; optionally encrypt using Windows DPAPI |
| **HTTP Server** | Binds only to `localhost` - not accessible from network |
| **Database Location** | Stored in `%LOCALAPPDATA%\TokenVault\` - user-specific, not world-readable |
| **Input Validation** | All HTTP endpoints validate input; parameterized SQL queries prevent injection |

---

## 11. Future Extensibility

| Feature | Implementation Path |
|---------|---------------------|
| **Multiple Environments** | Add Environment entity; switch between dev/staging/prod tokens |
| **Token Expiration Alerts** | Parse JWT exp claim; show notifications when tokens expire |
| **OpenAPI Import** | Parse swagger.json to auto-generate endpoints |
| **Team Sharing** | Export/import encrypted database backups |
| **Dark Mode** | WPF theme switching with resource dictionaries |
| **Plugins** | MEF-based plugin system for custom token sources |

---

## 12. Summary

This design delivers a **professional-grade native Windows application** that:

✅ Runs as a standard Windows program with a visible window  
✅ Uses SQLite for persistent local storage  
✅ Embeds an HTTP server for token ingestion (no terminal required)  
✅ Generates valid Postman Collection v2.1 JSON with Bearer auth  
✅ Provides a modern, intuitive WPF user interface  
✅ Supports system tray for background operation  
✅ Is distributable as a single .exe or MSIX installer  

The C# + WPF + .NET 8 stack ensures **maximum maintainability**, **excellent Windows integration**, and a **familiar developer experience** for future contributors.
