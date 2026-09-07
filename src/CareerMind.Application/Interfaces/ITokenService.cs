using CareerMind.Domain.Entities;
using System.Threading.Tasks;

namespace CareerMind.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken(User user);
        string HashToken(string token);
    }
}
