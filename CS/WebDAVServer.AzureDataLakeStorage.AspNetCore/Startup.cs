using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebDAVServer.AzureDataLakeStorage.AspNetCore.DataLake;

namespace WebDAVServer.AzureDataLakeStorage.AspNetCore
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.webdav.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            HostingEnvironment = env;
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddWebDav(Configuration, HostingEnvironment);
            services.AddDataLake(Configuration, HostingEnvironment);
            //Enables web sockets. Web sockets are used to update the documents list in case of any changes on the server.
            services.AddSingleton<WebSocketsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseAuthentication();
            app.UseAuthorization();

            //Enables web sockets. Web sockets are used to update the documents list in case of any changes on the server.
            app.UseWebSockets();
            app.UseWebSocketsMiddleware();
            app.UseWebDav(HostingEnvironment);     
        }
    }
}
