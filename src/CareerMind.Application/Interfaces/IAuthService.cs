using CareerMind.Application.DTOs.Auth;
using System;
using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task LogoutAsync(string refreshToken);
        Task<UserDto> GetCurrentUserAsync(Guid userId);
    }
}
