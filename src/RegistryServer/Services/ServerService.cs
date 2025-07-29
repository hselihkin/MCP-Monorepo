using Microsoft.EntityFrameworkCore;
using RegistryServer.Data;
using RegistryServer.Models;

namespace RegistryServer.Services
{
    public class ServerService : IServerService
    {
        private readonly ApplicationDbContext _context;

        public ServerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Server>> GetAllServersAsync(string userRole)
        {
            var allServers = await _context.Servers.ToListAsync();

            switch (userRole.ToLower())
            {
                case "math":
                    return allServers.Where(s =>
                        s.ServerName.Contains("Calculator", StringComparison.OrdinalIgnoreCase));

                case "geo":
                    return allServers.Where(s =>
                        s.ServerName.Contains("Weather", StringComparison.OrdinalIgnoreCase));

                case "admin":
                    return allServers.OrderBy(s => s.Endpoint.ToString());

                // Default behavior: return no servers for unhandled roles
                default:
                    return Enumerable.Empty<Server>();
            }
        }

        public async Task<Server?> GetServerAsync(Uri Endpoint)
        {
            return await _context.Servers
                .FirstOrDefaultAsync(s => s.Endpoint == Endpoint);
        }

        public async Task<IEnumerable<Server>> GetServersByToolAsync(string toolName)
        {
            var servers = await _context.Servers.ToListAsync();
            return servers.Where(s => s.Tools.Contains(toolName)).OrderBy(s => s.Endpoint);
        }

        public async Task<Server> CreateServerAsync(Server server)
        {
            if (await ServerExistsAsync(server.Endpoint))
            {
                throw new InvalidOperationException($"Server with name '{server.ServerName}' already exists.");
            }

            _context.Servers.Add(server);
            await _context.SaveChangesAsync();
            return server;
        }

        public async Task<Server?> UpdateServerAsync(Uri Endpoint, Server updatedServer)
        {
            var existingServer = await GetServerAsync(Endpoint);
            if (existingServer == null)
            {
                return null;
            }

            // Update properties
            existingServer.ServerName = updatedServer.ServerName;
            existingServer.ServerDesc = updatedServer.ServerDesc;
            existingServer.Tools = updatedServer.Tools;
            existingServer.Arguments = updatedServer.Arguments;

            await _context.SaveChangesAsync();
            return existingServer;
        }

        public async Task<bool> DeleteServerAsync(Uri Endpoint)
        {
            var server = await GetServerAsync(Endpoint);
            if (server == null)
            {
                return false;
            }

            _context.Servers.Remove(server);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ServerExistsAsync(Uri Endpoint)
        {
            return await _context.Servers
                .AnyAsync(s => s.Endpoint == Endpoint);
        }

        public async Task<Server?> AddToolToServerAsync(Uri Endpoint, string toolName, Dictionary<string, string> toolArguments)
        {
            var server = await GetServerAsync(Endpoint);
            if (server == null)
            {
                return null;
            }

            if (!server.Tools.Contains(toolName))
            {
                server.Tools.Add(toolName);
            }

            server.Arguments[toolName] = toolArguments;
            await _context.SaveChangesAsync();
            return server;
        }

        public async Task<Server?> RemoveToolFromServerAsync(Uri Endpoint, string toolName)
        {
            var server = await GetServerAsync(Endpoint);
            if (server == null)
            {
                return null;
            }

            server.Tools.Remove(toolName);
            server.Arguments.Remove(toolName);
            await _context.SaveChangesAsync();
            return server;
        }

        public async Task<Server?> UpdateToolArgumentsAsync(Uri Endpoint, string toolName, Dictionary<string, string> toolArguments)
        {
            var server = await GetServerAsync(Endpoint);
            if (server == null)
            {
                return null;
            }

            if (server.Tools.Contains(toolName))
            {
                server.Arguments[toolName] = toolArguments;
                await _context.SaveChangesAsync();
                return server;
            }

            return null;
        }
    }
}
