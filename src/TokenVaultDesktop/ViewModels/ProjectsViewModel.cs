using System.Collections.ObjectModel;
using System.Windows.Input;
using TokenVaultDesktop.Models;
using TokenVaultDesktop.Services;

namespace TokenVaultDesktop.ViewModels;

/// <summary>
/// ViewModel for the Projects management view
/// </summary>
public class ProjectsViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    
    private ObservableCollection<Project> _projects = new();
    private Project? _selectedProject;
    private bool _isLoading;
    private string _newProjectName = string.Empty;
    private int _newProjectPort = 5000;
    private string _newProjectBaseUrl = "http://localhost:5000";
    
    public ProjectsViewModel(IProjectService projectService)
    {
        _projectService = projectService;
        
        LoadProjectsCommand = new AsyncRelayCommand(LoadProjectsAsync);
        AddProjectCommand = new AsyncRelayCommand(AddProjectAsync, _ => CanAddProject);
        DeleteProjectCommand = new AsyncRelayCommand(DeleteProjectAsync);
        SaveProjectCommand = new AsyncRelayCommand(SaveProjectAsync);
    }
    
    #region Properties
    
    public ObservableCollection<Project> Projects
    {
        get => _projects;
        set => SetProperty(ref _projects, value);
    }
    
    public Project? SelectedProject
    {
        get => _selectedProject;
        set => SetProperty(ref _selectedProject, value);
    }
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    public string NewProjectName
    {
        get => _newProjectName;
        set
        {
            if (SetProperty(ref _newProjectName, value))
            {
                OnPropertyChanged(nameof(CanAddProject));
            }
        }
    }
    
    public int NewProjectPort
    {
        get => _newProjectPort;
        set
        {
            if (SetProperty(ref _newProjectPort, value))
            {
                NewProjectBaseUrl = $"http://localhost:{value}";
            }
        }
    }
    
    public string NewProjectBaseUrl
    {
        get => _newProjectBaseUrl;
        set => SetProperty(ref _newProjectBaseUrl, value);
    }
    
    public bool CanAddProject => !string.IsNullOrWhiteSpace(NewProjectName);
    
    #endregion
    
    #region Commands
    
    public ICommand LoadProjectsCommand { get; }
    public ICommand AddProjectCommand { get; }
    public ICommand DeleteProjectCommand { get; }
    public ICommand SaveProjectCommand { get; }
    
    #endregion
    
    #region Methods
    
    public async Task LoadProjectsAsync()
    {
        IsLoading = true;
        
        try
        {
            var projects = await _projectService.GetAllProjectsAsync();
            
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task AddProjectAsync(object? parameter)
    {
        if (!CanAddProject) return;
        
        var project = new Project
        {
            Name = NewProjectName.Trim(),
            Port = NewProjectPort,
            ApiBaseUrl = NewProjectBaseUrl
        };
        
        await _projectService.CreateOrUpdateProjectAsync(project);
        
        // Clear form
        NewProjectName = string.Empty;
        NewProjectPort = 5000;
        NewProjectBaseUrl = "http://localhost:5000";
        
        // Reload list
        await LoadProjectsAsync();
    }
    
    private async Task DeleteProjectAsync(object? parameter)
    {
        if (parameter is not Project project) return;
        
        var result = System.Windows.MessageBox.Show(
            $"Delete project '{project.Name}'?\n\nThis will also delete all associated tokens.",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            await _projectService.DeleteProjectAsync(project.Id);
            await LoadProjectsAsync();
        }
    }
    
    private async Task SaveProjectAsync(object? parameter)
    {
        if (parameter is not Project project) return;
        
        await _projectService.CreateOrUpdateProjectAsync(project);
        await LoadProjectsAsync();
    }
    
    #endregion
}
