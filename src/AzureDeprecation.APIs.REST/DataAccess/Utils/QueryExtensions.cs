using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.Contracts.v1.Documents;

namespace AzureDeprecation.APIs.REST.DataAccess.Utils;

public static class QueryExtensions
{
    public static IQueryable<DeprecationNoticeDocument> WithPagination(this IQueryable<DeprecationNoticeDocument> queryBuilder,
        PaginationNoticesRequest pagination)
    {
        return queryBuilder
            .Skip(pagination.Offset)
            .Take(pagination.Limit);
    }

    public static IQueryable<DeprecationNoticeDocument> WithFilters(this IQueryable<DeprecationNoticeDocument> queryBuilder, FilterNoticesRequest filters)
    {
        if (filters.Status != null)
        {
            if (filters.Status == StatusFilter.Opened)
                queryBuilder = queryBuilder.Where(it => it.PublishedNotice != null && it.PublishedNotice.ClosedAt == null);
            else
                queryBuilder = queryBuilder.Where(it => it.PublishedNotice != null && it.PublishedNotice.ClosedAt != null);
        }

        if (filters.Area != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo != null
                                              && it.DeprecationInfo.Impact!.Area == filters.Area.Value);
        }
        
        if (filters.Cloud != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo != null
                                              && it.DeprecationInfo.Impact!.Cloud == filters.Cloud);
        }

        if (filters.ImpactType != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo != null
                                                    && it.DeprecationInfo.Impact!.Type == filters.ImpactType);
        }

        if (filters.AzureService != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo != null
                                                    && it.DeprecationInfo.Impact!.Services.Contains(filters.AzureService.Value));
        }

        if (filters.Year != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo != null
                                              && it.DeprecationInfo.Timeline!.Any(x => x.IsDueDate == true && x.Date.Year == filters.Year.Value));
        }

        return queryBuilder;
    }
}