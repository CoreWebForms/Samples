using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Web.Optimization;
using System.Web.UI.WebControls;
using System.Web.UI;
using WebForms.Compiler.Dynamic;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters;
using System.Web;
using Microsoft.AspNetCore.SystemWebAdapters.HttpHandlers;
using System.Web.SessionState;
using System.Threading.Tasks;

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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "images")),
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
