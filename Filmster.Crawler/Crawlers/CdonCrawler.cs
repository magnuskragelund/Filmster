using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Filmster.Crawler;
using Filmster.Data;
using Filmster.Utilities;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class CdonCrawler : Crawler
    {
        private string _crawlstart =
            "http://cdon.dk/film/video-on-demand/vis_alle_film/default?179902-display=1&179902-page-size=50&179902-sort-order=139&179902-page={0}";

        public void Start()
        {
            var page = 0;
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            Logger.Log("Starting CDON Crawler");

            try
            {
                while (resultContainsMovies)
                //while (page == 0)
                {
                    page++;

                    var doc = GetDocument(string.Format(_crawlstart, page));

                    HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//table[@class='product-list']//td[@class='title']/a");

                    if (list == null || list.Count < 1)
                    {
                        resultContainsMovies = false;
                        break;
                    }

                    foreach (HtmlNode htmlNode in list)
                    {
                        moviesToLoad.Add("http://cdon.dk" + htmlNode.Attributes["href"].Value);
                    }
                }

                StartedThreads = moviesToLoad.Count;

                Logger.Log("Found movies: " + StartedThreads);

                foreach (var movie in moviesToLoad)
                {
                    ThreadPool.QueueUserWorkItem(LoadMovie, movie);
                }

                DoneEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Logger.LogException("CDON failed to load index", ex);
            }

            Logger.Log("Ending CDON crawler");
        }

        public void LoadMovie(object obj)
        {
            try
            {
                var repository = new FilmsterRepository();
                var movieUrl = (string)obj;

                var doc = GetDocument(movieUrl).DocumentNode;

                bool highDef = false;
                const int vendorId = 8;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//h1").InnerText;
                var plot = doc.SelectSingleNode("//div[@class='description-container']").LastChild.InnerText;
                plot = Regex.Replace(plot, "OBS! Ingen undertekster.", "", RegexOptions.IgnoreCase);

                string coverUrl;
                if (doc.SelectSingleNode("//div[@class='product-image-container']/a") != null)
                {
                    coverUrl = doc.SelectSingleNode("//div[@class='product-image-container']/a").Attributes["href"].Value;
                }
                else
                {
                    coverUrl = doc.SelectSingleNode("//div[@class='product-image-container']/img").Attributes["src"].Value;
                }

                if (title.Contains(" (HD)"))
                {
                    highDef = true;
                    title = title.Replace(" (HD)", "");
                }

                int.TryParse(doc.InnerText.TrySubstringByStringToString("Releasedato:", "&nbsp;", false).RemoveNonNumericChars(), out releaseYear);
                float.TryParse(
                        doc.SelectSingleNode("//div[@class='price']").InnerText.Replace(" kr", ""), NumberStyles.Any, new CultureInfo("en-US").NumberFormat,
                        out price);


                ResolveRentalOption(repository, movieUrl, coverUrl, vendorId, title, plot, releaseYear, false, highDef, price);
                repository.Save();

                Logger.LogVerbose(title.Trim());


            }
            catch (Exception ex)
            {
                LogCrawlerError(ex);
            }

            if (Interlocked.Decrement(ref StartedThreads) == 0)
            {
                DoneEvent.Set();
            }

        }
    }
}