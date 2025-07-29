using ModelContextProtocol.Server;
using RegistryServer.Services;
using System.ComponentModel;

namespace RegistryServer.Tools
{
    [McpServerToolType]
    public class AuthTools
    {
        private readonly IAuthService _authService;

        public AuthTools(IAuthService authService)
        {
            _authService = authService;
        }

        [McpServerTool, Description("Registers a new user with a specific role")]
        public async Task<string> RegisterAsync(
            [Description("Desired username")] string username,
            [Description("User's password")] string password,
            [Description("User's role")] string role)
        {
            try
            {
                await _authService.RegisterAsync(username, password, role);
                return $"User '{username}' registered successfully.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        [McpServerTool, Description("Logs in a user and returns a JWT token")]
        public async Task<string> LoginAsync(
            [Description("Your username")] string username,
            [Description("Your password")] string password)
        {
            var token = await _authService.LoginAsync(username, password);
            return token ?? "Error: Invalid username or password.";
        }
    }
}