using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Filmster.Data;

namespace Filmster.Web
{
    public class CoverHandler : IHttpHandler
    {
        private const int ImageMaxWidth = 1200;
        private IFilmsterRepository _repo = new FilmsterRepository();

        public void ProcessRequest(HttpContext context)
        {
            string cacheDirectory = ConfigurationManager.AppSettings["CoverCacheDirectory"];
            int width = 130;

            int movieId;
            Movie movie;

            if (!String.IsNullOrEmpty(context.Request.QueryString["w"]))
            {
                int.TryParse(context.Request.QueryString["w"], out width);
            }

            width = Math.Min(width, ImageMaxWidth);

            if (!int.TryParse(context.Request.QueryString["id"], out movieId))
            {
                throw new HttpException(404, "No such image");
            }

            movie = _repo.GetMovie(movieId);

            if (movie == null)
            {
                throw new HttpException(404, "No such image");
            }

            string filename = string.Format("{0}_{1}.jpg", movieId, width);
            string file = Path.Combine(cacheDirectory, filename);
            if (!File.Exists(file))
            {
                WebRequest req = WebRequest.Create(movie.RentalOptions.Last().CoverUrl);
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();
                Bitmap b = new Bitmap(stream);

                decimal factor = Decimal.Divide(width, b.Width);

                Bitmap target = new Bitmap(width, Convert.ToInt32(b.Height * factor));
                using (Graphics graphics = Graphics.FromImage(target))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(b, 0, 0, target.Width, target.Height);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        target.Save(memoryStream, ImageFormat.Jpeg);
                        Stream fileStream = File.Create(file);
                        memoryStream.WriteTo(fileStream);
                        fileStream.Close();
                        memoryStream.WriteTo(context.Response.OutputStream);
                    }
                }
            }
            else
            {
                using (new MemoryStream())
                {
                    context.Response.WriteFile(file);
                }
            }


            context.Response.ContentType = "image/jpeg";

            HttpCachePolicy cachePolicy = context.Response.Cache;
            cachePolicy.SetCacheability(HttpCacheability.NoCache);
            cachePolicy.VaryByParams["id"] = true;
            cachePolicy.VaryByParams["w"] = true;
            cachePolicy.VaryByParams["flush"] = true;
            cachePolicy.SetOmitVaryStar(true);
            cachePolicy.SetExpires(DateTime.Now + TimeSpan.FromDays(-1));
            cachePolicy.SetValidUntilExpires(true);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}