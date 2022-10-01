using System.ComponentModel.DataAnnotations;
using AzureDeprecation.APIs.REST.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace AzureDeprecation.Tests.Unit;

[Trait("Category", "Unit")]
public class DeprecationsRequestModelTests
{
    [Fact]
    public void Parse_Pagination_Succeeds()
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.PageSizeQueryParameterName] = $"{100}",
            [DeprecationsRequestModel.PageOffsetQueryParameterName] = $"{15}"
        };

        var result = DeprecationsRequestModel.Parse(queryParams);
       
        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.NotNull(result.Filters);
        
        Assert.Equal(15, result.Pagination.Offset);
        Assert.Equal(100, result.Pagination.Limit);
    }
    
    [Fact]
    public void Filters_AndPagination_Succeeds()
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.PageSizeQueryParameterName] = $"{100}",
            [DeprecationsRequestModel.PageOffsetQueryParameterName] = $"{15}",
            [DeprecationsRequestModel.StatusQueryParameterName] = "Opened",
            [DeprecationsRequestModel.YearQueryParameterName] = "1990"
        };

        var result = DeprecationsRequestModel.Parse(queryParams);
       
        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.NotNull(result.Filters);
        
        Assert.Equal(15, result.Pagination.Offset);
        Assert.Equal(100, result.Pagination.Limit);
        Assert.Equal("Opened", result.Filters.Status.ToString());
        Assert.Equal("1990", result.Filters.Year.ToString());
    }
    
    [Theory]
    [InlineData("Opened")]
    [InlineData("Closed")]
    public void Parse_StatusFilters_Succeeds(string status)
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.StatusQueryParameterName] = status
        };

        var result = DeprecationsRequestModel.Parse(queryParams);
       
        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.NotNull(result.Filters);
        
        Assert.Equal(status, result.Filters.Status?.ToString());
    }
    
    [Theory]
    [InlineData(1990)]
    [InlineData(2020)]
    public void Parse_YearFilter_Succeeds(int year)
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.YearQueryParameterName] = $"{year}"
        };

        var result = DeprecationsRequestModel.Parse(queryParams);
       
        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.NotNull(result.Filters);
        
        Assert.Equal(year, result.Filters.Year);
    }

    [Fact]
    public void Parse_InvalidEnum_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.StatusQueryParameterName] = "Invalid_Status"
        };

        Assert.Throws<BadHttpRequestException>(() => DeprecationsRequestModel.Parse(queryParams));
    }
    
    [Fact]
    public void Parse_InvalidInteger_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.PageOffsetQueryParameterName] = "Invalid_Offset"
        };

        Assert.Throws<BadHttpRequestException>(() => DeprecationsRequestModel.Parse(queryParams));
    }
    
    [Fact]
    public void Parse_InvalidPageSize_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.PageSizeQueryParameterName] = $"{-1}"
        };

        Assert.Throws<BadHttpRequestException>(() => DeprecationsRequestModel.Parse(queryParams));
    }
    
    [Fact]
    public void Parse_InvalidOffset_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            [DeprecationsRequestModel.PageSizeQueryParameterName] = $"{-1}"
        };

        Assert.Throws<BadHttpRequestException>(() => DeprecationsRequestModel.Parse(queryParams));
    }
    
    [Fact]
    public void Parse_UnknownVariable_Skipped()
    {
        var queryParams = new QueryCollectionImpl
        {
            ["unknownQueryName"] = "test_value"
        };

        var result = DeprecationsRequestModel.Parse(queryParams);
        Assert.NotNull(result);
        Assert.NotNull(result.Filters);
        Assert.NotNull(result.Pagination);
    }

    public class QueryCollectionImpl : Dictionary<string, StringValues>, IQueryCollection
    {
        public new ICollection<string> Keys => base.Keys;
    }
}