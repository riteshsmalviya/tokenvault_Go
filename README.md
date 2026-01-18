# TokenVault Desktop

A native Windows GUI application for managing authentication tokens and generating Postman collections with automatic Bearer token authentication.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Platform](https://img.shields.io/badge/Platform-Windows-0078D6)
![License](https://img.shields.io/badge/License-MIT-green)

## 🎯 Overview

TokenVault Desktop replaces the CLI-based TokenVault with a full-featured Windows desktop application. It provides:

- **Embedded HTTP Server**: Accepts tokens from your backend applications on `localhost:9999`
- **Postman Collection Generator**: Creates ready-to-import collections with Bearer token auth
- **SQLite Database**: Persistent local storage for projects and tokens
- **System Tray Support**: Runs quietly in the background

## 📋 Features

### 🚀 Postman Collection Generator
- Enter project name, port, and API base URL
- Add custom endpoints (GET, POST, PUT, DELETE, PATCH)
- Generates valid Postman Collection v2.1 JSON
- Includes pre-configured Bearer token authentication (`{{token}}`)
- Automatic TokenVault pre-request script for token fetching

### 🔑 Token Management
- View all stored tokens with masked values
- Copy tokens to clipboard
- Delete outdated tokens
- Real-time token reception notifications

### 📁 Project Management
- Save project configurations (name, port, base URL)
- Quick access to frequently used projects
- Port-to-project mapping for automation

### ⚙️ Settings
- Configure HTTP server port
- Minimize to system tray option
- Start with Windows option
- Database location information

## 🛠️ Building from Source

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11 (x64)
- Visual Studio 2022 (recommended) or VS Code with C# extension

### Build Steps

1. **Clone the repository**
   ```powershell
   git clone https://github.com/yourusername/TokenVaultDesktop.git
   cd TokenVaultDesktop
   ```

2. **Restore packages**
   ```powershell
   dotnet restore
   ```

3. **Build the application**
   ```powershell
   dotnet build --configuration Release
   ```

4. **Run the application**
   ```powershell
   dotnet run --project src/TokenVaultDesktop/TokenVaultDesktop.csproj
   ```

### Publish as Self-Contained Executable

Create a single-file, self-contained executable:

```powershell
dotnet publish src/TokenVaultDesktop/TokenVaultDesktop.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output ./publish `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true
```

The executable will be in `./publish/TokenVaultDesktop.exe`.

## 🚀 Usage

### Starting the Application

1. Run `TokenVaultDesktop.exe`
2. The HTTP server starts automatically on `localhost:9999`
3. The main window opens to the Postman Collection Generator

### Generating a Postman Collection

1. Go to **Generate Collection** (default view)
2. Enter:
   - **Project Name**: e.g., "my-api" (used as collection name and token identifier)
   - **Local Port**: e.g., 5000
   - **API Base URL**: e.g., `http://localhost:5000`
3. (Optional) Add endpoints by clicking **+ Add Endpoint**
4. Click **🚀 Generate Postman Collection**
5. Save the JSON file
6. Import into Postman

### Sending Tokens from Your Backend

Your backend applications can send tokens to TokenVault via HTTP:

```javascript
// Node.js / Express example
await fetch("http://localhost:9999/store", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
        project: "my-api",  // Must match your Postman project name
        token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    })
});
```

```python
# Python / FastAPI example
import requests

requests.post("http://localhost:9999/store", json={
    "project": "my-api",
    "token": jwt_token
})
```

```go
// Go example
http.Post("http://localhost:9999/store", "application/json",
    strings.NewReader(`{"project":"my-api","token":"` + token + `"}`))
```

```csharp
// C# / ASP.NET example
await httpClient.PostAsJsonAsync("http://localhost:9999/store", new {
    project = "my-api",
    token = jwtToken
});
```

### HTTP API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/ping` | GET | Health check |
| `/store` | POST | Store a token |
| `/fetch/{project}` | GET | Retrieve token for a project |
| `/projects` | GET | List all projects |
| `/status` | GET | Server status |

## 📁 Data Storage

TokenVault stores data in:
```
%LOCALAPPDATA%\TokenVault\tokenvault.db
```

This SQLite database contains:
- Projects (name, port, base URL)
- Tokens (project association, value, timestamps)
- Application settings

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    TokenVault Desktop                            │
├─────────────────────────────────────────────────────────────────┤
│  PRESENTATION (WPF)          │  EMBEDDED HTTP SERVER (Kestrel)  │
│  ┌─────────────────────────┐ │  ┌──────────────────────────────┐│
│  │ MainWindow              │ │  │ POST /store                  ││
│  │ PostmanGeneratorView    │ │  │ GET  /fetch/{project}        ││
│  │ ProjectsView            │ │  │ GET  /ping                   ││
│  │ TokensView              │ │  │ GET  /projects               ││
│  │ SettingsView            │ │  └──────────────────────────────┘│
│  └─────────────────────────┘ │                                   │
├─────────────────────────────────────────────────────────────────┤
│  SERVICES                                                        │
│  ┌─────────────────┐ ┌─────────────────┐ ┌────────────────────┐ │
│  │ ProjectService  │ │ TokenService    │ │ PostmanGenerator   │ │
│  └─────────────────┘ └─────────────────┘ └────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│  DATA (SQLite)                                                   │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │ %LOCALAPPDATA%\TokenVault\tokenvault.db                     ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

## 🔧 Technology Stack

| Component | Technology |
|-----------|------------|
| UI Framework | WPF (.NET 8) |
| Architecture | MVVM |
| Database | SQLite (Microsoft.Data.Sqlite) |
| HTTP Server | ASP.NET Core Minimal APIs (Kestrel) |
| DI Container | Microsoft.Extensions.DependencyInjection |
| MVVM Toolkit | CommunityToolkit.Mvvm |

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📞 Support

- Open an issue for bug reports
- Start a discussion for feature requests
