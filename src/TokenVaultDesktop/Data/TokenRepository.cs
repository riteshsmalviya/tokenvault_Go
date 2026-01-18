using Microsoft.Data.Sqlite;
using TokenVaultDesktop.Models;

namespace TokenVaultDesktop.Data;

/// <summary>
/// Repository for Token entities
/// </summary>
public interface ITokenRepository
{
    Task<List<Token>> GetAllAsync();
    Task<List<Token>> GetByProjectIdAsync(int projectId);
    Task<Token?> GetLatestByProjectNameAsync(string projectName);
    Task<Token?> GetByIdAsync(int id);
    Task<int> CreateAsync(Token token);
    Task<int> UpsertByProjectNameAsync(string projectName, string tokenValue);
    Task DeleteAsync(int id);
    Task DeleteByProjectIdAsync(int projectId);
}

public class TokenRepository : ITokenRepository
{
    private readonly TokenVaultDbContext _context;
    
    public TokenRepository(TokenVaultDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Token>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var tokens = new List<Token>();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.Id, t.ProjectId, t.TokenValue, t.TokenType, t.ExpiresAt, t.CreatedAt, t.UpdatedAt,
                   p.Name as ProjectName
            FROM Tokens t
            INNER JOIN Projects p ON t.ProjectId = p.Id
            ORDER BY t.UpdatedAt DESC
        ";
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tokens.Add(MapToken(reader));
        }
        
        return tokens;
    }
    
    public async Task<List<Token>> GetByProjectIdAsync(int projectId)
    {
        var connection = _context.GetConnection();
        var tokens = new List<Token>();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.Id, t.ProjectId, t.TokenValue, t.TokenType, t.ExpiresAt, t.CreatedAt, t.UpdatedAt,
                   p.Name as ProjectName
            FROM Tokens t
            INNER JOIN Projects p ON t.ProjectId = p.Id
            WHERE t.ProjectId = @ProjectId
            ORDER BY t.UpdatedAt DESC
        ";
        command.Parameters.AddWithValue("@ProjectId", projectId);
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tokens.Add(MapToken(reader));
        }
        
        return tokens;
    }
    
    public async Task<Token?> GetLatestByProjectNameAsync(string projectName)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.Id, t.ProjectId, t.TokenValue, t.TokenType, t.ExpiresAt, t.CreatedAt, t.UpdatedAt,
                   p.Name as ProjectName
            FROM Tokens t
            INNER JOIN Projects p ON t.ProjectId = p.Id
            WHERE p.Name = @ProjectName COLLATE NOCASE
            ORDER BY t.UpdatedAt DESC
            LIMIT 1
        ";
        command.Parameters.AddWithValue("@ProjectName", projectName);
        
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapToken(reader) : null;
    }
    
    public async Task<Token?> GetByIdAsync(int id)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT t.Id, t.ProjectId, t.TokenValue, t.TokenType, t.ExpiresAt, t.CreatedAt, t.UpdatedAt,
                   p.Name as ProjectName
            FROM Tokens t
            INNER JOIN Projects p ON t.ProjectId = p.Id
            WHERE t.Id = @Id
        ";
        command.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapToken(reader) : null;
    }
    
    public async Task<int> CreateAsync(Token token)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Tokens (ProjectId, TokenValue, TokenType, ExpiresAt, CreatedAt, UpdatedAt)
            VALUES (@ProjectId, @TokenValue, @TokenType, @ExpiresAt, datetime('now'), datetime('now'));
            SELECT last_insert_rowid();
        ";
        
        command.Parameters.AddWithValue("@ProjectId", token.ProjectId);
        command.Parameters.AddWithValue("@TokenValue", token.TokenValue);
        command.Parameters.AddWithValue("@TokenType", token.TokenType);
        command.Parameters.AddWithValue("@ExpiresAt", token.ExpiresAt.HasValue 
            ? token.ExpiresAt.Value.ToString("o") 
            : DBNull.Value);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
    
    /// <summary>
    /// Creates or updates a token for a project by name.
    /// If the project doesn't exist, it creates the project first.
    /// </summary>
    public async Task<int> UpsertByProjectNameAsync(string projectName, string tokenValue)
    {
        var connection = _context.GetConnection();
        
        // First, ensure project exists
        await using var projectCommand = connection.CreateCommand();
        projectCommand.CommandText = @"
            INSERT OR IGNORE INTO Projects (Name, Port, CreatedAt, UpdatedAt)
            VALUES (@Name, 0, datetime('now'), datetime('now'));
            
            SELECT Id FROM Projects WHERE Name = @Name COLLATE NOCASE;
        ";
        projectCommand.Parameters.AddWithValue("@Name", projectName);
        
        var projectIdResult = await projectCommand.ExecuteScalarAsync();
        var projectId = Convert.ToInt32(projectIdResult);
        
        // Delete existing tokens for this project (keep only latest)
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM Tokens WHERE ProjectId = @ProjectId";
        deleteCommand.Parameters.AddWithValue("@ProjectId", projectId);
        await deleteCommand.ExecuteNonQueryAsync();
        
        // Insert new token
        await using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO Tokens (ProjectId, TokenValue, TokenType, CreatedAt, UpdatedAt)
            VALUES (@ProjectId, @TokenValue, 'Bearer', datetime('now'), datetime('now'));
            SELECT last_insert_rowid();
        ";
        insertCommand.Parameters.AddWithValue("@ProjectId", projectId);
        insertCommand.Parameters.AddWithValue("@TokenValue", tokenValue);
        
        var result = await insertCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
    
    public async Task DeleteAsync(int id)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tokens WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task DeleteByProjectIdAsync(int projectId)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tokens WHERE ProjectId = @ProjectId";
        command.Parameters.AddWithValue("@ProjectId", projectId);
        
        await command.ExecuteNonQueryAsync();
    }
    
    private static Token MapToken(SqliteDataReader reader)
    {
        var token = new Token
        {
            Id = reader.GetInt32(0),
            ProjectId = reader.GetInt32(1),
            TokenValue = reader.GetString(2),
            TokenType = reader.GetString(3),
            ExpiresAt = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
            CreatedAt = DateTime.Parse(reader.GetString(5)),
            UpdatedAt = DateTime.Parse(reader.GetString(6))
        };
        
        // Map project name if available
        if (reader.FieldCount > 7)
        {
            token.Project = new Project { Name = reader.GetString(7) };
        }
        
        return token;
    }
}
