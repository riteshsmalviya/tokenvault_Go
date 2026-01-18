using TokenVaultDesktop.Data;
using TokenVaultDesktop.Models;

namespace TokenVaultDesktop.Services;

/// <summary>
/// Service for managing tokens
/// </summary>
public interface ITokenService
{
    Task<List<Token>> GetAllTokensAsync();
    Task<Token?> GetTokenAsync(string projectName);
    Task StoreTokenAsync(string projectName, string tokenValue);
    Task DeleteTokenAsync(int tokenId);
}

public class TokenService : ITokenService
{
    private readonly ITokenRepository _tokenRepository;
    
    public TokenService(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }
    
    public async Task<List<Token>> GetAllTokensAsync()
    {
        return await _tokenRepository.GetAllAsync();
    }
    
    public async Task<Token?> GetTokenAsync(string projectName)
    {
        return await _tokenRepository.GetLatestByProjectNameAsync(projectName);
    }
    
    public async Task StoreTokenAsync(string projectName, string tokenValue)
    {
        await _tokenRepository.UpsertByProjectNameAsync(projectName, tokenValue);
    }
    
    public async Task DeleteTokenAsync(int tokenId)
    {
        await _tokenRepository.DeleteAsync(tokenId);
    }
}
