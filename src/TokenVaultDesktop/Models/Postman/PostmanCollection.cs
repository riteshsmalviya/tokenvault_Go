using System.Text.Json.Serialization;

namespace TokenVaultDesktop.Models.Postman;

/// <summary>
/// Postman Collection v2.1 schema models for JSON serialization.
/// See: https://schema.getpostman.com/json/collection/v2.1.0/collection.json
/// </summary>

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
    
    [JsonPropertyName("event")]
    public List<PostmanEvent>? Events { get; set; }
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
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
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
    
    [JsonPropertyName("query")]
    public List<PostmanQueryParam>? Query { get; set; }
}

public class PostmanQueryParam
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }
}

public class PostmanAuth
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "bearer";
    
    [JsonPropertyName("bearer")]
    public List<PostmanAuthParam>? Bearer { get; set; }
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
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class PostmanHeader
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";
    
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }
}

public class PostmanBody
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "raw";
    
    [JsonPropertyName("raw")]
    public string? Raw { get; set; }
    
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

public class PostmanEvent
{
    [JsonPropertyName("listen")]
    public string Listen { get; set; } = "prerequest";
    
    [JsonPropertyName("script")]
    public PostmanScript Script { get; set; } = new();
}

public class PostmanScript
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text/javascript";
    
    [JsonPropertyName("exec")]
    public List<string> Exec { get; set; } = new();
}
