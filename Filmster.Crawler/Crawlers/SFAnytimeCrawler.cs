using System;
using System.Collections.Generic;
using System.Threading;
using Filmster.Common;
using Filmster.Crawler;
using Filmster.Data;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class SFAnytimeCrawler : Crawlers.Crawler
    {
        private string _crawlstart = "http://sfanytime.com/Genre.mvc/List/{0}/{0}/Rent/overview/recent/{1}";

        public void Start()
        {
            var page = 1;
            var category = "114"; //action
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            var index = GetDocument(string.Format(_crawlstart, category, page));
            var indexLinks = index.DocumentNode.SelectNodes("//div[@id='3']//a");
            
            foreach (var link in indexLinks)
            {
                category = link.Attributes["href"].Value.RemoveNonNumericChars();
                while (resultContainsMovies)
                //while (page == 1)
                {
                    Logger.LogVerbose("Fetching page " + page);

                    var doc = GetDocument(string.Format(_crawlstart, category, page));

                    HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//a[@class='thumbImage']");

                    if (list == null)
                    {
                        resultContainsMovies = false;
                        break;
                    }

                    foreach (HtmlNode htmlNode in list)
                    {
                        moviesToLoad.Add(string.Format("{0};{1}", "http://sfanytime.com" + htmlNode.Attributes["href"].Value, htmlNode.SelectSingleNode("img").Attributes["src"].Value));
                    }
                    page++;
                }
            }


            StartedThreads = moviesToLoad.Count;

            foreach (var movie in moviesToLoad)
            {
                ThreadPool.QueueUserWorkItem(LoadMovie, movie);
            }

            DoneEvent.WaitOne();
        }

        public void LoadMovie(object obj)
        {
            try
            {
                var repository = new FilmsterRepository();
                var movieUrl = ((string)obj).Split(';')[0];
                var coverUrl = ((string)obj).Split(';')[1];
                var doc = GetDocument(movieUrl).DocumentNode;

                bool highDef = false;
                const int vendorId = 5;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//div[@class='thumbDetailTitle']").InnerText.Trim();
                var plot = doc.SelectSingleNode("//div[@class='thumbOverviewSummary']").InnerText.Trim();
                var porn = doc.SelectNodes("//div[@class='thumbDetailListTitle']")[0].NextSibling.NextSibling.InnerText.Trim().Equals("Voksenfilm", StringComparison.InvariantCultureIgnoreCase);

                if (title.Contains("- HD"))
                {
                    highDef = true;
                    title = title.Replace("- HD", "").Trim();
                }

                int.TryParse(doc.SelectNodes("//div[@class='thumbDetailTextDuration']")[0].InnerText.RemoveNonNumericChars(), out releaseYear);
                float.TryParse(doc.SelectSingleNode("//div[@class='thumbWhiteBuyRent ']//span[@class='thumbRentText']").InnerText.RemoveNonNumericChars(), out price);

                ResolveRentalOption(repository, movieUrl, coverUrl, vendorId, title, plot, releaseYear, porn, highDef, price);
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