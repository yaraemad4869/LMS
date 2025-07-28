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
            CreateMap<Certificate, CertificateDto>()
                .ForMember(cd=> cd.CourseName, opt=>opt.MapFrom(c=>c.Enrollment.Course.Title))
                .ForMember(cd => cd.StudentName, opt => opt.MapFrom(c => c.Enrollment.Student.FullName));
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.NumOfStudents,
                       opt => opt.MapFrom(src => src.Students.Count()))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(c=>c.Reviews.Select(r=>r.Rating).Average()));
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.CourseName,
                       opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(r => r.Student.FullName));
            CreateMap<CertificateDto, Certificate>();
            CreateMap<CourseDto, Course>();
            CreateMap<Instructor, UserDto>();
            CreateMap<UserDto, Instructor>();
            CreateMap<ReviewDto, Review>();
            CreateMap<LectureDto, Lecture>();
            CreateMap<Lecture, LectureDto>();
        }
    }
}
