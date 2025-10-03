using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LegacyBookStore.Data;
using LegacyBookStore.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LegacyBookStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IConfiguration _configuration, AppDbContext _context) : ControllerBase
    {
        //Email у нас пока не используется и проверить его нельзя
        //кто будет делать таску на IEmailSenderService, тот и допилит
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest registerModel)
        {
            if (await _context.Users.AnyAsync(u => u.Name == registerModel.Username))
            {
                return BadRequest("Username already exists");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerModel.Password);

            var user = new Models.User
            {
                Name = registerModel.Username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.Where(u => u.Name == request.Username).FirstOrDefaultAsync();
            //нужен репозиторий
            if (user is not null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized("Invalid username or password");
                }
                var token = GenerateJwtToken(request.Username);
                // Установка куки
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                    SameSite = SameSiteMode.Strict,
                    Secure = false
                };
                Response.Cookies.Append("access_token", token, cookieOptions);

                return Ok(new { message = "Login successful" });
            }
            return Unauthorized("Invalid username or password");
        }

        [Authorize]
        [HttpGet("checkToken")]
        public IActionResult CheckToken()
        {
            return Ok();
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return Ok(new { message = "Logout successful" });
        }
        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var cred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
