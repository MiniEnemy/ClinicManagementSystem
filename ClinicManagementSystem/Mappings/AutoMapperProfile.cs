using AutoMapper;
using ClinicManagementSystem.DTOs.Doctor;
using ClinicManagementSystem.DTOs.Patient;
using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // PATIENT MAPPINGS
            CreateMap<CreatePatientDto, Patient>()
                .ForMember(dest => dest.DateOfBirth,
                    opt => opt.MapFrom(src =>
                        DateTime.SpecifyKind(src.DateOfBirth, DateTimeKind.Utc)));

            CreateMap<Patient, PatientDto>();


            // DOCTOR MAPPINGS (Doctor has NO CreatedAt → so removed)
            CreateMap<CreateDoctorDto, Doctor>();
            CreateMap<Doctor, DoctorDto>();


            // APPOINTMENT MAPPINGS
            CreateMap<ClinicManagementSystem.Entities.Appointment,
                      ClinicManagementSystem.DTOs.Appointment.AppointmentDto>();

            CreateMap<ClinicManagementSystem.DTOs.Appointment.CreateAppointmentDto,
                      ClinicManagementSystem.Entities.Appointment>();
        }
    }
}
