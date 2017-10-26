using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebServer
{
    public class Startup
    {

        string serverUrl = Program.Configuration["server.urls"];

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
        IHostingEnvironment env,
        ILoggerFactory loggerFactory,
        IApplicationLifetime appLifeTime)
        {
            appLifeTime.ApplicationStarted.Register(async () => await OnStart());
            appLifeTime.ApplicationStopping.Register(async () => await OnStop());
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //simple web api server that always returns hello world
            app.Run(async (context) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                await context.Response.WriteAsync($"Hello World! from server {serverUrl}");
            });
        }

        private async Task OnStart()
        {
            //register service
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60000");
                var resp = await client.PutAsync($"/registry?server={serverUrl}", null);
            }

        }

        private async Task OnStop()
        {
            //remove service from registry
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:60000");
                var resp = await client.DeleteAsync("/registry?server=http://localhost:60001");
            }
        }

    }
}
