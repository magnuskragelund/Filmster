using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Filmster.Data;
using Filmster.Web.Models;
using Filmster.Web.Utils;

namespace Filmster.Web.Controllers
{
    public class HomeController : Controller
    {
        private IFilmsterRepository _repo;

        public HomeController()
            :this(new FilmsterRepository())
        {
            
        }

        public HomeController(IFilmsterRepository filmsterRepository)
        {
            _repo = filmsterRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            var m = _repo.GetMovie (id);
            EnforceCanoncalUrl(m.RouteValues());
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
