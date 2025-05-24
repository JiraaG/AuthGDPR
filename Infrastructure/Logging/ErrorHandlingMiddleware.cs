using AuthGDPR.Domain.Entities.Auth;
using AuthGDPR.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace AuthGDPR.Infrastructure.Logging
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ErrorHandlingMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Creazione di uno scope per risolvere IAuditLogService (servizio scoped)
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                await auditLogService.LogEventAsync(
                    userId: Guid.Empty, // eventualmente recupera lo userId dal contesto se disponibile
                    messageCategory: MessageCategory.Errore,
                    actionType: ActionType.InternalServerError,
                    entityName: "Exception",
                    entityId: "0",
                    description: exception.Message,
                    ipAddress: context.Connection.RemoteIpAddress?.ToString(),
                    traceId: context.TraceIdentifier
                );
            }

            var errors = new Dictionary<string, string[]>
            {
                { "Exception", new [] { $"{exception.Message}" } }
            };

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc9110",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = context.Request.Path
            };
            // Aggiungi il traceId nella sezione Extensions
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}
