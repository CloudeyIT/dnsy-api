using DnsClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Dnsy.Api
{
    public class Startup
    {
        public Startup (IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Dnsy.Api", Version = "v1"}); });

            var lookupClient = new LookupClient(
                NameServer.GooglePublicDns
            );
            services.AddSingleton<ILookupClient>(lookupClient);
            services.AddCors(options => options.AddDefaultPolicy(policy => policy
                .WithOrigins(Configuration.GetSection("Origins").Get<string[]>())
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod()
                .AllowAnyHeader()
            ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dnsy.Api v1"));
            }

            app.UseCors();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}