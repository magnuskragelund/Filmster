using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Filmster.Crawler;
using Filmster.Data;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class Crawler
    {
        internal Encoding DocumentEncoding = Encoding.UTF8;
        internal ManualResetEvent DoneEvent = new ManualResetEvent(false);
        internal int StartedThreads;
        internal bool AllowUpdatePlot = true;

        private readonly HtmlWeb _htmlWeb = new HtmlWeb();

        internal HtmlDocument GetDocument(string url)
        {
            Logger.LogVerbose("Loading page: " + url);
            _htmlWeb.AutoDetectEncoding = false;
            _htmlWeb.OverrideEncoding = DocumentEncoding;
            HtmlDocument doc = _htmlWeb.Load(url);
            return doc;
        }

        internal HtmlDocument PostDocument(NameValueCollection formValueCollection, string url)
        {
            HtmlWeb.PreRequestHandler handler = delegate(HttpWebRequest request)
                                                    {
                                                        string payload = this.AssemblePostPayload(formValueCollection);
                                                        byte[] buff = Encoding.ASCII.GetBytes(payload.ToCharArray());
                                                        request.ContentLength = buff.Length;
                                                        request.ContentType = "application/x-www-form-urlencoded";
                                                        Stream reqStream = request.GetRequestStream();
                                                        reqStream.Write(buff, 0, buff.Length);
                                                        return true;
                                                    };
            _htmlWeb.PreRequest += handler;
            HtmlDocument doc = _htmlWeb.Load(url, "POST");
            _htmlWeb.PreRequest -= handler;
            return doc;
        }

        private string AssemblePostPayload(NameValueCollection fv)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String key in fv.AllKeys)
            {
                sb.Append("&" + key + "=" + fv.Get(key));
            }
            return sb.ToString().Substring(1);
        }

        internal void ResolveRentalOption(IFilmsterRepository repository ,string movieUrl, string coverUrl, int vendorId, string title, string plot, int releaseYear, bool porn, bool highDef, float price)
        {
            Movie movie;
            var existingMovie = repository.GetMovie(title, null);
            
            if (existingMovie != null)
            {
                if(AllowUpdatePlot)
                {
                    existingMovie.Plot = plot;
                }

                movie = existingMovie;
            }
            else
            {
                DateTime? releaseDate = (releaseYear == 0) ? null : new DateTime().AddYears(releaseYear) as DateTime?;
                movie = new Movie()
                            {
                                Title = title,
                                Plot = plot,
                                ReleaseDate = releaseDate,
                                Porn = false
                            };
                repository.AddMovie(movie);
                Logger.Log("New Movie found: " + movie.Title);
            }

            RentalOption rentalOption;
            var existingOption = repository.GetRentalOption(movie.Id, vendorId, highDef);
            if (existingOption != null)
            {
                rentalOption = existingOption;
                rentalOption.Price = price;
                rentalOption.Url = movieUrl;
                rentalOption.CoverUrl = coverUrl;
                rentalOption.LastSeen = DateTime.Now;
            }
            else
            {
                rentalOption = new RentalOption()
                                   {
                                       Movie = movie,
                                       Vendor = repository.GetVendor(vendorId),
                                       Price = price,
                                       Url = movieUrl,
                                       CoverUrl = coverUrl,
                                       HighDefinition = highDef,
                                       Added = DateTime.Now,
                                       LastSeen = DateTime.Now
                                   };
                repository.AddRentalOption(rentalOption);
                Logger.Log("New RentalOption found for movie: " + movie.Title);
            }
        }

        protected void LogCrawlerError(Exception exception)
        {
            Logger.LogException("Crawler failed: ", exception);
        }
    }
}