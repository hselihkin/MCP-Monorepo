using ModelContextProtocol.Server;
using RegistryServer.Models;
using RegistryServer.Services;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;

namespace RegistryServer.Tools
{
    [McpServerToolType]
    public class ServerTools
    {
        private readonly IServerService _serverService;
        private readonly IAuthService _authService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ServerTools(IServerService serverService, IAuthService authService, IHttpClientFactory httpClientFactory)
        {
            _serverService = serverService;
            _authService = authService;
            _httpClientFactory = httpClientFactory;
        }

        [McpServerTool, Description("Adds Server Details in an SQL Database")]
        public async Task<string> AddServerEntryAsync(
            [Description("Server's Endpoint")] Uri Endpoint,
            [Description("The Name of the Server")] string serverName,
            [Description("Tool/Functions offered by the server")] List<string> tools,
            [Description("A JSON string representing the arguments of the Tool/Functions")] string argumentsJson,
            [Description("The JWT token obtained from logging in.")] string token,
            [Description("Overall Purpose of the Server")] string? serverDesc = null
        )
        {
            var arguments = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(argumentsJson);

            var principal = _authService.ValidateToken(token);
            if (principal == null)
            {
                return "Error: Invalid or expired token.";
            }

            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                return "Error: Token is missing role information.";
            }

            var userRole = roleClaim.Value;
            if (userRole != "admin")
            {
                return "Error: Only admins are allowed to do this operation";
            }

            try
            {
                var server = new Server
                {
                    Endpoint = Endpoint,
                    ServerName = serverName,
                    Tools = tools,
                    ServerDesc = serverDesc,
                    Arguments = arguments
                };

                await _serverService.CreateServerAsync(server);
                return $"Server '{serverName}' with tool '{tools}' added successfully.";
            }
            catch (InvalidOperationException ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        [McpServerTool, Description("Gets all server entries in the SQL Database based on user role.")]
        public async Task<object> GetAllServersAsync(
            [Description("The JWT token obtained from logging in.")] string token
        )
        {
            var principal = _authService.ValidateToken(token);
            if (principal == null)
            {
                return "Error: Invalid or expired token.";
            }

            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                return "Error: Token is missing role information.";
            }

            var userRole = roleClaim.Value;
            return await _serverService.GetAllServersAsync(userRole);
        }


        [McpServerTool, Description("Gets all currently running servers or active servers amongst the servers listed in the SQL Database based on user role.")]
        public async Task<object> GetAllRunningServersAsync(
            [Description("The JWT token obtained from logging in.")] string token
        )
        {
            var principal = _authService.ValidateToken(token);
            if (principal == null)
            {
                return "Error: Invalid or expired token.";
            }

            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                return "Error: Token is missing role information.";
            }

            var userRole = roleClaim.Value;
            var potentialServers = await _serverService.GetAllServersAsync(userRole);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

            var healthCheckTasks = potentialServers.Select(async server =>
            {
                try
                {
                    var healthCheckUri = new Uri(server.Endpoint.GetLeftPart(UriPartial.Authority));
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                    var response = await client.GetAsync(healthCheckUri, cts.Token);

                    return server;
                }
                catch (Exception)
                {
                    return null;
                }
            });

            var results = await Task.WhenAll(healthCheckTasks);
            var runningServers = results.Where(s => s != null).ToList();

            return runningServers;
        }

        [McpServerTool, Description("Updates a Record of the SQL Database")]
        public async Task<string> UpdateServerAsync(
            [Description("server's Endpoint to identify the server")] Uri Endpoint,
            [Description("Updated server name")] string serverName,
            [Description("Updated list of tools")] List<string> tools, 
            [Description("Updated arguments of the tools")] Dictionary<string, Dictionary<string, string>> newArguments,
            [Description("The JWT token obtained from logging in.")] string token,
            [Description("Updated Desription of the server")] string? newDesc = null
        )
        {
            var principal = _authService.ValidateToken(token);
            if (principal == null)
            {
                return "Error: Invalid or expired token.";
            }

            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                return "Error: Token is missing role information.";
            }

            var userRole = roleClaim.Value;
            if (userRole != "admin")
            {
                return "Error: Only admins are allowed to do this operation";
            }

            var updatedServer = new Server
            {
                Endpoint = Endpoint,
                ServerName = serverName,
                Tools = tools,
                ServerDesc = newDesc,
                Arguments = newArguments
            };

            var result = await _serverService.UpdateServerAsync(Endpoint, updatedServer);
            return result != null
                ? $"Server '{serverName}' with tool '{tools}' updated successfully."
                : $"Server '{serverName}' with tool '{tools}' not found.";
        }

        [McpServerTool, Description("Deletes a Record of the SQL Database")]
        public async Task<string> DeleteServerAsync(
            [Description("endpoint of the server to be deleted")] Uri Endpoint,
            [Description("The JWT token obtained from logging in.")] string token
        )
        {
            var principal = _authService.ValidateToken(token);
            if (principal == null)
            {
                return "Error: Invalid or expired token.";
            }

            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                return "Error: Token is missing role information.";
            }

            var userRole = roleClaim.Value;
            if (userRole != "admin")
            {
                return "Error: Only admins are allowed to do this operation";
            }

            var result = await _serverService.DeleteServerAsync(Endpoint);
            return result
                ? $"Server at '{Endpoint}' deleted successfully."
                : $"Server at ' {Endpoint}' not found.";
        }

    }
}
