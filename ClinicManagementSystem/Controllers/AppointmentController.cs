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

        // Receptionist: create appointment with checks
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            // validate patient and doctor exist
            var patient = await _uow.Patients.GetByIdAsync(dto.PatientId);
            if (patient == null) return BadRequest("Patient not found");

            var doctor = await _uow.Doctors.GetByIdAsync(dto.DoctorId);
            if (doctor == null) return BadRequest("Doctor not found");

            // check schedule: doctor must have a schedule covering that day/time
            var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(dto.DoctorId);
            var daySchedules = schedules.Where(s => s.DayOfWeek == dto.AppointmentDate.DayOfWeek);
            bool withinSchedule = daySchedules.Any(s =>
            {
                var apptTime = dto.AppointmentDate.TimeOfDay;
                return apptTime >= s.StartTime && apptTime < s.EndTime;
            });

            if (!withinSchedule) return BadRequest("Appointment time outside doctor's schedule");

            // check conflict
            if (await _uow.Appointments.HasConflictAsync(dto.DoctorId, dto.AppointmentDate))
                return Conflict("Doctor has a conflicting appointment at that time");

            var appointment = _mapper.Map<Appointment>(dto);
            await _uow.Appointments.AddAsync(appointment);
            await _uow.CompleteAsync();

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, _mapper.Map<AppointmentDto>(appointment));
        }

        // Doctor: get own appointments
        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMine()
        {
            // extract DoctorId from claim - in a real system you'd store doctor-user mapping
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // For simplicity: require a query param doctorId or map user to doctor; here we read doctorId from query
            return BadRequest("Doctor endpoint requires doctorId mapping. Use /api/appointments/doctor/{doctorId}");
        }

        // Get appointments for a given doctor (Doctor can view own, Admin can view any)
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            // if role is Doctor, ensure doc id matches (this requires mapping user->doctor in real app)
            var appointments = await _uow.Appointments.GetByDoctorIdAsync(doctorId);
            return Ok(_mapper.Map<IEnumerable<AppointmentDto>>(appointments));
        }

        // Admin: get all appointments
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _uow.Appointments.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<AppointmentDto>>(appointments));
        }

        // Reschedule (Receptionist): update appointment date after checks
        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] CreateAppointmentDto dto)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt == null) return NotFound();

            // schedule & conflict checks like create
            var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(dto.DoctorId);
            var daySchedules = schedules.Where(s => s.DayOfWeek == dto.AppointmentDate.DayOfWeek);
            bool withinSchedule = daySchedules.Any(s =>
            {
                var apptTime = dto.AppointmentDate.TimeOfDay;
                return apptTime >= s.StartTime && apptTime < s.EndTime;
            });

            if (!withinSchedule) return BadRequest("Appointment time outside doctor's schedule");
            if (await _uow.Appointments.HasConflictAsync(dto.DoctorId, dto.AppointmentDate))
                return Conflict("Doctor has a conflicting appointment at that time");

            appt.AppointmentDate = dto.AppointmentDate;
            appt.Description = dto.Description;
            _uow.Appointments.Update(appt);
            await _uow.CompleteAsync();

            return Ok(_mapper.Map<AppointmentDto>(appt));
        }

        // Cancel appointment (Receptionist or Admin)
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt == null) return NotFound();
            appt.Status = AppointmentStatus.Cancelled;
            _uow.Appointments.Update(appt);
            await _uow.CompleteAsync();
            return NoContent();
        }

        // Doctor: mark completed
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt == null) return NotFound();
            appt.Status = AppointmentStatus.Completed;
            _uow.Appointments.Update(appt);
            await _uow.CompleteAsync();
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt == null) return NotFound();
            return Ok(_mapper.Map<AppointmentDto>(appt));
        }
    }
}
