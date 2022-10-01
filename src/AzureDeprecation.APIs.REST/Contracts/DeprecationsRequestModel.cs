using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.Contracts.Enum;
using Microsoft.AspNetCore.Http;

namespace AzureDeprecation.APIs.REST.Contracts;

public class DeprecationsRequestModel
{
    public FilterNoticesRequest Filters { get; set; } = new();
    public PaginationNoticesRequest Pagination { get; set; } = new();

    public const string StatusQueryParameterName = "status";
    public const string ImpactAreaQueryParameterName = "impactArea";
    public const string YearQueryParameterName = "deprecationYear";
    public const string AzureServiceQueryParameterName = "azureService";
    public const string ImpactTypeQueryParameterName = "impactType";
    public const string CloudQueryParameterName = "cloud";
    public const string PageOffsetQueryParameterName = "pageOffset";
    public const string PageSizeQueryParameterName = "pageSize";

    public static DeprecationsRequestModel Parse(IQueryCollection queryParameters)
    {
        return new DeprecationsRequestModel
        {
            Filters = ReadFilters(queryParameters),
            Pagination = ReadPagination(queryParameters)
        };
    }

    static PaginationNoticesRequest ReadPagination(IQueryCollection requestQuery)
    {
        var paginationInfo = new PaginationNoticesRequest();

        var configuredPageLimit = GetQueryParameterForInteger(PageSizeQueryParameterName, requestQuery);
        if (configuredPageLimit != null)
        {
            if (configuredPageLimit.Value <= 0)
            {
                throw new BadHttpRequestException("Page limit has to be higher than 0");
            }

            paginationInfo.Limit = configuredPageLimit.Value;
        }

        var configuredPageOffset = GetQueryParameterForInteger(PageOffsetQueryParameterName, requestQuery);
        if (configuredPageOffset != null)
        {
            if (configuredPageOffset.Value <= 0)
            {
                throw new BadHttpRequestException("Page offset has to be higher than 0");
            }

            paginationInfo.Offset = configuredPageOffset.Value;
        }

        return paginationInfo;
    }

    static FilterNoticesRequest ReadFilters(IQueryCollection requestQuery)
    {
        var noticeFilters = new FilterNoticesRequest
        {
            Area = GetQueryParameterForEnum<ImpactArea>(ImpactAreaQueryParameterName, requestQuery),
            Cloud = GetQueryParameterForEnum<AzureCloud>(CloudQueryParameterName, requestQuery),
            ImpactType = GetQueryParameterForEnum<ImpactType>(ImpactTypeQueryParameterName, requestQuery),
            AzureService = GetQueryParameterForEnum<AzureService>(AzureServiceQueryParameterName, requestQuery),
            Status = GetQueryParameterForEnum<StatusFilter>(StatusQueryParameterName, requestQuery),
            Year = GetQueryParameterForInteger(YearQueryParameterName, requestQuery)
        };

        return noticeFilters;
    }

    static TEnum? GetQueryParameterForEnum<TEnum>(string queryParameterName, IQueryCollection requestQuery)
        where TEnum : struct, Enum
    {
        if (!requestQuery.ContainsKey(queryParameterName))
        {
            return null;
        }

        if (Enum.TryParse(requestQuery[queryParameterName], ignoreCase: true, out TEnum parameterValue) == false)
        {
            var allowedValues = Enum.GetValues<TEnum>();
            throw new BadHttpRequestException($"Value of {queryParameterName} is not valid. Allowed values are {string.Join(", ", allowedValues)}");
        }

        return parameterValue;
    }

    static int? GetQueryParameterForInteger(string queryParameterName, IQueryCollection requestQuery)
    {
        if (!requestQuery.ContainsKey(queryParameterName))
        {
            return null;
        }

        if (int.TryParse(requestQuery[queryParameterName], out var parameterValue) == false)
        {
            throw new BadHttpRequestException($"Value of {queryParameterName} is not a valid integer");
        }

        return parameterValue;
    }
}