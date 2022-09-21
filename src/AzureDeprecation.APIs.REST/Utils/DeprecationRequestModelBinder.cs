using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using CodeJam;
using CodeJam.Collections;
using CodeJam.Reflection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AzureDeprecation.APIs.REST.Utils;

public static class DeprecationRequestModelBinder
{
    public static DeprecationsRequestModel CreateModel(IQueryCollection requestQuery)
    {
        return new DeprecationsRequestModel
        {
            Filters = ReadFilters(requestQuery),
            Pagination = ReadPagination(requestQuery)
        };
    }

    static PaginationNoticesRequest ReadPagination(IQueryCollection requestQuery)
    {
        return ReadByProperties<PaginationNoticesRequest>(requestQuery, prefix: null);
    }

    static FilterNoticesRequest ReadFilters(IQueryCollection requestQuery)
    {
        return ReadByProperties<FilterNoticesRequest>(requestQuery, prefix: nameof(DeprecationsRequestModel.Filters));
    }

    static TResult ReadByProperties<TResult>(IQueryCollection requestQuery, string? prefix) where TResult : new()
    {
        var result = new TResult();
        
        var filterProperties = requestQuery
            .Where(it => prefix == null || it.Key.ToLower().StartsWith(prefix.ToLower() + "."))
            .Where(x => x.Key.Split('.', StringSplitOptions.RemoveEmptyEntries).Length == (prefix == null ? 1 : 2))
            .ToDictionary(k => k.Key.Split('.').Last(), v => v.Value);

        if (!filterProperties.Any())
            return result;

        var objectProps = typeof(TResult)
            .GetProperties()
            .ToDictionary(x => x.Name, v => v, StringComparer.OrdinalIgnoreCase);
        
        foreach (var filterProperty in filterProperties)
        {
            if (!objectProps.TryGetValue(filterProperty.Key, out var propertyInfo))
                continue;

            if (filterProperty.Value.IsNullOrEmpty())
                continue;

            if (propertyInfo.PropertyType.IsArray)
            {
                propertyInfo.SetValue(result, filterProperty
                    .Value
                    .Select(x => GetValue(x, filterProperty.Key, propertyInfo))
                    .ToArray());
                
                continue;
            }
            
            Code.ValidCount(1, filterProperty.Value);
            propertyInfo.SetValue(result, GetValue(filterProperty.Value.Single(), filterProperty.Key, propertyInfo));
        }

        return result;
    }

    static object? GetValue(string queryValue, string queryVariable, PropertyInfo property)
    {
        object? result;
        try
        {
            result = TypeConverter.Convert(queryValue, property.PropertyType);
        }
        catch
        {
            throw new ValidationException($"Error on parsing value: {queryValue} of query: {queryVariable}");
        }
        
        var validationAttributes = property.GetCustomAttributes<ValidationAttribute>();
        foreach (var validationAttribute in validationAttributes)
        {
            validationAttribute.Validate(result, queryVariable);
        }

        return result;
    }
    
    static class TypeConverter
    {
        public static object? Convert(string value, Type destinationType)
        {
            var nullableType = Nullable.GetUnderlyingType(destinationType);
            if (nullableType != null)
                return Convert(value, nullableType);
            
            if (destinationType == typeof(string))
                return value;

            if (destinationType.IsEnum)
            {
                var isEnum = Enum.TryParse(destinationType, value, ignoreCase: true, out var result);
                Code.AssertState(isEnum, "Invalid enum value");
                return result;
            }
            
            if (destinationType.IsInteger())
            {
                var isInt = Int32.TryParse(value, out var result);
                Code.AssertState(isInt, "Invalid int value");
                return result;
            }

            return JsonConvert.DeserializeObject(value, destinationType);
        }
    }
}