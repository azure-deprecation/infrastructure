using System.Linq;
using AutoMapper;
using AzureDeprecation.Contracts.Messages.v1;
using Octokit;
using ApiInfo = AzureDeprecation.Contracts.Messages.v1.ApiInfo;

namespace AzureDeprecation.Notices.Management.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Issue, ApiInfo>()
                .ForMember(apiInfo => apiInfo.Id, x => x.MapFrom(issue => issue.Id))
                .ForMember(apiInfo => apiInfo.Url, x => x.MapFrom(issue => issue.Url));
            CreateMap<Issue, DashboardInfo>()
                .ForMember(dashboardInfo => dashboardInfo.Id, x => x.MapFrom(issue => issue.Number))
                .ForMember(dashboardInfo => dashboardInfo.Url, x => x.MapFrom(issue => issue.HtmlUrl));
            CreateMap<Issue, PublishedNotice>()
                .ForMember(notice => notice.Title, x => x.MapFrom(issue => issue.Title))
                .ForMember(notice => notice.ClosedAt, x => x.MapFrom(issue => issue.ClosedAt))
                .ForMember(notice => notice.CreatedAt, x => x.MapFrom(issue => issue.CreatedAt))
                .ForMember(notice => notice.UpdatedAt, x => x.MapFrom(issue => issue.UpdatedAt))
                .ForMember(notice => notice.ApiInfo, x => x.MapFrom(issue => issue))
                .ForMember(notice => notice.DashboardInfo, x => x.MapFrom(issue => issue))
                .ForMember(notice => notice.Labels, x => x.MapFrom(issue => issue.Labels.Select(s=>s.Name)));
        }
    }
}
