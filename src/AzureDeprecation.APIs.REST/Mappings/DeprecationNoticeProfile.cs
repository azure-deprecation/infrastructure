using AutoMapper;
using CodeJam.Strings;

namespace AzureDeprecation.APIs.REST.Mappings;

using Presentation = Contracts;
using Db = DataAccess.Models;
using Shared = AzureDeprecation.Contracts.v1.Shared;

public class DeprecationNoticeProfile : Profile
{
    public DeprecationNoticeProfile()
    {
        CreateMap<Db.DeprecationNoticesResult, Presentation.DeprecationNoticesResponse>();
        
        CreateMap<Db.NoticeEntity, Presentation.DeprecationInfo>()
            .IncludeMembers(i => i.DeprecationInfo)
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Links, o => 
                o.MapFrom((x, _) =>
                {
                    if (x.PublishedNotice?.DashboardInfo?.Url.IsNullOrEmpty() == true)
                    {
                        return new Dictionary<Presentation.ExternalLinkType, string>();
                    }

                    return new Dictionary<Presentation.ExternalLinkType, string>
                    {
                        { Presentation.ExternalLinkType.GitHubNoticeUrl, x.PublishedNotice!.DashboardInfo!.Url! }
                    };
                }));
        
        CreateMap<Shared.DeprecationInfo, Presentation.DeprecationInfo>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Links, opt => opt.Ignore());
        
        CreateMap<Shared.Notice, Presentation.Notice>();
        CreateMap<Shared.ContactEntry, Presentation.ContactEntry>();
        CreateMap<Shared.Impact, Presentation.Impact>();
        CreateMap<Shared.TimeLineEntry, Presentation.TimeLineEntry>();

    }
}