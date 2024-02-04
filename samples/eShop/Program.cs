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

app.UseSession();
app.UseSystemWebAdapters();

app.MapWebForms();

app.Run();