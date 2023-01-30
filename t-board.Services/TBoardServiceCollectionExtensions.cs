using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using t_board.Services.Contracts;
using t_board.Services.Services;
using t_board.Services.Services.Scrapper;

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

            // Scrapper
            services.AddScoped<AjansPressScrapper>();
            services.AddScoped<InterPressScrapper>();
            services.AddScoped<MedyaTakipScrapper>();

            services.AddScoped<ServiceResolver>(serviceProvider => key =>
            {
                switch (key)
                {
                    case "AjansPress":
                        return serviceProvider.GetService<AjansPressScrapper>();
                    case "InterPress":
                        return serviceProvider.GetService<InterPressScrapper>();
                    case "MedyaTakip":
                        return serviceProvider.GetService<MedyaTakipScrapper>();
                    default:
                        throw new KeyNotFoundException();
                }
            });

            return services;
        }
    }
}