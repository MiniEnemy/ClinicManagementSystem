using AutoMapper;
using ClinicManagementSystem.Data;
using ClinicManagementSystem.DTOs.Appointment;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Helpers;
using ClinicManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

            // Check if appointment is in the future
            if (dto.DateTime <= DateTime.UtcNow)
                return BadRequest("Appointment must be in the future");

            // Check doctor's schedule (using DayOfWeek)
            var appointmentDay = dto.DateTime.DayOfWeek;
            var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(dto.DoctorId);
            var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == appointmentDay);

            if (daySchedule == null)
                return BadRequest("Doctor is not available on this day");

            var appointmentTime = dto.DateTime.TimeOfDay;
            if (appointmentTime < daySchedule.StartTime || appointmentTime >= daySchedule.EndTime)
                return BadRequest("Appointment time is outside the doctor's schedule");

            if (await _uow.Appointments.HasConflictAsync(dto.DoctorId, dto.DateTime))
                return Conflict("Doctor has a conflicting appointment at that date/time");

            var appointment = _mapper.Map<Appointment>(dto);
            await _uow.Appointments.AddAsync(appointment);
            await _uow.CompleteAsync();

            // Reload the appointment with included entities
            var createdAppointment = await _uow.Appointments.GetByIdAsync(appointment.Id);
            var resultDto = _mapper.Map<AppointmentDto>(createdAppointment);

            // Set display properties
            if (createdAppointment.Patient != null)
            {
                resultDto.PatientName = $"{createdAppointment.Patient.FirstName} {createdAppointment.Patient.LastName}";
                resultDto.PatientEmail = createdAppointment.Patient.Email;
            }

            if (createdAppointment.Doctor != null)
            {
                resultDto.DoctorName = createdAppointment.Doctor.FullName;
                resultDto.DoctorEmail = createdAppointment.Doctor.Email;
                resultDto.DoctorSpecialization = createdAppointment.Doctor.Specialization;
            }

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, resultDto);
        }

        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] CreateAppointmentDto dto)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt == null) return NotFound();

            var doctor = await _uow.Doctors.GetByIdAsync(dto.DoctorId);
            if (doctor == null) return BadRequest("Doctor not found");

            if (dto.DateTime <= DateTime.UtcNow)
                return BadRequest("Appointment must be in the future");

            // Check doctor's schedule
            var appointmentDay = dto.DateTime.DayOfWeek;
            var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(dto.DoctorId);
            var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == appointmentDay);

            if (daySchedule == null)
                return BadRequest("Doctor is not available on this day");

            var appointmentTime = dto.DateTime.TimeOfDay;
            if (appointmentTime < daySchedule.StartTime || appointmentTime >= daySchedule.EndTime)
                return BadRequest("New appointment time is outside the doctor's schedule");

            if (await _uow.Appointments.HasConflictAsync(dto.DoctorId, dto.DateTime))
                return Conflict("Doctor has a conflicting appointment at that date/time");

            appt.DateTime = dto.DateTime.ToUniversalTime();
            appt.Description = dto.Description;
            appt.DoctorId = dto.DoctorId;

            _uow.Appointments.Update(appt);
            await _uow.CompleteAsync();

            // Reload with included entities
            var updatedAppointment = await _uow.Appointments.GetByIdAsync(id);
            var resultDto = _mapper.Map<AppointmentDto>(updatedAppointment);

            if (updatedAppointment.Patient != null)
            {
                resultDto.PatientName = $"{updatedAppointment.Patient.FirstName} {updatedAppointment.Patient.LastName}";
                resultDto.PatientEmail = updatedAppointment.Patient.Email;
            }

            if (updatedAppointment.Doctor != null)
            {
                resultDto.DoctorName = updatedAppointment.Doctor.FullName;
                resultDto.DoctorEmail = updatedAppointment.Doctor.Email;
                resultDto.DoctorSpecialization = updatedAppointment.Doctor.Specialization;
            }

            return Ok(resultDto);
        }

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

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Complete(int id)
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

            var resultDto = _mapper.Map<AppointmentDto>(appt);

            if (appt.Patient != null)
            {
                resultDto.PatientName = $"{appt.Patient.FirstName} {appt.Patient.LastName}";
                resultDto.PatientEmail = appt.Patient.Email;
            }

            if (appt.Doctor != null)
            {
                resultDto.DoctorName = appt.Doctor.FullName;
                resultDto.DoctorEmail = appt.Doctor.Email;
                resultDto.DoctorSpecialization = appt.Doctor.Specialization;
            }

            return Ok(resultDto);
        }

        // UPDATED: Now with pagination, sorting, and filtering
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        public async Task<IActionResult> GetAll([FromQuery] AppointmentQueryParams queryParams)
        {
            // If user is Doctor (not Admin), restrict to their appointments only
            if (User.IsInRole("Doctor") && !User.IsInRole("Admin"))
            {
                var doctorClaim = User.FindFirst("DoctorId");
                if (doctorClaim != null && int.TryParse(doctorClaim.Value, out int doctorId))
                {
                    queryParams.DoctorId = doctorId;
                }
            }

            var pagedAppointments = await _uow.Appointments.GetPagedAsync(queryParams);

            // Map to DTOs
            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(pagedAppointments.Data);

            // Set display properties
            foreach (var appointment in pagedAppointments.Data)
            {
                var dto = appointmentDtos.First(d => d.Id == appointment.Id);

                if (appointment.Patient != null)
                {
                    dto.PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
                    dto.PatientEmail = appointment.Patient.Email;
                }

                if (appointment.Doctor != null)
                {
                    dto.DoctorName = appointment.Doctor.FullName;
                    dto.DoctorEmail = appointment.Doctor.Email;
                    dto.DoctorSpecialization = appointment.Doctor.Specialization;
                }
            }

            var pagedResponse = new PagedResponse<AppointmentDto>(
                appointmentDtos,
                pagedAppointments.PageNumber,
                pagedAppointments.PageSize,
                pagedAppointments.TotalRecords
            );

            return Ok(pagedResponse);
        }

        // UPDATED: Now with pagination, sorting, and filtering
        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMine([FromQuery] AppointmentQueryParams queryParams)
        {
            var doctorClaim = User.FindFirst("DoctorId");
            if (doctorClaim == null || !int.TryParse(doctorClaim.Value, out int doctorId))
                return BadRequest("Doctor account not linked to a doctor profile");

            // Always filter by current doctor
            queryParams.DoctorId = doctorId;

            var pagedAppointments = await _uow.Appointments.GetPagedAsync(queryParams);

            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(pagedAppointments.Data);

            foreach (var appointment in pagedAppointments.Data)
            {
                var dto = appointmentDtos.First(d => d.Id == appointment.Id);

                if (appointment.Patient != null)
                {
                    dto.PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
                    dto.PatientEmail = appointment.Patient.Email;
                }

                if (appointment.Doctor != null)
                {
                    dto.DoctorName = appointment.Doctor.FullName;
                    dto.DoctorEmail = appointment.Doctor.Email;
                    dto.DoctorSpecialization = appointment.Doctor.Specialization;
                }
            }

            var pagedResponse = new PagedResponse<AppointmentDto>(
                appointmentDtos,
                pagedAppointments.PageNumber,
                pagedAppointments.PageSize,
                pagedAppointments.TotalRecords
            );

            return Ok(pagedResponse);
        }

        // UPDATED: Now with pagination, sorting, and filtering
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] AppointmentQueryParams queryParams)
        {
            // If user is Doctor (not Admin), they can only see their own appointments
            if (User.IsInRole("Doctor") && !User.IsInRole("Admin"))
            {
                var doctorClaim = User.FindFirst("DoctorId");
                if (doctorClaim == null || !int.TryParse(doctorClaim.Value, out int currentDoctorId))
                    return BadRequest("Doctor account not linked to a doctor profile");

                if (doctorId != currentDoctorId)
                    return Forbid("You can only view your own appointments");
            }

            queryParams.DoctorId = doctorId;

            var pagedAppointments = await _uow.Appointments.GetPagedAsync(queryParams);

            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(pagedAppointments.Data);

            foreach (var appointment in pagedAppointments.Data)
            {
                var dto = appointmentDtos.First(d => d.Id == appointment.Id);

                if (appointment.Patient != null)
                {
                    dto.PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
                    dto.PatientEmail = appointment.Patient.Email;
                }

                if (appointment.Doctor != null)
                {
                    dto.DoctorName = appointment.Doctor.FullName;
                    dto.DoctorEmail = appointment.Doctor.Email;
                    dto.DoctorSpecialization = appointment.Doctor.Specialization;
                }
            }

            var pagedResponse = new PagedResponse<AppointmentDto>(
                appointmentDtos,
                pagedAppointments.PageNumber,
                pagedAppointments.PageSize,
                pagedAppointments.TotalRecords
            );

            return Ok(pagedResponse);
        }
    }
}