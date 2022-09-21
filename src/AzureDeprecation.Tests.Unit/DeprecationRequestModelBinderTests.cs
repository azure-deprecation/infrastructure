using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.APIs.REST.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace AzureDeprecation.Tests.Unit;

[Trait("Category", "Unit")]
public class DeprecationRequestModelBinderTests
{
    [Fact]
    public void Parse_Pagination_Succeeds()
    {
        var queryParams = new QueryCollectionImpl
        {
            ["limit"] = $"{100}",
            ["offset"] = $"{15}"
        };

        var result = DeprecationRequestModelBinder.CreateModel(queryParams);
       
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
            ["limit"] = $"{100}",
            ["offset"] = $"{15}",
            ["filters.Status"] = "Opened",
            ["filters.Year"] = "1990"
        };

        var result = DeprecationRequestModelBinder.CreateModel(queryParams);
       
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
            ["filters.Status"] = status
        };

        var result = DeprecationRequestModelBinder.CreateModel(queryParams);
       
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
            ["filters.Year"] = $"{year}"
        };

        var result = DeprecationRequestModelBinder.CreateModel(queryParams);
       
        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.NotNull(result.Filters);
        
        Assert.Equal(year, result.Filters.Year);
    }
    
    [Theory]
    [MemberData(nameof(EnumsData))]
    public void Parse_EnumFilters_Succeeds(string propertyName, string value)
    {
        var queryParams = new QueryCollectionImpl
        {
            [$"filters.{propertyName}"] = $"{value}"
        };

        var result = DeprecationRequestModelBinder.CreateModel(queryParams);
       
        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.NotNull(result.Filters);

        var prop = result.Filters.GetType().GetProperties().Single(it => it.Name == propertyName);
        Assert.Equal(value, prop.GetValue(result.Filters)?.ToString());
    }

    [Fact]
    public void Parse_InvalidEnum_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            ["filters.Status"] = "Invalid_Status"
        };

        Assert.Throws<ValidationException>(() => DeprecationRequestModelBinder.CreateModel(queryParams));
    }
    
    [Fact]
    public void Parse_InvalidInteger_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            ["offset"] = "Invalid_Offset"
        };

        Assert.Throws<ValidationException>(() => DeprecationRequestModelBinder.CreateModel(queryParams));
    }
    
    [Fact]
    public void Parse_InvalidPageSize_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            ["limit"] = $"{10000}"
        };

        Assert.Throws<ValidationException>(() => DeprecationRequestModelBinder.CreateModel(queryParams));
    }
    
    [Fact]
    public void Parse_InvalidOffset_Error()
    {
        var queryParams = new QueryCollectionImpl
        {
            ["limit"] = $"{-1}"
        };

        Assert.Throws<ValidationException>(() => DeprecationRequestModelBinder.CreateModel(queryParams));
    }
    
    [Fact]
    public void Parse_UnknownVariable_Skipped()
    {
        var queryParams = new QueryCollectionImpl
        {
            ["unknownQueryName"] = "test_value"
        };

        var result = DeprecationRequestModelBinder.CreateModel(queryParams);
        Assert.NotNull(result);
        Assert.NotNull(result.Filters);
        Assert.NotNull(result.Pagination);
    }

    public static IEnumerable<object[]> EnumsData
    {
        get
        {
            var propNames = typeof(FilterNoticesRequest).GetProperties()
                .Where(it => Nullable.GetUnderlyingType(it.PropertyType)?.IsEnum ?? false)
                .Select(it => (it.Name, Nullable.GetUnderlyingType(it.PropertyType)) );
            
            foreach (var (propName, enumType) in propNames)
            {
                foreach (var enumValue in Enum.GetNames(enumType))
                {
                    yield return new object[] { propName, enumValue };
                }
            }
        }
    }

    public class QueryCollectionImpl : Dictionary<string, StringValues>, IQueryCollection
    {
        public new ICollection<string> Keys => base.Keys;
    }
}