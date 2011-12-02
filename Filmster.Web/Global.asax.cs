using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using Canonicalize;
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

            routes.Canonicalize().NoWww().Lowercase().NoTrailingSlash();

            routes.MapRoute(
                null,
                "robots.txt",
                new
                    {
                        controller = "Filmster",
                        action = "RobotsText"
                    }
                );

            routes.MapRoute(
                null,
                "lej/{rentalOptionId}",
                new { controller = "Filmster", action = "Rent" },
                new { rentalOptionId = @"\d+" });

            routes.MapRoute(
                null,
                "{title}/{id}",
                new { controller = "Filmster", action = "Details" },
                new { id = @"\d+" });

            routes.MapRoute(
                null,
                "om-filmster",
                new { controller = "Filmster", action = "About" });

            routes.MapRoute(
                null,
                "soeg/",
                new { controller = "Filmster", action = "Search" });

            routes.MapRoute(
                null,
                "katalog/{id}",
                new { controller = "Filmster", action = "Catalog" });

            routes.MapRoute(
                null, // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Filmster", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }        
    }
}