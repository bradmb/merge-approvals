using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using MergeApprover.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MergeApprover
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x => x.UseMemoryStorage());
            services.Configure<ConfigurationOptions>(Configuration.GetSection("GitLab"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ConfigurationOptions>>().Value);
            services.AddScoped<IGitLabManager, GitLabManager>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHangfireServer();
            app.UseHangfireDashboard();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run((context) =>
            {
                context.Response.Redirect("/hangfire");
                return Task.CompletedTask;
            });

            RecurringJob.AddOrUpdate<IGitLabManager>("Process Merge Requests", gl => gl.ProcessMergeRequests(), Cron.Minutely);
        }
    }
}
