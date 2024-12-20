using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase {
  private readonly UserContext _context;
  private readonly IConfiguration _configuration;

  public UsersController(UserContext context, IConfiguration configuration) {
    _context = context;
    _configuration = configuration;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(User user) {
    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
    await _context.Users.InsertOneAsync(user);
    return Ok();
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(User user) {
    var dbUser = await _context.Users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
    if (dbUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password)) {
      return Unauthorized();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_configuration["JWT:Key"]);
    var tokenDescriptor = new SecurityTokenDescriptor {
      Subject = new ClaimsIdentity(new Claim[] {
        new Claim(ClaimTypes.Name, dbUser.Id.ToString())
      }),
      Expires = DateTime.UtcNow.AddHours(1),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Ok(new {
      token = tokenHandler.WriteToken(token)
    });
  }
}