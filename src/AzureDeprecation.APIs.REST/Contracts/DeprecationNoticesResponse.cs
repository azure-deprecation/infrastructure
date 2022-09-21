using System.Collections.Generic;

namespace AzureDeprecation.APIs.REST.Contracts;

public class DeprecationNoticesResponse
{
    public IReadOnlyList<DeprecationInfo> Deprecations { get; set; }
}