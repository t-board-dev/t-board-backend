using Microsoft.Extensions.DependencyInjection;
using t_board.Services.Contracts;
using t_board.Services.Services;

namespace t_board.Services
{
    public static class TBoardServiceCollectionExtensions
    {
        public static IServiceCollection AddTBoardServices(this IServiceCollection services)
        {
            services.AddTransient<IInviteService, InviteService>();

            return services;
        }
    }
}