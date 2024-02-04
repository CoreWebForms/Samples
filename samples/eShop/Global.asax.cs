using Autofac;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using log4net;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

#if NET
using Microsoft.AspNetCore.Hosting;
#else
using Autofac.Integration.Web;
#endif

namespace eShopLegacyWebForms
{
    // Can probably clean this kind of sharing up once HttpRuntime.WebObjectActivator is available
#if !NET
    partial class Global : IContainerProviderAccessor, IServiceProvider
    {
        static IContainerProvider _containerProvider;
        IContainer container;

        public IContainerProvider ContainerProvider
        {
            get { return _containerProvider; }
        }

        object IServiceProvider.GetService(Type serviceType) => container.Resolve(serviceType);

        public IServiceProvider Services => this;
    }
#endif

    public partial class Global : HttpApplication
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#if NET
        public Global(IServiceProvider services)
        {
            Services = services;
        }

        private IServiceProvider Services { get; }

#endif

        protected void Application_Start(object sender, EventArgs e)
        {
#if !NET
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
#endif

#if !NET
            ConfigureContainer();
#endif
            ConfigDataBase();
        }

        /// <summary>
        /// Track the machine name and the start time for the session inside the current session
        /// </summary>
        protected void Session_Start(Object sender, EventArgs e)
        {
            HttpContext.Current.Session["MachineName"] = Environment.MachineName;
            HttpContext.Current.Session["SessionStartTime"] = DateTime.Now;
        }


#if !NET
        /// <summary>
        /// http://docs.autofac.org/en/latest/integration/webforms.html
        /// </summary>
        private void ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            ConfigureServices(builder);

            container = builder.Build();
            _containerProvider = new ContainerProvider(container);
        }
#endif

        public static void ConfigureServices(ContainerBuilder builder)
        {
            var mockData = bool.Parse(ConfigurationManager.AppSettings["UseMockData"]);
            builder.RegisterModule(new ApplicationModule(mockData));
        }

        private void ConfigDataBase()
        {
            var mockData = bool.Parse(ConfigurationManager.AppSettings["UseMockData"]);

            if (!mockData)
            {
                Database.SetInitializer<CatalogDBContext>((CatalogDBInitializer)Services.GetService(typeof(CatalogDBInitializer)));
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //set the property to our new object
            LogicalThreadContext.Properties["activityid"] = new ActivityIdHelper();

            LogicalThreadContext.Properties["requestinfo"] = new WebRequestInfo();

            _log.Debug("Application_BeginRequest");
        }
    }

    public class ActivityIdHelper
    {
        public override string ToString()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }

            return Trace.CorrelationManager.ActivityId.ToString();
        }

    }
    public class WebRequestInfo
    {
        public override string ToString()
        {
            return HttpContext.Current?.Request?.RawUrl + ", " + HttpContext.Current?.Request?.UserAgent;
        }
    }
}