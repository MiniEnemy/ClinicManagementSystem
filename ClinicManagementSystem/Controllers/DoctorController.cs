using AutoMapper;
using ClinicManagementSystem.DTOs.Doctor;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DoctorController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // Admin only: create doctor
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
        {
            var doctor = _mapper.Map<Doctor>(dto);
            await _uow.Doctors.AddAsync(doctor);
            await _uow.CompleteAsync();
            return CreatedAtAction(nameof(Get), new { id = doctor.Id }, _mapper.Map<DoctorDto>(doctor));
        }

        // Admin: get all doctors
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var docs = await _uow.Doctors.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<DoctorDto>>(docs));
        }

        // Admin: get doctor
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get(int id)
        {
            var doc = await _uow.Doctors.GetByIdAsync(id);
            if (doc == null) return NotFound();
            return Ok(_mapper.Map<DoctorDto>(doc));
        }

        // Admin: update doctor
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateDoctorDto dto)
        {
            var doc = await _uow.Doctors.GetByIdAsync(id);
            if (doc == null) return NotFound();
            _mapper.Map(dto, doc);
            _uow.Doctors.Update(doc);
            await _uow.CompleteAsync();
            return Ok(_mapper.Map<DoctorDto>(doc));
        }

        // Admin: delete doctor
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _uow.Doctors.GetByIdAsync(id);
            if (doc == null) return NotFound();
            _uow.Doctors.Remove(doc);
            await _uow.CompleteAsync();
            return NoContent();
        }
    }
}
