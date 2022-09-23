using AzureDeprecation.Contracts.Enum;

namespace AzureDeprecation.APIs.REST.DataAccess.Models;

public class FilterNoticesRequest
{
    public StatusFilter? Status { get; set; }
    
    public int? Year { get; set; }
    
    public AzureService? Service { get; set; }
    
    public ImpactType? ImpactType { get; set; }
    
    public AzureCloud? Cloud { get; set; }
    
    public ImpactArea? Area { get; set; }
}