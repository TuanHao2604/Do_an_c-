using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartTourApp.Domain.Entities;
using SmartTourApp.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartTourApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(IAppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new AuthResultDto(false, null, "Tên đăng nhập hoặc mật khẩu không đúng.", null);
        }

        var token = GenerateJwtToken(user);
        var userInfo = new UserInfoDto(user.Id, user.Username, user.FullName, user.Email, user.Role.Name);

        return new AuthResultDto(true, token, null, userInfo);
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return new AuthResultDto(false, null, "Tên đăng nhập đã tồn tại.", null);

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return new AuthResultDto(false, null, "Email đã được sử dụng.", null);

        var userRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User")
            ?? throw new InvalidOperationException("Role 'User' not found");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            RoleId = userRole.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        user.Role = userRole;
        var userInfo = new UserInfoDto(user.Id, user.Username, user.FullName, user.Email, user.Role.Name);

        return new AuthResultDto(true, token, null, userInfo);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "SmartTourApp-Super-Secret-Key-2024-Min32Chars!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "SmartTourApp",
            audience: _configuration["Jwt:Audience"] ?? "SmartTourApp",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
