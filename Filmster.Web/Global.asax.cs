using System;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Filmster.Data;

namespace Filmster.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                null,
                "{title}/{id}",
                new { controller = "Home", action = "Details"},
                new {id = @"\d+" });

            routes.MapRoute(
                null,
                "soeg/",
                new { controller = "Home", action = "Search" });

            routes.MapRoute(
                null,
                "katalog/{id}",
                new { controller = "Home", action = "Catalog" });

            routes.MapRoute(
                null, // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            Database.SetInitializer(new FilmsterInitializer());
        }        
    }
}