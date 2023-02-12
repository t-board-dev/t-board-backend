using Microsoft.AspNetCore.Builder;
using t_board_backend.Middlewares;

namespace t_board_backend.Extensions
{
    public static class WebApplicationExtensions
    {
        public static void ConfigureCustomExceptionMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
