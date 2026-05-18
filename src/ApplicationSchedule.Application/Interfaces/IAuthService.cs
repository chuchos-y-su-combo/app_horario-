using ApplicationSchedule.Application.DTOs.Auth;

namespace ApplicationSchedule.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
