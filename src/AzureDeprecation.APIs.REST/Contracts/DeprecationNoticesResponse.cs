namespace AzureDeprecation.APIs.REST.Contracts;

public class DeprecationNoticesResponse
{
    public IReadOnlyList<DeprecationInfo> Deprecations { get; set; } = new List<DeprecationInfo>();
}