using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Filmster.Data;
using Filmster.Web.Models;
using Filmster.Web.Utils;

namespace Filmster.Web.Controllers
{
    public class FilmsterController : Controller
    {
        private IFilmsterRepository _repo;

        public FilmsterController()
            :this(new FilmsterRepository())
        {
            
        }

        public FilmsterController(IFilmsterRepository filmsterRepository)
        {
            _repo = filmsterRepository;
        }

        public ActionResult Index()
        {
            var randomMovies = _repo.GetRandomMovies(5, 3);
            return View(randomMovies);
        }

        public ActionResult Details(int id)
        {
            var m = _repo.GetMovie (id);
            EnforceCanoncalUrl(m.RouteValues());
            ViewBag.OtherMovies = _repo.GetRandomMovies(5, 1);
            return View(m);
        }

        public ActionResult Search()
        {
            var query = HttpUtility.UrlDecode(Request.QueryString["q"]);
            List<Movie> result;
            if(string.IsNullOrEmpty(query))
            {
                result = new List<Movie>();
                query = string.Empty;
            }
            else
            {
                result = _repo.Query(query);
            }
            ViewBag.Query = query;

            return View(result);
        }

        public ActionResult Catalog(string id)
        {
            var viewModel = new CatalogViewModel
                                {
                                    Movies = _repo.GetMoviesByTitleFistChar(id),
                                    SelectedValue = id
                                };

            return View(viewModel);
        }

        public JsonResult AutoComplete(string q)
        {
            var data = _repo.Query(q, true);
            return Json(data.Select(m => new { m.Title, Url = (Url.RouteUrl(m.RouteValues())) }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            return View();
        }

        public FileContentResult RobotsText()
        {
            var content = "User-agent: *" + Environment.NewLine;

            if (string.Equals(ConfigurationManager.AppSettings["Environment"], "production", StringComparison.InvariantCultureIgnoreCase))
            {
                content += "Disallow: /elmah.axd" + Environment.NewLine;
            }
            else
            {
                content += "Disallow: /" + Environment.NewLine;
            }

            return File(
                    Encoding.UTF8.GetBytes(content),
                    "text/plain");
        }

        private void EnforceCanoncalUrl(RouteValueDictionary routeValues)
        {
            string canonicalPathAndQuery = Url.RouteUrl(routeValues);

            if(Request.Url.PathAndQuery != canonicalPathAndQuery)
            {
                Response.RedirectPermanent(canonicalPathAndQuery);
            }
        }

    }
}
