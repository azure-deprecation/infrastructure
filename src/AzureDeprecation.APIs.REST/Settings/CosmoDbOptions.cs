namespace AzureDeprecation.APIs.REST.Settings;

public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";
    
    public string? DbName { get; set; }
    
    public string? ContainerId { get; set; }
    
    public string? ConnectionString { get; set; }
}