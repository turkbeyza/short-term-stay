using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using short_term_stay_project.Data;
using short_term_stay_project.DTOs;
using short_term_stay_project.Models;

namespace short_term_stay_project.Services;

public class AuthService : IAuthService
{
    private readonly ShortTermStayDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ShortTermStayDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        // Simplified password check for this project (should use hashing in real app)
        if (user == null || user.PasswordHash != request.Password)
            return null;

        var token = GenerateJwtToken(user);
        return new LoginResponse(token, user.Username, user.Role.ToString());
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return false;

        var user = new User
        {
            Username = request.Username,
            PasswordHash = request.Password, // Simplified
            Role = Enum.Parse<UserRole>(request.Role)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
