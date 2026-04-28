using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Data;

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

        [HttpGet("ListOfAllUsers")] // For demo purposes only, not recommended for production
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database error", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("User not found");
            }
            User? user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var DTO = new UserDTO
            {
                Username = user.Username,
                Email = user.Email
            };

            return Ok(DTO);
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
                return BadRequest("Invalid email or password");
            }

            var token = GenerateToken(user);

            var refreshToken = GenerateRefreshToken();
            refreshToken.UserId = user.ID;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new { Token = token, RefreshToken = refreshToken.Token });
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


        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

            if (existingToken == null ||
                existingToken.IsRevoked ||
                existingToken.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired refresh token");
            }

            // revoke old token
            existingToken.IsRevoked = true;

            var newRefreshToken = GenerateRefreshToken();
            newRefreshToken.UserId = existingToken.UserId;

            _context.RefreshTokens.Add(newRefreshToken);

            // create new JWT
            var newJwt = GenerateToken(existingToken.User);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Token = newJwt,
                RefreshToken = newRefreshToken.Token
            });
        }


        private RefreshToken GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                Token = Convert.ToBase64String(randomBytes),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
        }


        [Authorize]
        [HttpPatch("updateUser")]
        public async Task<IActionResult> PatchUser(UpdateUserInfoDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("No data provided.");
            }

            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (userId == null)
            {
                return Unauthorized("Missing userId claim.");
            }


            bool updated = false;

            if (!string.IsNullOrWhiteSpace(dto.Username) || dto.Username != "string")
            {
                user.Username = dto.Username;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) || dto.Email != "user@example.com")
            {
                user.Email = dto.Email;
                updated = true;
            }

            if (!updated)
            {
                return BadRequest("No valid fields provided to update.");
            }

            await _context.SaveChangesAsync();

            return Ok("User updated successfully!");
        }


        [Authorize]
        [HttpPut("updatePassword")]
        public async Task<IActionResult> UpdatePassword(UpdateUserPasswordDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("No data provided.");
            }

            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("User not found");
            }
            User? user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            user.PasswordBackdoor = dto.Password;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _context.SaveChangesAsync();

            return Ok("Password updated successfully!");
        }



        [Authorize]
        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser()
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("User not found");
            }
            User? user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User deleted successfully!");
        }
    }
}