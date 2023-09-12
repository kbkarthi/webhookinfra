using AutoMapper;
using SampleWebApiAspNetCore.Dtos;
using SampleWebApiAspNetCore.Entities;

namespace SampleWebApiAspNetCore.MappingProfiles
{
    public class WebhookMappings : Profile
    {
        public WebhookMappings()
        {
            CreateMap<WebhookEntity, WebhookDto>().ReverseMap();
            CreateMap<WebhookEntity, WebhookCreateDto>().ReverseMap();
        }
    }
}
