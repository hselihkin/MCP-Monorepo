using System.Security.Claims;

namespace BasicCalculatorServer.services
{
    public interface IAuthService
    {
        ClaimsPrincipal? ValidateToken(string token);
    }
}
