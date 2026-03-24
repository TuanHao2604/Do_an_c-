namespace SmartTourApp.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginRequest request);
    Task<AuthResultDto> RegisterAsync(RegisterRequest request);
}

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string FullName, string Email);
public record AuthResultDto(bool Success, string? Token, string? Error, UserInfoDto? User);
public record UserInfoDto(Guid Id, string Username, string FullName, string Email, string Role);
