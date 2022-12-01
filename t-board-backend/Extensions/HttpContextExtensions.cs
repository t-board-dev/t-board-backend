using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using t_board.Services.Contracts;
using t_board.Services.Services;

namespace t_board_backend.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task<bool> IsAuth(this HttpContext context)
        {
            var token = await context.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(token))
                return false;

            var _jwtService = (JwtService)context.RequestServices.GetService(typeof(IJwtService));
            return _jwtService.IsTokenValid(token);
        }

        private static async Task<Claim> GetClaimByType(this HttpContext context, string type)
        {
            var token = await context.GetTokenAsync("access_token");

            var _jwtService = (JwtService)context.RequestServices.GetService(typeof(IJwtService));

            var claims = _jwtService.GetTokenClaims(token);
            return claims.FirstOrDefault(c => c.Type == type);
        }

        public static async Task<string> GetCurrentUserId(this HttpContext context)
        {
            var idClaim = await context.GetClaimByType("id");

            if (idClaim == null)
                throw new(nameof(idClaim));

            return idClaim.Value;
        }

        public static async Task<int> GetCurrentUserCompanyId(this HttpContext context)
        {
            var companyClaim = await context.GetClaimByType("company");

            if (companyClaim == null)
                throw new(nameof(companyClaim));

            var companyId = int.Parse(companyClaim.Value);

            return companyId;
        }

        public static bool IsCurrentUserAdmin(this HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.User == null)
                throw new(nameof(context.User));

            return context.User.IsInRole("Admin");
        }
    }
}
