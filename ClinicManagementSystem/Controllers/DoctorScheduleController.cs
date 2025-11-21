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
    public class DoctorScheduleController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DoctorScheduleController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // CREATE schedule
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateScheduleDto dto)
        {
            var schedule = new DoctorSchedule
            {
                DoctorId = dto.DoctorId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            await _uow.DoctorSchedules.AddAsync(schedule);
            await _uow.CompleteAsync();

            return CreatedAtAction(nameof(Get), new { id = schedule.Id }, _mapper.Map<ScheduleDto>(schedule));
        }

        // GET schedule by ID (fixes your “Get does not exist” error)
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var schedule = await _uow.DoctorSchedules.GetByIdAsync(id);

            if (schedule == null)
                return NotFound();

            return Ok(_mapper.Map<ScheduleDto>(schedule));
        }

        // GET all schedules for a doctor
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(doctorId);
            return Ok(_mapper.Map<IEnumerable<ScheduleDto>>(schedules));
        }
    }
}
