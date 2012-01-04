using System.Web;
using System.Web.Mvc;

namespace Filmster.Web.Controllers
{
    public class BlogController : Controller
    {
        public ActionResult Post(string id)
        {
            ViewEngineResult viewResult = ViewEngines.Engines.FindView(ControllerContext, id, null);

            if (viewResult.View == null)
            {
                return new HttpNotFoundResult("Post not found");
            }

            return View(id);
        }
    }
}
