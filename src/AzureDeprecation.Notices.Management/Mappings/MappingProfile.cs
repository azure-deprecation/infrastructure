using AutoMapper;
using AzureDeprecation.Contracts.v1.Documents;
using AzureDeprecation.Contracts.v1.Messages;
using AzureDeprecation.Contracts.v1.Shared;
using Octokit;
using ApiInfo = AzureDeprecation.Contracts.v1.Shared.ApiInfo;

namespace AzureDeprecation.Notices.Management.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DraftNotice, Notice>()
                .ForMember(notice => notice.Description, x => x.MapFrom(draftNotice => draftNotice.Description))
                .ForMember(notice => notice.Links, x => x.MapFrom(draftNotice => draftNotice.Links));
            CreateMap<NewAzureDeprecationV1Message, DeprecationInfo>()
                .ForMember(deprecationInfo => deprecationInfo.Title, x => x.MapFrom(issue => issue.Title))
                .ForMember(deprecationInfo => deprecationInfo.RequiredAction, x => x.MapFrom((issue, _) => issue.RequiredAction?.Description))
                .ForMember(deprecationInfo => deprecationInfo.Contact, x => x.MapFrom(issue => issue.Contact))
                .ForMember(deprecationInfo => deprecationInfo.Notice, x => x.MapFrom(issue => issue.Notice))
                .ForMember(deprecationInfo => deprecationInfo.Impact, x => x.MapFrom(issue => issue.Impact))
                .ForMember(deprecationInfo => deprecationInfo.Timeline, x => x.MapFrom(issue => issue.Timeline))
                .ForMember(deprecationInfo => deprecationInfo.AdditionalInformation, x => x.MapFrom(issue => issue.AdditionalInformation));
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
            CreateMap<NewDeprecationNoticePublishedV1Message, DeprecationNoticeDocument>();
        }
    }
}