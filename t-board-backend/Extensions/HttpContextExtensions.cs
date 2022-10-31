using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace t_board_backend.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetCurrentUserId(this HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.User == null)
                throw new ArgumentNullException(nameof(context.User));

            var userId = context.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            return userId;
        }

        public static int GetCurrentUserCompanyId(this HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.User == null)
                throw new ArgumentNullException(nameof(context.User));

            var userCompany = context.User.Claims.FirstOrDefault(c => c.Type == "company")?.Value;
            if (userCompany == null)
                throw new ArgumentNullException(nameof(userCompany));

            var companyId = int.Parse(userCompany);

            return companyId;
        }

        public static bool IsCurrentUserAdmin(this HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.User == null)
                throw new ArgumentNullException(nameof(context.User));

            return context.User.IsInRole("Admin");
        }
    }
}
