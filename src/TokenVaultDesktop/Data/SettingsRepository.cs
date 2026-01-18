using Microsoft.Data.Sqlite;

namespace TokenVaultDesktop.Data;

/// <summary>
/// Repository for application settings
/// </summary>
public interface ISettingsRepository
{
    Task<string?> GetAsync(string key);
    Task<T> GetAsync<T>(string key, T defaultValue);
    Task SetAsync(string key, string value);
    Task<Dictionary<string, string>> GetAllAsync();
}

public class SettingsRepository : ISettingsRepository
{
    private readonly TokenVaultDbContext _context;
    
    public SettingsRepository(TokenVaultDbContext context)
    {
        _context = context;
    }
    
    public async Task<string?> GetAsync(string key)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Settings WHERE Key = @Key";
        command.Parameters.AddWithValue("@Key", key);
        
        var result = await command.ExecuteScalarAsync();
        return result?.ToString();
    }
    
    public async Task<T> GetAsync<T>(string key, T defaultValue)
    {
        var value = await GetAsync(key);
        
        if (string.IsNullOrEmpty(value))
            return defaultValue;
        
        try
        {
            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(value);
            if (typeof(T) == typeof(bool))
                return (T)(object)bool.Parse(value);
            if (typeof(T) == typeof(string))
                return (T)(object)value;
                
            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
    
    public async Task SetAsync(string key, string value)
    {
        var connection = _context.GetConnection();
        
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Settings (Key, Value) VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value
        ";
        command.Parameters.AddWithValue("@Key", key);
        command.Parameters.AddWithValue("@Value", value);
        
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var settings = new Dictionary<string, string>();
        
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Key, Value FROM Settings";
        
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            settings[reader.GetString(0)] = reader.GetString(1);
        }
        
        return settings;
    }
}
