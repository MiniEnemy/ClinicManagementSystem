using AutoMapper;
using ClinicManagementSystem.DTOs.Appointment;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AppointmentController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // Receptionist/Admin: Create Appointment
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _uow.Patients.GetByIdAsync(dto.PatientId);
            if (patient == null) return BadRequest("Patient not found");

            var doctor = await _uow.Doctors.GetByIdAsync(dto.DoctorId);
            if (doctor == null) return BadRequest("Doctor not found");

            // Check doctor schedule
            var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(dto.DoctorId);
            var daySchedules = schedules.Where(s => s.DayOfWeek == dto.AppointmentDate.DayOfWeek);

            bool withinSchedule = daySchedules.Any(s =>
                dto.AppointmentDate.TimeOfDay >= s.StartTime &&
                dto.AppointmentDate.TimeOfDay < s.EndTime);

            if (!withinSchedule)
                return BadRequest("Appointment time is not within the doctor's schedule");

            // Check for conflicts
            bool conflict = await _uow.Appointments.HasConflictAsync(dto.DoctorId, dto.AppointmentDate);
            if (conflict)
                return Conflict("Doctor already has an appointment at this time");

            var appointment = _mapper.Map<Appointment>(dto);
            await _uow.Appointments.AddAsync(appointment);
            await _uow.CompleteAsync();

            var result = _mapper.Map<AppointmentDto>(appointment);

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, result);
        }

        // Doctor: View own appointments (Requires mapping user->doctor in future)
        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMine()
        {
            var doctorIdClaim = User.FindFirst("DoctorId");
            if (doctorIdClaim == null)
                return BadRequest("Doctor account is not linked to any doctor profile.");

            int doctorId = int.Parse(doctorIdClaim.Value);

            var appointments = await _uow.Appointments.GetByDoctorIdAsync(doctorId);

            return Ok(_mapper.Map<IEnumerable<AppointmentDto>>(appointments));
        }

        // Doctor/Admin: Get appointments by doctor
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            var appointments = await _uow.Appointments.GetByDoctorIdAsync(doctorId);
            return Ok(_mapper.Map<IEnumerable<AppointmentDto>>(appointments));
        }

        // Admin: Get all appointments
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _uow.Appointments.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<AppointmentDto>>(appointments));
        }

        // Receptionist/Admin: Reschedule Appointment
        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _uow.Appointments.GetByIdAsync(id);
            if (appointment == null) return NotFound();

            var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(dto.DoctorId);
            var daySchedules = schedules.Where(s => s.DayOfWeek == dto.AppointmentDate.DayOfWeek);

            bool withinSchedule = daySchedules.Any(s =>
                dto.AppointmentDate.TimeOfDay >= s.StartTime &&
                dto.AppointmentDate.TimeOfDay < s.EndTime);

            if (!withinSchedule)
                return BadRequest("New appointment time is outside doctor's schedule");

            bool conflict = await _uow.Appointments.HasConflictAsync(dto.DoctorId, dto.AppointmentDate);
            if (conflict)
                return Conflict("Doctor already has an appointment at this time");

            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.Description = dto.Description;

            _uow.Appointments.Update(appointment);
            await _uow.CompleteAsync();

            return Ok(_mapper.Map<AppointmentDto>(appointment));
        }

        // Receptionist/Admin: Cancel Appointment
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Cancelled;

            _uow.Appointments.Update(appointment);
            await _uow.CompleteAsync();

            return NoContent();
        }

        // Doctor: Mark Completed
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = AppointmentStatus.Completed;

            _uow.Appointments.Update(appointment);
            await _uow.CompleteAsync();

            return NoContent();
        }

        // Get appointment by id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(id);
            if (appointment == null) return NotFound();

            return Ok(_mapper.Map<AppointmentDto>(appointment));
        }
    }
}
