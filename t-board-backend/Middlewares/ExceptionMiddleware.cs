using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;
using t_board_backend.Models;

namespace t_board_backend.Middlewares
{

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration Configuration;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<ExceptionMiddleware> logger)
        {
            Configuration = configuration;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new BaseResponse()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error"
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));

            await LogError(exception);
        }

        private async Task LogError(object error)
        {
            var serverURL = Configuration["Seq:SERVER_URL"];
            var apiKey = Configuration["Seq:API_KEY"];

            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .WriteTo.Seq(serverURL, apiKey: apiKey)
                .CreateLogger();

            var serializerSetting = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var serializedError = JsonConvert.SerializeObject(error, serializerSetting);

            Serilog.Log.Error(serializedError);

            await Serilog.Log.CloseAndFlushAsync();
        }
    }
}
