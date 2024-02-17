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
    .AddPreApplicationStartMethod()
    .AddWrappedAspNetCoreSession()
    .AddRouting(defaultPage: "~/Default.aspx")
    .AddHttpApplication<Global>()
    .AddWebForms()
    .AddOptimization()
    .AddScriptManager()
    .AddDynamicPages();

var app = builder.Build();

foreach (var staticPath in new[] { "Content", "images", "Pics", "fonts", "Scripts" })
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
app.MapScriptManager();
app.MapBundleTable();

app.Run();
