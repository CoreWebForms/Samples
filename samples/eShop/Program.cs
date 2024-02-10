using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;

var builder = WebApplication.CreateBuilder(args);

builder.UseWebConfig(isOptional: false);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>((ctx, builder) =>
{
    Global.ConfigureServices(builder);
});

builder.Services.AddDataProtection();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSystemWebAdapters()
    .AddWrappedAspNetCoreSession()
    .AddRouting(defaultPage: "~/Default.aspx")
    .AddHttpApplication<Global>()
    .AddWebForms()
    .AddDynamicPages();


var app = builder.Build();

foreach (var staticPath in new[] { "Content", "images", "Pics", "fonts" })
{
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, staticPath)),
        RequestPath = "/" + staticPath,
    });
}

// We used property injection in ASP.NET Framework, so let's force it to do so for handlers (the only place we need them)
app.Use((ctx, next) =>
{
    if (ctx.AsSystemWeb().GetHandler() is { } handler)
    {
        var scope = ctx.RequestServices.GetRequiredService<ILifetimeScope>();

        scope.InjectUnsetProperties(handler);
    }

    return next(ctx);
});

app.UseSession();
app.UseSystemWebAdapters();

app.MapWebForms();

app.Run();
