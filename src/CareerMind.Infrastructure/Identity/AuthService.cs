using CareerMind.Application.DTOs.Auth;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CareerMind.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly CareerMindDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(
            CareerMindDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (request.Role == "Admin")
                throw new Exception("Cannot register as Admin.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                throw new Exception("Email already exists.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if (role == null)
            {
                // Create role if doesn't exist for simplicity, but strictly should be seeded.
                role = new Role { Name = request.Role };
                _context.Roles.Add(role);
            }

            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = passwordHash,
                Role = role
            };

            if (request.Role == "Candidate")
            {
                user.CandidateProfile = new CandidateProfile();
            }
            else if (request.Role == "Employer")
            {
                user.Company = new Company { Name = request.FirstName + " Company" }; // Placeholder logic
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            var rawRefreshToken = refreshToken.Token;
            refreshToken.Token = _tokenService.HashToken(rawRefreshToken);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiresAt = refreshToken.ExpiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.Name
                }
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            var rawRefreshToken = refreshToken.Token;
            refreshToken.Token = _tokenService.HashToken(rawRefreshToken);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiresAt = refreshToken.ExpiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.Name
                }
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var hashedToken = _tokenService.HashToken(request.RefreshToken);

            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt => rt.Token == hashedToken);

            if (refreshToken == null || !refreshToken.IsActive)
                throw new Exception("Invalid or revoked refresh token.");

            // Revoke old token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            var newAccessToken = _tokenService.GenerateAccessToken(refreshToken.User);
            var newRefreshToken = _tokenService.GenerateRefreshToken(refreshToken.User);

            var rawNewRefreshToken = newRefreshToken.Token;
            newRefreshToken.Token = _tokenService.HashToken(rawNewRefreshToken);

            refreshToken.ReplacedByToken = newRefreshToken.Token;

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = rawNewRefreshToken,
                ExpiresAt = newRefreshToken.ExpiresAt,
                User = new UserDto
                {
                    Id = refreshToken.User.Id,
                    Email = refreshToken.User.Email,
                    FirstName = refreshToken.User.FirstName,
                    LastName = refreshToken.User.LastName,
                    Role = refreshToken.User.Role.Name
                }
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var hashedToken = _tokenService.HashToken(refreshToken);

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == hashedToken);

            if (token != null && token.IsActive)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserDto> GetCurrentUserAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.Name
            };
        }
    }
}
