using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
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

// Load System.Drawing
_ = typeof(System.Drawing.Bitmap);

builder.Services.AddSystemWebAdapters()
    .AddWrappedAspNetCoreSession()
    .AddHttpApplication<Global>()
    .AddWebForms()
    .AddDynamicPages()
    .AddPrefix<ScriptManager>("asp") // For WebForms.Extensions
    .AddPrefix<ListView>("asp") // For WebForms.Extensions
    .AddPrefix<BundleReference>("webopt"); // For WebForms.Optimization

var app = builder.Build();

foreach (var staticPath in new[] { "Content", "images", "Pics", "fonts" })
{
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, staticPath)),
        RequestPath = "/" + staticPath,
    });
}

// Probably should add this as a default or ability to opt in as it was automatic in WebForms
app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
{
    RouteTable.Routes.MapPageRoute("MainPage", "/", "~/Default.aspx");
});

// Post back seems to require this for now...
app.Use((ctx, next) =>
{
    ctx.Features.GetRequiredFeature<IHttpBodyControlFeature>().AllowSynchronousIO = true;
    return next(ctx);
});

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
