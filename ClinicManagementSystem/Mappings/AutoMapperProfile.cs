using AutoMapper;
using ClinicManagementSystem.DTOs.Appointment;
using ClinicManagementSystem.DTOs.Doctor;
using ClinicManagementSystem.DTOs.Patient;
using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Appointment mappings
            CreateMap<Appointment, AppointmentDto>()
                .ReverseMap();

            CreateMap<CreateAppointmentDto, Appointment>()
                .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src =>
                    src.DateTime.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(src.DateTime, DateTimeKind.Utc)
                        : src.DateTime.ToUniversalTime()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AppointmentStatus.Scheduled))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Patient mappings
            CreateMap<Patient, PatientDto>().ReverseMap();
            CreateMap<CreatePatientDto, Patient>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.ToUtcDateOfBirth()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Doctor mappings
            CreateMap<Doctor, DoctorDto>().ReverseMap();
            CreateMap<CreateDoctorDto, Doctor>();
        }
    }
}