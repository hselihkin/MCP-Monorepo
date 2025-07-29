using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Security.Claims;
using ADVCalculatorServer.services;

namespace ADVCalculatorServer.Tools
{
    [McpServerToolType]
    public class Adv_Calculator
    {
        //private readonly IAuthService _authService;

        //public Adv_Calculator(IAuthService authService)
        //{
        //    _authService = authService;
        //}

        [McpServerTool, Description("Calculates the nth root of a number")]
        public double NthRoot(
            [Description("Target Number")] double num, 
            [Description("Degree of root")] double n
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

            if (n == 0)
            {
                return double.NaN;
            }

            if (num < 0)
            {
                bool isOddInteger = Math.Abs(n - Math.Round(n)) < 1e-9 && (int)Math.Round(n) % 2 != 0;

                if (isOddInteger)
                {
                    return -Math.Pow(-num, 1.0 / n);
                }
                else
                {
                    return double.NaN;
                }
            }

            return Math.Pow(num, 1.0 / n);
        }

        [McpServerTool, Description("Gives Logarithmic of a number")]
        public double LogNum(
            [Description("Target Number")] double num,
            [Description("Base of Logarithm")] double logBase
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

            if (num <= 0 || logBase <= 0 || logBase == 1)
            {
                return double.NaN;
            }
            return Math.Log(num, logBase);
        }

        [McpServerTool, Description("Gives Power of a number")]
        public double PowNum(
            [Description("Target Number")] double num,
            [Description("Degree of power")] double p
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

            return Math.Pow(num, p);
        }
    }
}
