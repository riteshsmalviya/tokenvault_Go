using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using TokenVaultDesktop.Models;
using TokenVaultDesktop.Services;

namespace TokenVaultDesktop.ViewModels;

/// <summary>
/// ViewModel for an endpoint entry in the generator
/// </summary>
public class EndpointViewModel : ViewModelBase
{
    private string _method = "GET";
    private string _path = string.Empty;
    private string _name = string.Empty;
    private bool _requiresAuth = true;
    private string? _requestBody;
    
    public string Method
    {
        get => _method;
        set => SetProperty(ref _method, value);
    }
    
    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public bool RequiresAuth
    {
        get => _requiresAuth;
        set => SetProperty(ref _requiresAuth, value);
    }
    
    public string? RequestBody
    {
        get => _requestBody;
        set => SetProperty(ref _requestBody, value);
    }
    
    public List<string> AvailableMethods { get; } = new() { "GET", "POST", "PUT", "DELETE", "PATCH" };
}

/// <summary>
/// ViewModel for the Postman Collection Generator view
/// </summary>
public class PostmanGeneratorViewModel : ViewModelBase
{
    private readonly IPostmanGenerator _postmanGenerator;
    private readonly IProjectService _projectService;
    
    private string _projectName = string.Empty;
    private int _port = 5000;
    private string _apiBaseUrl = "http://localhost:5000";
    private bool _includeTokenVaultScript = true;
    private string _statusMessage = string.Empty;
    private bool _isGenerating;
    
    public PostmanGeneratorViewModel(
        IPostmanGenerator postmanGenerator,
        IProjectService projectService)
    {
        _postmanGenerator = postmanGenerator;
        _projectService = projectService;
        
        Endpoints = new ObservableCollection<EndpointViewModel>();
        
        AddEndpointCommand = new RelayCommand(AddEndpoint);
        RemoveEndpointCommand = new RelayCommand(RemoveEndpoint);
        GenerateCollectionCommand = new AsyncRelayCommand(GenerateCollectionAsync, _ => CanGenerate);
        LoadProjectCommand = new AsyncRelayCommand(LoadProjectAsync);
    }
    
    #region Properties
    
    public string ProjectName
    {
        get => _projectName;
        set
        {
            if (SetProperty(ref _projectName, value))
            {
                OnPropertyChanged(nameof(CanGenerate));
            }
        }
    }
    
    public int Port
    {
        get => _port;
        set
        {
            if (SetProperty(ref _port, value))
            {
                // Auto-update base URL
                if (ApiBaseUrl.StartsWith("http://localhost:"))
                {
                    ApiBaseUrl = $"http://localhost:{value}";
                }
            }
        }
    }
    
    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set
        {
            if (SetProperty(ref _apiBaseUrl, value))
            {
                OnPropertyChanged(nameof(CanGenerate));
            }
        }
    }
    
    public bool IncludeTokenVaultScript
    {
        get => _includeTokenVaultScript;
        set => SetProperty(ref _includeTokenVaultScript, value);
    }
    
    public ObservableCollection<EndpointViewModel> Endpoints { get; }
    
    public bool HasEndpoints => Endpoints.Count > 0;
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    
    public bool IsGenerating
    {
        get => _isGenerating;
        set
        {
            if (SetProperty(ref _isGenerating, value))
            {
                OnPropertyChanged(nameof(CanGenerate));
            }
        }
    }
    
    public bool CanGenerate => 
        !IsGenerating && 
        !string.IsNullOrWhiteSpace(ProjectName) && 
        !string.IsNullOrWhiteSpace(ApiBaseUrl);
    
    #endregion
    
    #region Commands
    
    public ICommand AddEndpointCommand { get; }
    public ICommand RemoveEndpointCommand { get; }
    public ICommand GenerateCollectionCommand { get; }
    public ICommand LoadProjectCommand { get; }
    
    #endregion
    
    #region Methods
    
    private void AddEndpoint(object? parameter)
    {
        var endpoint = new EndpointViewModel
        {
            Method = "GET",
            Path = "/api/",
            Name = $"Request {Endpoints.Count + 1}"
        };
        
        Endpoints.Add(endpoint);
        OnPropertyChanged(nameof(HasEndpoints));
    }
    
    private void RemoveEndpoint(object? parameter)
    {
        if (parameter is EndpointViewModel endpoint)
        {
            Endpoints.Remove(endpoint);
            OnPropertyChanged(nameof(HasEndpoints));
        }
    }
    
    private async Task LoadProjectAsync(object? parameter)
    {
        if (parameter is Project project)
        {
            ProjectName = project.Name;
            Port = project.Port;
            ApiBaseUrl = project.ApiBaseUrl ?? $"http://localhost:{project.Port}";
        }
        else if (parameter is string projectName)
        {
            var existingProject = await _projectService.GetProjectByNameAsync(projectName);
            if (existingProject != null)
            {
                ProjectName = existingProject.Name;
                Port = existingProject.Port;
                ApiBaseUrl = existingProject.ApiBaseUrl ?? $"http://localhost:{existingProject.Port}";
            }
        }
    }
    
    private async Task GenerateCollectionAsync(object? parameter)
    {
        if (!CanGenerate) return;
        
        IsGenerating = true;
        StatusMessage = "Generating collection...";
        
        try
        {
            // Build request
            var request = new PostmanGenerationRequest
            {
                ProjectName = ProjectName,
                Port = Port,
                ApiBaseUrl = ApiBaseUrl,
                IncludeTokenVaultScript = IncludeTokenVaultScript,
                Endpoints = Endpoints.Select(e => new EndpointDefinition
                {
                    Name = e.Name,
                    Method = e.Method,
                    Path = e.Path,
                    RequiresAuth = e.RequiresAuth,
                    RequestBody = e.RequestBody
                }).ToList()
            };
            
            // Generate collection
            var collection = _postmanGenerator.GenerateCollection(request);
            
            // Show save dialog
            var saveDialog = new SaveFileDialog
            {
                FileName = $"{ProjectName.Replace(" ", "_")}_postman_collection.json",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Save Postman Collection",
                DefaultExt = ".json"
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                await _postmanGenerator.SaveToFileAsync(collection, saveDialog.FileName);
                
                // Save project to database for future use
                await _projectService.CreateOrUpdateProjectAsync(new Project
                {
                    Name = ProjectName,
                    Port = Port,
                    ApiBaseUrl = ApiBaseUrl
                });
                
                StatusMessage = $"✓ Collection saved: {saveDialog.FileName}";
                
                System.Windows.MessageBox.Show(
                    $"Postman collection saved successfully!\n\n" +
                    $"File: {saveDialog.FileName}\n\n" +
                    $"To import:\n" +
                    $"1. Open Postman\n" +
                    $"2. Click 'Import'\n" +
                    $"3. Select the generated file\n\n" +
                    $"Your collection includes:\n" +
                    $"• Bearer token authentication ({{{{token}}}})\n" +
                    $"• TokenVault auto-fetch script\n" +
                    $"• {(Endpoints.Count > 0 ? Endpoints.Count : 3)} request(s)",
                    "Collection Generated",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "Generation cancelled";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Error: {ex.Message}";
            
            System.Windows.MessageBox.Show(
                $"Failed to generate collection:\n\n{ex.Message}",
                "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsGenerating = false;
        }
    }
    
    #endregion
}
