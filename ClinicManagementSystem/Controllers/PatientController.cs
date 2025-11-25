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
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PatientController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = _mapper.Map<Patient>(dto);
            patient.SetUtcDates();

            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<PatientDto>(patient);
            return Ok(resultDto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _unitOfWork.Patients.GetAllAsync();
            var resultDtos = _mapper.Map<IEnumerable<PatientDto>>(patients);
            return Ok(resultDtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public async Task<IActionResult> Get(int id)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();

            var resultDto = _mapper.Map<PatientDto>(patient);
            return Ok(resultDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientDto updatedPatientDto)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();

            patient.FirstName = updatedPatientDto.FirstName;
            patient.LastName = updatedPatientDto.LastName;
            patient.Email = updatedPatientDto.Email;
            patient.Phone = updatedPatientDto.Phone;
            patient.DateOfBirth = updatedPatientDto.DateOfBirth;

            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<PatientDto>(patient);
            return Ok(resultDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();

            await _unitOfWork.Patients.SoftDeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Patient soft-deleted successfully." });
        }
    }
}
