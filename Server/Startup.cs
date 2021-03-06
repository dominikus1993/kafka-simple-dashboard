using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using KafkaSimpleDashboard.Server.Infrastructure.IoC;
using KafkaSimpleDashboard.Server.Infrastructure.SignalR;
using KafkaSimpleDashboard.Server.Logging;
using KafkaSimpleDashboard.Server.Services.Abstractions;
using KafkaSimpleDashboard.Server.Services.Implementations;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace KafkaSimpleDashboard.Server
{
    public class Startup
    {
        public const string PathBaseEnviromentVariable = "PATH_BASE";

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public string PathBase => Configuration[PathBaseEnviromentVariable] ?? string.Empty;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddRazorPages();
            services.AddInfrastructure(Configuration);
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!string.IsNullOrEmpty(PathBase))
            {
                Log.Logger.Information("Set BasePath {PathBase}", PathBase);
                app.UsePathBase(PathBase);
            }
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseResponseCompression();
            
            app.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = RequestLogging.EnrichFromRequest;
                opts.GetLevel = RequestLogging.ExcludeHealthChecks; // Use the custom level
            });

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/ping", new HealthCheckOptions()
                {
                    Predicate = r => r.Name.Contains("self"),
                    ResponseWriter = PongWriteResponse,
                });
                endpoints.MapHub<KafkaMessagesHub>("/kafkahub");
                endpoints.MapFallbackToFile("index.html");
            });
        }
        
        private static Task PongWriteResponse(HttpContext httpContext,
            HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            return httpContext.Response.WriteAsync("pong");
        }
    }
}
