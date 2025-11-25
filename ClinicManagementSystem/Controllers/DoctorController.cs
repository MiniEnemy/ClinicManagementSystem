using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _unitOfWork.Doctors.GetAllAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public async Task<IActionResult> Get(int id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Doctor doctor)
        {
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.CompleteAsync();
            return Ok(doctor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Doctor updatedDoctor)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();

            doctor.FullName = updatedDoctor.FullName;
            doctor.Email = updatedDoctor.Email;
            doctor.Phone = updatedDoctor.Phone;
            doctor.Specialization = updatedDoctor.Specialization;

            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.CompleteAsync();

            return Ok(doctor);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();

            _unitOfWork.Doctors.Remove(doctor);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Doctor deleted successfully." });
        }
    }
}
