using AutoPartsB2B.Core.Entities;
using AutoPartsB2B.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoPartsB2B.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // Bayi girişi yapar ve yetkilendirme anahtarı (token) üretir
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginUser.Username && u.Password == loginUser.Password);

            if (user == null)
                return Unauthorized("Geçersiz bayi kodu veya şifre.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("KayseriOtoB2BSuperGizliAnahtar123456!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            // Sadece token değil, giriş yapan kullanıcının kimlik bilgisini de gönderir
            return Ok(new
            {
                Token = tokenHandler.WriteToken(token),
                UserId = user.Id,
                Username = user.Username
            });
        }

        // Formdan gelen verilerle sisteme yeni bayi kaydeder
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User newUser)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == newUser.Username);

            if (userExists)
                return BadRequest("Bu bayi kodu zaten kullanılıyor.");

            // Boş gelen alanlara sistem işleyişi için varsayılan değerler atar
            newUser.Role = "Bayi";
            if (string.IsNullOrEmpty(newUser.FullName))
            {
                newUser.FullName = newUser.Username;
            }

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Bayi kaydı başarıyla oluşturuldu." });
        }
    }
}