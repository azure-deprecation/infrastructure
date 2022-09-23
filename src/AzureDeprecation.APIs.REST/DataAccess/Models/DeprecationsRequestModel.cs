namespace AzureDeprecation.APIs.REST.DataAccess.Models;

public class DeprecationsRequestModel
{
    public FilterNoticesRequest Filters { get; set; } = new();

    public PaginationNoticesRequest Pagination { get; set; } = new();
}