using ClinicManagementSystem.DTOs.Auth;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClinicManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDoctorRepository _doctorRepository;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            IDoctorRepository doctorRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _doctorRepository = doctorRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest("Email already exists");

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // FIXED: Link doctor account if role is Doctor
            if (dto.Role == "Doctor")
            {
                var doctor = await _doctorRepository.GetByEmailAsync(dto.Email);
                if (doctor != null)
                {
                    user.DoctorId = doctor.Id;
                    await _userManager.UpdateAsync(user);
                }
            }

            if (!string.IsNullOrEmpty(dto.Role))
            {
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));

                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return Ok(new { message = "User created" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var r in roles)
                authClaims.Add(new Claim(ClaimTypes.Role, r));

            // FIXED: Always include DoctorId if available
            if (user.DoctorId.HasValue)
                authClaims.Add(new Claim("DoctorId", user.DoctorId.Value.ToString()));

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: authClaims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                roles = roles,
                doctorId = user.DoctorId
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound("User not found");

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            // FIXED: Link doctor when assigning Doctor role
            if (dto.Role == "Doctor")
            {
                var doctor = await _doctorRepository.GetByEmailAsync(dto.Email);
                if (doctor != null)
                {
                    user.DoctorId = doctor.Id;
                    await _userManager.UpdateAsync(user);
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new { message = $"Role '{dto.Role}' assigned to {dto.Email}" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound("User not found");

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return NotFound("Role does not exist");

            var result = await _userManager.RemoveFromRoleAsync(user, dto.Role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = $"Role '{dto.Role}' removed from {dto.Email}" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-roles/{email}")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-roles")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }

        // NEW: Link doctor to user
        [Authorize(Roles = "Admin")]
        [HttpPost("link-doctor")]
        public async Task<IActionResult> LinkDoctorToUser([FromBody] LinkDoctorDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound("User not found");

            var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId);
            if (doctor == null)
                return NotFound("Doctor not found");

            user.DoctorId = dto.DoctorId;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = $"Doctor {doctor.FullName} linked to user {dto.Email}" });
        }
    }

    public class LinkDoctorDto
    {
        public string Email { get; set; } = null!;
        public int DoctorId { get; set; }
    }
}
