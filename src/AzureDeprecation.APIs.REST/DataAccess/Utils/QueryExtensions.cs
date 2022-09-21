using AzureDeprecation.APIs.REST.DataAccess.Models;

namespace AzureDeprecation.APIs.REST.DataAccess.Utils;

public static class QueryExtensions
{
    public static IQueryable<NoticeEntity> WithPagination(this IQueryable<NoticeEntity> queryBuilder,
        PaginationNoticesRequest pagination)
    {
        return queryBuilder
            .Skip(pagination.Offset)
            .Take(pagination.Limit);
    }

    public static IQueryable<NoticeEntity> WithFilters(this IQueryable<NoticeEntity> queryBuilder, FilterNoticesRequest filters)
    {
        if (filters.Status != null)
        {
            if (filters.Status == StatusFilter.Opened)
                queryBuilder = queryBuilder.Where(it => it.PublishedNotice.ClosedAt == null);
            else
                queryBuilder = queryBuilder.Where(it => it.PublishedNotice.ClosedAt != null);
        }

        if (filters.Area != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo.Impact!.Area == filters.Area.Value);
        }
        
        if (filters.Cloud != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo.Impact!.Cloud == filters.Cloud);
        }
        
        if (filters.ImpactType != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo.Impact!.Type == filters.ImpactType);
        }
        
        if (filters.Service != null)
        {
            queryBuilder = queryBuilder.Where(it => it.DeprecationInfo.Impact!.Services.Contains(filters.Service.Value));
        }

        if (filters.Year != null)
        {
            // TODO: implement logic for filtering by year (maybe need to add additional field there)
        }

        return queryBuilder;
    }
}