using System.Web;
using System.Web.Routing;
using Filmster.Data;

namespace Filmster.Web.Utils
{
    public static class RoutingUtills
    {
        public static RouteValueDictionary RouteValues(this Movie movie)
        {
            return new RouteValueDictionary
                       {
                           {"controller", "Home"},
                           {"action", "Details"},
                           {"title", TitleToUrl(movie.Title)},
                           {"id", movie.Id}
                       };
        }

        private static string TitleToUrl(string title)
        {
            title = title.ToLower()
                .Replace("æ", "ae")
                .Replace("ø", "oe")
                .Replace("å", "aa")
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("&", "and");

            return title;
        }
    }
}