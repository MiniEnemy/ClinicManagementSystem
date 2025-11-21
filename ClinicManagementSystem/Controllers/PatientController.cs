using AutoMapper;
using ClinicManagementSystem.DTOs.Patient;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PatientController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // Create patient - Admin or Receptionist
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto dto)
        {
            var patient = _mapper.Map<Patient>(dto);
            await _uow.Patients.AddAsync(patient);
            await _uow.CompleteAsync();
            return CreatedAtAction(nameof(Get), new { id = patient.Id }, _mapper.Map<PatientDto>(patient));
        }

        // List patients - any authenticated user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _uow.Patients.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<PatientDto>>(patients));
        }

        // View patient by id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var patient = await _uow.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(_mapper.Map<PatientDto>(patient));
        }

        // Update patient
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Update(int id, [FromBody] CreatePatientDto dto)
        {
            var patient = await _uow.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();

            _mapper.Map(dto, patient);
            _uow.Patients.Update(patient);
            await _uow.CompleteAsync();
            return Ok(_mapper.Map<PatientDto>(patient));
        }

        // Soft delete patient (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _uow.Patients.SoftDeleteAsync(id);
            await _uow.CompleteAsync();
            return NoContent();
        }
    }
}
