using AutoMapper;
using ClinicManagementSystem.DTOs.Patient;
using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreatePatientDto, Patient>();
            CreateMap<Patient, PatientDto>();
            CreateMap<Entities.Appointment, DTOs.Appointment.AppointmentDto>();
            CreateMap<DTOs.Appointment.CreateAppointmentDto, Entities.Appointment>();
        }
    }
}
