using System;
using System.ComponentModel.DataAnnotations;

namespace AzureDeprecation.APIs.REST.DataAccess.Models;

public class PaginationNoticesRequest
{
    public const int DefaultPageSize = 100;
    public const int MaxPageSize = 500;
    
    [Range(0, Int32.MaxValue)]
    public int Offset { get; set; } = 0;
    
    [Range(1, MaxPageSize)]
    public int Limit { get; set; } = DefaultPageSize;
}