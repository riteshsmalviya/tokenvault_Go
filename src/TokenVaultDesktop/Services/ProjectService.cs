using TokenVaultDesktop.Data;
using TokenVaultDesktop.Models;

namespace TokenVaultDesktop.Services;

/// <summary>
/// Service for managing projects
/// </summary>
public interface IProjectService
{
    Task<List<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByNameAsync(string name);
    Task<Project?> GetProjectByPortAsync(int port);
    Task<Project> CreateOrUpdateProjectAsync(Project project);
    Task DeleteProjectAsync(int id);
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    
    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }
    
    public async Task<List<Project>> GetAllProjectsAsync()
    {
        return await _projectRepository.GetAllAsync();
    }
    
    public async Task<Project?> GetProjectByNameAsync(string name)
    {
        return await _projectRepository.GetByNameAsync(name);
    }
    
    public async Task<Project?> GetProjectByPortAsync(int port)
    {
        return await _projectRepository.GetByPortAsync(port);
    }
    
    public async Task<Project> CreateOrUpdateProjectAsync(Project project)
    {
        var existing = await _projectRepository.GetByNameAsync(project.Name);
        
        if (existing != null)
        {
            existing.Port = project.Port;
            existing.ApiBaseUrl = project.ApiBaseUrl;
            existing.Description = project.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _projectRepository.UpdateAsync(existing);
            return existing;
        }
        else
        {
            project.Id = await _projectRepository.CreateAsync(project);
            return project;
        }
    }
    
    public async Task DeleteProjectAsync(int id)
    {
        await _projectRepository.DeleteAsync(id);
    }
}
