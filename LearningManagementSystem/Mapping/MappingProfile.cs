using AutoMapper;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.DTOs;
namespace LearningManagementSystem.Mapping
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CreateMap<Source, Destination>();
            CreateMap<Certificate, CertificateDto>().ForMember(cd=> cd.CourseName, opt=>opt.MapFrom(c=>c.Enrollment.Course.Title))
                .ForMember(cd => cd.StudentName, opt => opt.MapFrom(c => c.Enrollment.Student.FullName));
            CreateMap<CertificateDto, Certificate>();
            //CreateMap<ProductDto, Product>();

            // For more complex mappings
            // CreateMap<Source, Destination>()
            //     .ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.SourceProperty));
        }
    }
}
