using System.Runtime.Loader;
using System.Web.Optimization;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.Routing;

namespace WingtipToys
{
    public class Program
    {
        static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDataProtection();

            builder.Services.AddSession();
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSystemWebAdapters()
            .AddWrappedAspNetCoreSession()
            .AddHttpApplication<Global>()
            .AddWebForms()
            .AddDynamicPages()
            .AddPrefix<ScriptManager>("asp") // For WebForms.Extensions
            .AddPrefix<ListView>("asp") // For WebForms.Extensions
            .AddPrefix<BundleReference>("webopt"); // For WebForms.Optimization

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSession();
            app.UseSystemWebAdapters();

            app.MapGet("/acls", () => AssemblyLoadContext.All.Select(acl => new
            {
                Name = acl.Name,
                Assemblies = acl.Assemblies.Select(a => a.FullName)
            }));

            app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
            {
                RouteTable.Routes.MapPageRoute("MainPage", "/", "~/Default.aspx");
            });

            app.MapWebForms();
            app.MapHttpHandlers();

            app.Run();
        }
    }
}
