using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;

namespace KafkaSimpleDashboard.Server.Logging
{
        public static class RequestLogging
    {
        public static LogEventLevel CustomGetLevel(HttpContext ctx, double _, Exception ex) =>
            ex != null
                ? LogEventLevel.Error
                : ctx.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : LogEventLevel.Debug; //Debug instead of Information

        private static bool IsHealthCheckEndpoint(HttpContext ctx)
        {
            var endpoint = ctx.GetEndpoint();
            if (endpoint != null) // same as !(endpoint is null)
            {
                return string.Equals(
                    endpoint.DisplayName,
                    "Health checks",
                    StringComparison.Ordinal);
            }

            // No endpoint, so not a health check endpoint
            return false;
        }

        public static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception ex) =>
            ex != null
                ? LogEventLevel.Error
                : ctx.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : IsHealthCheckEndpoint(ctx) // Not an error, check if it was a health check
                        ? LogEventLevel.Verbose // Was a health check, use Verbose
                        : LogEventLevel.Information;

        public static void EnrichFromRequest(
            IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;
            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);
            if (request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            var endpoint = httpContext.GetEndpoint();
            if (endpoint != null)
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }
        }
    }
        
    public static class Extensions
    {
        private static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(section).Bind(model);
            return model;
        }

        public static IHostBuilder UseLogger(this IHostBuilder hostBuilder, string applicationName = null)
        {
            return hostBuilder.UseSerilog(((context, configuration) =>
            {
                var serilogOptions = context.Configuration.GetOptions<SerilogOptions>("Serilog");
                if (!Enum.TryParse<LogEventLevel>(serilogOptions.MinimumLevel, true, out var level))
                {
                    level = LogEventLevel.Information;
                }

                var conf = configuration
                    .MinimumLevel.Is(level)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", applicationName)
                    .Enrich.WithEnvironmentUserName()
                    .Enrich.WithProcessId()
                    .Enrich.WithProcessName()
                    .Enrich.WithThreadId()
                    .Enrich.WithExceptionDetails();

                conf.WriteTo.Async((logger) =>
                {
                    if (serilogOptions.ConsoleEnabled)
                    {
                        if (serilogOptions.Format.ToLower() == "elasticsearch")
                        {
                            logger.Console(new ElasticsearchJsonFormatter());
                        }
                        else if (serilogOptions.Format.ToLower() == "compact")
                        {
                            logger.Console(new CompactJsonFormatter());
                        }
                        else if (serilogOptions.Format.ToLower() == "colored")
                        {
                            logger.Console(theme: AnsiConsoleTheme.Code);
                        }
                    }

                    logger.Trace();
                });
            }));
        }
    }
}