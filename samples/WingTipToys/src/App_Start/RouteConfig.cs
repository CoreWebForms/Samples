using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
//#if !NET
//using Microsoft.AspNet.FriendlyUrls;//
//#endif

namespace WingtipToys
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(System.Web.Routing.RouteCollection routes)
        {
//#if !NET
/*
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;

            routes.EnableFriendlyUrls(settings);
*/
//#endif
        }
    }
}
