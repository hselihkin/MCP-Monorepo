using System.Security.Claims;

namespace ADVCalculatorServer.services
{
    public interface IAuthService
    {
        ClaimsPrincipal? ValidateToken(string token);
    }
}
