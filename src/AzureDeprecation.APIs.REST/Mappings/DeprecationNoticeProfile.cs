using AutoMapper;

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
            .ForMember(d => d.DeprecationData, o => o.MapFrom(s => s.DeprecationInfo))
            .ForMember(d => d.PublishedNotice, o => o.MapFrom(s => s.PublishedNotice))
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id));
        
        CreateMap<Messages.Notice, Presentation.Notice>();
        CreateMap<Messages.ContactEntry, Presentation.ContactEntry>();
        CreateMap<Messages.Impact, Presentation.Impact>();
        CreateMap<Messages.TimeLineEntry, Presentation.TimeLineEntry>();
        CreateMap<Messages.ApiInfo, Presentation.ApiInfo>();
        CreateMap<Messages.DashboardInfo, Presentation.DashboardInfo>();
        
        CreateMap<Messages.DeprecationInfo, Presentation.DeprecationData>();
        CreateMap<Messages.PublishedNotice, Presentation.PublishedNotice>();
    }
}