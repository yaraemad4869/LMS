using AutoMapper;
using LearningManagementSystem.Models;
namespace LearningManagementSystem.Mapping
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CreateMap<Source, Destination>();
            //CreateMap<Certificate, CertificateD>();
            //CreateMap<ProductDto, Product>();

            // For more complex mappings
            // CreateMap<Source, Destination>()
            //     .ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.SourceProperty));
        }
    }
}
