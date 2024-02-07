using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using t_board.Services.Contracts;
using t_board.Services.Services;
using t_board.Services.Services.Scraper;

namespace t_board.Services
{
    public static class TBoardServiceCollectionExtensions
    {
        public static IServiceCollection AddTBoardServices(this IServiceCollection services)
        {
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IInviteService, InviteService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();

            // Scraper
            services.AddScoped<AjansPressScraper>();
            services.AddScoped<InterPressScraper>();
            services.AddScoped<MedyaTakipScraper>();

            services.AddScoped<ServiceResolver>(serviceProvider => key =>
            {
                switch (key)
                {
                    case "AjansPress":
                        return serviceProvider.GetService<AjansPressScraper>();
                    case "InterPress":
                        return serviceProvider.GetService<InterPressScraper>();
                    case "MedyaTakip":
                        return serviceProvider.GetService<MedyaTakipScraper>();
                    default:
                        throw new KeyNotFoundException();
                }
            });

            return services;
        }
    }
}