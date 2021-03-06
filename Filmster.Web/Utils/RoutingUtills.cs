﻿using System.Linq;
using System.Text.RegularExpressions;
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
                           {"controller", "Filmster"},
                           {"action", "Details"},
                           {"title", TitleToUrl(movie.Title)},
                           {"id", movie.Id}
                       };
        }

        public static Movie FindAlternateMovieByPath(IFilmsterRepository repository)
        {
            var titleFromPath = HttpContext.Current.Request.Url.PathAndQuery.Split('/')[1];
            return repository
                .GetAllMovies()
                .Where(m => TitleToUrl(m.Title) == titleFromPath)
                .FirstOrDefault();
        }

        private static string TitleToUrl(string title)
        {
            title = title.ToLower()
                .Replace("æ", "ae")
                .Replace("ø", "oe")
                .Replace("å", "aa")
                .Replace(" - ", "-")
                .Replace(" ", "-")
                .Replace("&", "and");

            title = Regex.Replace(title, @"[^\w-]", "");

            return title;
        }
    }
}