using AutoMapper;
using CodeJam.Strings;

namespace AzureDeprecation.APIs.REST.Mappings;

using Presentation = Contracts;
using Db = DataAccess.Models;
using Messages = AzureDeprecation.Contracts.Messages.v1;

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
                    if (x.PublishedNotice?.DashboardInfo?.Url.IsNullOrEmpty() ?? true)
                        return new Dictionary<Presentation.ExternalLinkType, string?>();
                    
                    return new Dictionary<Presentation.ExternalLinkType, string?>
                    {
                        { Presentation.ExternalLinkType.GitHubNoticeUrl, x.PublishedNotice!.DashboardInfo!.Url }
                    };
                }));
        
        CreateMap<Messages.DeprecationInfo, Presentation.DeprecationInfo>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Links, opt => opt.Ignore());
        
        CreateMap<Messages.Notice, Presentation.Notice>();
        CreateMap<Messages.ContactEntry, Presentation.ContactEntry>();
        CreateMap<Messages.Impact, Presentation.Impact>();
        CreateMap<Messages.TimeLineEntry, Presentation.TimeLineEntry>();

    }
}