using System.Security.Claims;

namespace WeatherServer.services
{
    public interface IAuthService
    {
        ClaimsPrincipal? ValidateToken(string token);
    }
}
