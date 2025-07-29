using RegistryServer.Models;
using System.Security.Claims;

namespace RegistryServer.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string username, string password, string role);
        Task<string?> LoginAsync(string username, string password);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
