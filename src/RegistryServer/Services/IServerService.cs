using RegistryServer.Models;

namespace RegistryServer.Services
{
    public interface IServerService
    {
        Task<IEnumerable<Server>> GetAllServersAsync(string userRole);
        Task<Server?> GetServerAsync(Uri Endpoint);
        Task<IEnumerable<Server>> GetServersByToolAsync(string toolName);
        Task<Server> CreateServerAsync(Server server);
        Task<Server?> UpdateServerAsync(Uri Endpoint, Server updatedServer);
        Task<bool> DeleteServerAsync(Uri Endpoint);
        Task<bool> ServerExistsAsync(Uri Endpoint);
        Task<Server?> AddToolToServerAsync(Uri Endpoint, string toolName, Dictionary<string, string> toolArguments);
        Task<Server?> RemoveToolFromServerAsync(Uri Endpoint, string toolName);
        Task<Server?> UpdateToolArgumentsAsync(Uri Endpoint, string toolName, Dictionary<string, string> toolArguments);
    }
}
