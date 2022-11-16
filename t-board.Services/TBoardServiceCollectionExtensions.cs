using Microsoft.Extensions.DependencyInjection;
using t_board.Services.Contracts;
using t_board.Services.Services;

namespace t_board.Services
{
    public static class TBoardServiceCollectionExtensions
    {
        public static IServiceCollection AddTBoardServices(this IServiceCollection services)
        {
            // SINGLETON
            services.AddSingleton<IMailService, MailService>();

            // SCOPED
            services.AddScoped<IInviteService, InviteService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}