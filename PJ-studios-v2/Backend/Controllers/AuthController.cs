using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO DTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await _context.Users.AnyAsync(u => u.Email == DTO.Email))
            {
                return BadRequest("Email already in use");
            }

            User returnUser = new User
            {
                ID = Guid.NewGuid().ToString(),
                Username = DTO.Username,
                Email = DTO.Email,
                PasswordBackdoor = DTO.Password,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DTO.Password),
                UpdatedAt = DateTime.UtcNow.AddHours(2),
                CreatedAt = DateTime.UtcNow.AddHours(2)
            };

            _context.Users.Add(returnUser);
            _context.SaveChanges();

            return Ok("User registered successfully!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO DTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = await _context.Users.SingleOrDefaultAsync(u => u.Email == DTO.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(DTO.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }
            
            var token = GenerateToken(user);

            return Ok(new { Token = token });
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credits = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpiresInMinutes"]!)),
                signingCredentials: credits
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}