using CareerMind.Application.DTOs.Auth;
using CareerMind.Application.Interfaces;
using CareerMind.Domain.Entities;
using CareerMind.Infrastructure.Data;
using CareerMind.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CareerMind.UnitTests
{
    public class AuthServiceTests
    {
        private CareerMindDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<CareerMindDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            return new CareerMindDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            
            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
            tokenServiceMock.Setup(x => x.GenerateRefreshToken(It.IsAny<User>())).Returns(new RefreshToken { Token = "raw-refresh-token", ExpiresAt = DateTime.UtcNow.AddDays(7) });
            tokenServiceMock.Setup(x => x.HashToken(It.IsAny<string>())).Returns("hashed-refresh-token");

            var passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed-password");

            var authService = new AuthService(context, passwordHasherMock.Object, tokenServiceMock.Object);

            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "Test",
                LastName = "User",
                Role = "Candidate"
            };

            // Act
            var response = await authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("access-token", response.AccessToken);
            Assert.Equal("raw-refresh-token", response.RefreshToken);
            Assert.Equal("test@example.com", response.User.Email);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ThrowsException()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Users.Add(new User { Email = "test@example.com", PasswordHash = "hashed", Role = new Role { Name = "Candidate" } });
            await context.SaveChangesAsync();

            var authService = new AuthService(context, new Mock<IPasswordHasher>().Object, new Mock<ITokenService>().Object);

            var request = new RegisterRequest { Email = "test@example.com", Role = "Candidate" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => authService.RegisterAsync(request));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new User { Email = "test@example.com", PasswordHash = "hashed-password", Role = new Role { Name = "Candidate" } };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
            tokenServiceMock.Setup(x => x.GenerateRefreshToken(It.IsAny<User>())).Returns(new RefreshToken { Token = "raw-refresh-token", ExpiresAt = DateTime.UtcNow.AddDays(7) });
            tokenServiceMock.Setup(x => x.HashToken(It.IsAny<string>())).Returns("hashed-refresh-token");

            var passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock.Setup(x => x.VerifyPassword("Password123", "hashed-password")).Returns(true);

            var authService = new AuthService(context, passwordHasherMock.Object, tokenServiceMock.Object);

            var request = new LoginRequest { Email = "test@example.com", Password = "Password123" };

            // Act
            var response = await authService.LoginAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("access-token", response.AccessToken);
            Assert.Equal("test@example.com", response.User.Email);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsException()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new User { Email = "test@example.com", PasswordHash = "hashed-password", Role = new Role { Name = "Candidate" } };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock.Setup(x => x.VerifyPassword("WrongPassword", "hashed-password")).Returns(false);

            var authService = new AuthService(context, passwordHasherMock.Object, new Mock<ITokenService>().Object);

            var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => authService.LoginAsync(request));
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new User { Email = "test@example.com", Role = new Role { Name = "Candidate" } };
            var refreshToken = new RefreshToken
            {
                Token = "hashed-old-token",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                User = user
            };
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(x => x.HashToken("raw-old-token")).Returns("hashed-old-token");
            tokenServiceMock.Setup(x => x.HashToken("raw-new-token")).Returns("hashed-new-token");
            tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("new-access-token");
            tokenServiceMock.Setup(x => x.GenerateRefreshToken(It.IsAny<User>())).Returns(new RefreshToken { Token = "raw-new-token", ExpiresAt = DateTime.UtcNow.AddDays(7) });

            var authService = new AuthService(context, new Mock<IPasswordHasher>().Object, tokenServiceMock.Object);

            var request = new RefreshTokenRequest { RefreshToken = "raw-old-token" };

            // Act
            var response = await authService.RefreshTokenAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("new-access-token", response.AccessToken);
            Assert.Equal("raw-new-token", response.RefreshToken);
            
            // Check that old token is revoked
            var oldToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "hashed-old-token");
            Assert.True(oldToken.IsRevoked);
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_RevokesToken()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var refreshToken = new RefreshToken
            {
                Token = "hashed-token",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            };
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(x => x.HashToken("raw-token")).Returns("hashed-token");

            var authService = new AuthService(context, new Mock<IPasswordHasher>().Object, tokenServiceMock.Object);

            // Act
            await authService.LogoutAsync("raw-token");

            // Assert
            var tokenInDb = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "hashed-token");
            Assert.True(tokenInDb.IsRevoked);
        }
    }
}
