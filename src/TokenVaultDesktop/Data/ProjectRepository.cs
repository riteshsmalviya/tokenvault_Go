using Microsoft.Data.Sqlite;
using TokenVaultDesktop.Models;

namespace TokenVaultDesktop.Data;

/// <summary>
/// Repository for Project entities
/// </summary>
public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(int id);
    Task<Project?> GetByNameAsync(string name);
    Task<Project?> GetByPortAsync(int port);
    Task<int> CreateAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(int id);
}

public class ProjectRepository : IProjectRepository
{
    private readonly TokenVaultDbContext _context;
    
    public ProjectRepository(TokenVaultDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Project>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var projects = new List<Project>();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Port, ApiBaseUrl, Description, CreatedAt, UpdatedAt FROM Projects ORDER BY Name";
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            projects.Add(MapProject(reader));
        }
        
        return projects;
    }
    
    public async Task<Project?> GetByIdAsync(int id)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Port, ApiBaseUrl, Description, CreatedAt, UpdatedAt FROM Projects WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProject(reader) : null;
    }
    
    public async Task<Project?> GetByNameAsync(string name)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Port, ApiBaseUrl, Description, CreatedAt, UpdatedAt FROM Projects WHERE Name = @Name COLLATE NOCASE";
        command.Parameters.AddWithValue("@Name", name);
        
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProject(reader) : null;
    }
    
    public async Task<Project?> GetByPortAsync(int port)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Port, ApiBaseUrl, Description, CreatedAt, UpdatedAt FROM Projects WHERE Port = @Port";
        command.Parameters.AddWithValue("@Port", port);
        
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProject(reader) : null;
    }
    
    public async Task<int> CreateAsync(Project project)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Projects (Name, Port, ApiBaseUrl, Description, CreatedAt, UpdatedAt)
            VALUES (@Name, @Port, @ApiBaseUrl, @Description, datetime('now'), datetime('now'));
            SELECT last_insert_rowid();
        ";
        
        command.Parameters.AddWithValue("@Name", project.Name);
        command.Parameters.AddWithValue("@Port", project.Port);
        command.Parameters.AddWithValue("@ApiBaseUrl", (object?)project.ApiBaseUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)project.Description ?? DBNull.Value);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
    
    public async Task UpdateAsync(Project project)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Projects 
            SET Name = @Name, 
                Port = @Port, 
                ApiBaseUrl = @ApiBaseUrl, 
                Description = @Description,
                UpdatedAt = datetime('now')
            WHERE Id = @Id
        ";
        
        command.Parameters.AddWithValue("@Id", project.Id);
        command.Parameters.AddWithValue("@Name", project.Name);
        command.Parameters.AddWithValue("@Port", project.Port);
        command.Parameters.AddWithValue("@ApiBaseUrl", (object?)project.ApiBaseUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)project.Description ?? DBNull.Value);
        
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Projects WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        
        await command.ExecuteNonQueryAsync();
    }
    
    private static Project MapProject(SqliteDataReader reader)
    {
        return new Project
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Port = reader.GetInt32(2),
            ApiBaseUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
            Description = reader.IsDBNull(4) ? null : reader.GetString(4),
            CreatedAt = DateTime.Parse(reader.GetString(5)),
            UpdatedAt = DateTime.Parse(reader.GetString(6))
        };
    }
}
