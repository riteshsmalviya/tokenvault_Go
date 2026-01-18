using Microsoft.Data.Sqlite;

namespace TokenVaultDesktop.Data;

/// <summary>
/// SQLite database context for TokenVault.
/// Manages database connection, initialization, and schema creation.
/// </summary>
public class TokenVaultDbContext : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    private readonly object _lock = new();
    
    public TokenVaultDbContext(string? databasePath = null)
    {
        var dbPath = databasePath ?? GetDefaultDatabasePath();
        _connectionString = $"Data Source={dbPath}";
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
    
    /// <summary>
    /// Gets the default database path in user's local app data
    /// </summary>
    private static string GetDefaultDatabasePath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "TokenVault", "tokenvault.db");
    }
    
    /// <summary>
    /// Gets a database connection (thread-safe singleton)
    /// </summary>
    public SqliteConnection GetConnection()
    {
        if (_connection == null)
        {
            lock (_lock)
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection(_connectionString);
                    _connection.Open();
                }
            }
        }
        
        return _connection;
    }
    
    /// <summary>
    /// Creates a new connection for concurrent operations
    /// </summary>
    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
    
    /// <summary>
    /// Initializes the database schema
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        var connection = GetConnection();
        
        // Create Projects table
        var createProjectsTable = @"
            CREATE TABLE IF NOT EXISTS Projects (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                Name            TEXT NOT NULL UNIQUE,
                Port            INTEGER NOT NULL,
                ApiBaseUrl      TEXT,
                Description     TEXT,
                CreatedAt       TEXT DEFAULT (datetime('now')),
                UpdatedAt       TEXT DEFAULT (datetime('now'))
            );
            
            CREATE INDEX IF NOT EXISTS idx_projects_name ON Projects(Name);
            CREATE INDEX IF NOT EXISTS idx_projects_port ON Projects(Port);
        ";
        
        // Create Tokens table
        var createTokensTable = @"
            CREATE TABLE IF NOT EXISTS Tokens (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId       INTEGER NOT NULL,
                TokenValue      TEXT NOT NULL,
                TokenType       TEXT DEFAULT 'Bearer',
                ExpiresAt       TEXT,
                CreatedAt       TEXT DEFAULT (datetime('now')),
                UpdatedAt       TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
            );
            
            CREATE INDEX IF NOT EXISTS idx_tokens_projectid ON Tokens(ProjectId);
        ";
        
        // Create Endpoints table
        var createEndpointsTable = @"
            CREATE TABLE IF NOT EXISTS Endpoints (
                Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId           INTEGER NOT NULL,
                Name                TEXT NOT NULL,
                Method              TEXT NOT NULL DEFAULT 'GET',
                Path                TEXT NOT NULL,
                Description         TEXT,
                RequiresAuth        INTEGER DEFAULT 1,
                RequestBodyTemplate TEXT,
                CreatedAt           TEXT DEFAULT (datetime('now')),
                FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
            );
            
            CREATE INDEX IF NOT EXISTS idx_endpoints_projectid ON Endpoints(ProjectId);
        ";
        
        // Create Settings table
        var createSettingsTable = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Key     TEXT PRIMARY KEY,
                Value   TEXT NOT NULL
            );
        ";
        
        await using var command = connection.CreateCommand();
        
        command.CommandText = createProjectsTable;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createTokensTable;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createEndpointsTable;
        await command.ExecuteNonQueryAsync();
        
        command.CommandText = createSettingsTable;
        await command.ExecuteNonQueryAsync();
        
        // Insert default settings if not exists
        command.CommandText = @"
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('ServerPort', '9999');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('MinimizeToTray', 'true');
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('StartWithWindows', 'false');
        ";
        await command.ExecuteNonQueryAsync();
    }
    
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
