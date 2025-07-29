using BasicCalculatorServer.services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Claims;

namespace BasicCalculatorServer.Tools
{
    [McpServerToolType]
    public class Basic_Calculator
    {
        //private readonly IAuthService _authService;

        //public Basic_Calculator(IAuthService authService)
        //{
        //    _authService = authService;
        //}

        [McpServerTool, Description("Adds two numbers")]
        public double AddNum(
            [Description("2nd Number goes here")] double a,
            [Description("1st Number goes here")] double b
            //[Description("The JWT token obtained from logging in.")] string token
        )
        {
            //var principal = _authService.ValidateToken(token);
            //if (principal == null)
            //{
            //    throw new Exception("Error: Invalid or expired token.");
            //}

            //var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            //if (roleClaim == null)
            //{
            //    throw new Exception("Error: Token is missing role information.");
            //}

            //var userRole = roleClaim.Value;
            //if (userRole != "admin" || userRole != "geo")
            //{
            //    throw new Exception("Error: you donot have the required role to perform this operation");
            //}

            return a+b;
        }

        [McpServerTool, Description("Subtracts two numbers")]
        public double SubNum(
            [Description("1st Number")] double a,
            [Description("2nd Number")] double b
            //[Description("The JWT token obtained from logging in.")] string token
        )
        {
            //var principal = _authService.ValidateToken(token);
            //if (principal == null)
            //{
            //    throw new Exception("Error: Invalid or expired token.");
            //}

            //var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            //if (roleClaim == null)
            //{
            //    throw new Exception("Error: Token is missing role information.");
            //}

            //var userRole = roleClaim.Value;
            //if (userRole != "admin" || userRole != "geo")
            //{
            //    throw new Exception("Error: you donot have the required role to perform this operation");
            //}

            return a - b;
        }

        [McpServerTool, Description("Multiplies two numbers")]
        public double MulNum(
            [Description("2nd Number goes here")] double a,
            [Description("1st Number goes here")] double b
            //[Description("The JWT token obtained from logging in.")] string token
        )
        {
            //var principal = _authService.ValidateToken(token);
            //if (principal == null)
            //{
            //    throw new Exception("Error: Invalid or expired token.");
            //}

            //var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            //if (roleClaim == null)
            //{
            //    throw new Exception("Error: Token is missing role information.");
            //}

            //var userRole = roleClaim.Value;
            //if (userRole != "admin" || userRole != "geo")
            //{
            //    throw new Exception("Error: you donot have the required role to perform this operation");
            //}

            return a * b;
        }

        [McpServerTool, Description("Divides two numbers")]
        public double DivNum(
            [Description("1st Number")] double a,
            [Description("2nd Number")] double b
            //[Description("The JWT token obtained from logging in.")] string token
        )
        {
            //var principal = _authService.ValidateToken(token);
            //if (principal == null)
            //{
            //    throw new Exception("Error: Invalid or expired token.");
            //}

            //var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            //if (roleClaim == null)
            //{
            //    throw new Exception("Error: Token is missing role information.");
            //}

            //var userRole = roleClaim.Value;
            //if (userRole != "admin" || userRole != "geo")
            //{
            //    throw new Exception("Error: you donot have the required role to perform this operation");
            //}

            if (b == 0)
            {
                return double.NaN;
            }

            return a / b;
        }
    }
}
