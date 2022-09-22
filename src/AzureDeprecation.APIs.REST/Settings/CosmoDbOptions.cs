namespace AzureDeprecation.APIs.REST.Settings;

public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";

    public string DatabaseName { get; set; } = null!;
    
    public string ContainerName { get; set; } = null!;
    
    public string ConnectionString { get; set; } = null!;
}