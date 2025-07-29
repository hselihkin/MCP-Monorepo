using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Claims;
using WeatherServer.services;

namespace WeatherServer.Tools
{
    [McpServerToolType]
    public class WeatherForecast
    {
        //private readonly IAuthService _authService;

        //public WeatherForecast(IAuthService authService)
        //{
        //    _authService = authService;
        //}

        [McpServerTool, Description("Gets the weather forecast for a given location.")]
        public async Task<string> GetWeatherForecast(
            [Description("Latitude of the location")] double latitude,
            [Description("Longitude of the location")] double longitude
            //[Description("The JWT token obtained from logging in.")] string token
        )
        {
            //var principal = _authService.ValidateToken(token);
            //if (principal == null)
            //{
            //    return "Error: Invalid or expired token.";
            //}

            //var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            //if (roleClaim == null)
            //{
            //    return "Error: Token is missing role information.";
            //}

            //var userRole = roleClaim.Value;
            //if (userRole != "admin" || userRole != "geo")
            //{
            //    return "Error: you donot have the required role to perform this operation";
            //}

            try
            {
                // Use HttpClient to call the external Open-Meteo API
                var client = new HttpClient();
                string requestUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&hourly=temperature_2m";

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                // You can parse the JSON and return a cleaner response
                // For simplicity, we'll return the raw JSON here.
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                return $"Error: Could not retrieve weather data. {e.Message}";
            }
        }
    }
}
