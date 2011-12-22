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
            var randomMovies = _repo.GetPopularMovies(50);
            return View(randomMovies);
        }

        public ActionResult Details(int id)
        {
            var m = _repo.GetMovie(id);
            if(m == null)
            {
                m = RoutingUtills.FindAlternateMovieByPath(_repo);

                if(m == null)
                {
                    return new HttpNotFoundResult("Movie not found");
                }
                Response.RedirectPermanent(Url.RouteUrl(m.RouteValues()));
            }

            EnforceCanoncalUrl(m.RouteValues());
            ViewBag.OtherMovies = _repo.GetPopularMovies(5);
            m.Impressions++;
            _repo.Save(false);
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

        public RedirectResult Rent(int rentalOptionId)
        {
            var rentalOption = _repo.GetRentalOption(rentalOptionId);
            rentalOption.Impressions++;
            _repo.Save(false);

            var url = string.Empty;

            switch (rentalOption.Vendor.Name)
            {
                case "CDON":
                    url = string.Format("http://clk.tradedoubler.com/click?p=120&a=2050910&g=0&url={0}", rentalOption.Url);
                    break;
                default:
                    url = rentalOption.Url;
                    break;
            }

            return Redirect(url);
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
