namespace LearningManagementSystem.Mapping
{
    // Mapping/MappingProfile.cs
    using AutoMapper;
    using LearningManagementSystem.Models;

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
