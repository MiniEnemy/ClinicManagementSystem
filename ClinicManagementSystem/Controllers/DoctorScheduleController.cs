using AutoMapper;
using ClinicManagementSystem.DTOs.DoctorSchedule;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorScheduleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DoctorScheduleController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorScheduleDto dto)
        {
            if (dto.StartTime >= dto.EndTime)
                return BadRequest("StartTime must be earlier than EndTime.");

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(dto.DoctorId);
            if (doctor == null)
                return BadRequest($"Doctor with ID {dto.DoctorId} does not exist.");

            var schedule = new DoctorSchedule
            {
                DoctorId = dto.DoctorId,
                DayOfWeek = dto.Date.DayOfWeek, // FIXED: Use DayOfWeek from the date
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            var hasOverlap = await _unitOfWork.DoctorSchedules.HasOverlapAsync(
                schedule.DoctorId, dto.Date, schedule.StartTime, schedule.EndTime
            );

            if (hasOverlap)
                return BadRequest("Schedule overlaps with existing schedule for this day.");

            await _unitOfWork.DoctorSchedules.AddAsync(schedule);
            await _unitOfWork.CompleteAsync();

            return Ok(schedule);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _unitOfWork.DoctorSchedules.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
            if (doctor == null)
                return BadRequest("Doctor not found.");

            var schedules = await _unitOfWork.DoctorSchedules.GetByDoctorIdAsync(doctorId);
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null) return NotFound();
            return Ok(schedule);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorSchedule updated)
        {
            var existing = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (updated.StartTime >= updated.EndTime)
                return BadRequest("StartTime must be earlier than EndTime.");

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(updated.DoctorId);
            if (doctor == null)
                return BadRequest($"Doctor with ID {updated.DoctorId} does not exist.");

            // Create a mock date for overlap checking
            var mockDate = DateTime.Today.AddDays((int)updated.DayOfWeek - (int)DateTime.Today.DayOfWeek);

            var hasOverlap = await _unitOfWork.DoctorSchedules.HasOverlapAsync(
                updated.DoctorId, mockDate, updated.StartTime, updated.EndTime, id
            );

            if (hasOverlap)
                return BadRequest("Schedule overlaps with existing schedule for this day.");

            existing.DayOfWeek = updated.DayOfWeek;
            existing.StartTime = updated.StartTime;
            existing.EndTime = updated.EndTime;
            existing.DoctorId = updated.DoctorId;

            _unitOfWork.DoctorSchedules.Update(existing);
            await _unitOfWork.CompleteAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null) return NotFound();

            _unitOfWork.DoctorSchedules.Remove(schedule);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Schedule deleted successfully." });
        }

        [HttpGet("availability/{doctorId}")]
        public async Task<IActionResult> GetWeeklyAvailability(int doctorId)
        {
            var schedules = await _unitOfWork.DoctorSchedules.GetByDoctorIdAsync(doctorId);
            return Ok(schedules);
        }
    }
}
