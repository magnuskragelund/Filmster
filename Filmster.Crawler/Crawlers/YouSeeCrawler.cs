using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using Filmster.Common;
using Filmster.Crawler;
using Filmster.Data;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class YouSeeCrawler : Crawler
    {
        private string _crawlstart = "http://yousee.tv/film/liste/genre/alle/{0}/alfabetisk/";

        public void Start()
        {
            Logger.Log("Starting YouSeeCrawler");

            var moviesToLoad = new List<string>();
            var page = 1;
            var doc = GetDocument(string.Format(_crawlstart, page));

            HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//div[@class='list']//a[contains(@href, '/film/')]");

            //while (list != null && list.Count > 0)
                while (page == 1)
            {
                page++;
                foreach (HtmlNode htmlNode in list)
                {
                    var path = htmlNode.Attributes["href"].Value;
                    moviesToLoad.Add("http://yousee.tv" + path);
                }

                doc = GetDocument(string.Format(_crawlstart, page));
                list = doc.DocumentNode.SelectNodes("//div[@class='list']/div/a[contains(@href, '/film/')]");
            }

            StartedThreads = moviesToLoad.Count;

            Logger.Log("Movies found: " + StartedThreads);


            foreach (var movie in moviesToLoad)
            {
                ThreadPool.QueueUserWorkItem(LoadMovie, movie);
            }

            DoneEvent.WaitOne();
            Logger.Log("Ending SputnikCrawler");
        }

        public void LoadMovie(object obj)
        {
            try
            {
                var repository = new FilmsterRepository();
                var movieUrl = (string)obj;

                var doc = GetDocument(movieUrl).DocumentNode;

                if(!doc.InnerText.Contains("DKK"))
                {
                    DecrementThreadCount();
                    Logger.LogVerbose("Skipping movie due to access restrictions");
                    return;
                }

                const bool porn = false;
                const int vendorId = 6;
                
                float price = 0;
                int releaseYear = 0;
                var title = doc.SelectSingleNode("//div[@class='summary']//h1").InnerText.Trim();
                var plot = HttpUtility.HtmlDecode(doc.SelectSingleNode("//div[@class='summary']/p").InnerText.Trim());
                var coverUrl = doc.SelectSingleNode("//div[@id='movie_info']/a/img").Attributes["src"].Value;
                float.TryParse(doc.InnerText.SubstringByStringToString("DKK", ",-", false), out price);
                int.TryParse(doc.SelectNodes("//div[@class='summary']/table//td[@class='col2']")[0].InnerText, out releaseYear);

                ResolveRentalOption(repository, movieUrl, coverUrl, vendorId, title, plot, releaseYear, porn, false, price);

                repository.Save();

                Logger.LogVerbose("Done loading " + title.Trim());
            }
            catch (Exception ex)
            {
                LogCrawlerError(ex);
            }

            DecrementThreadCount();
        }

        private void DecrementThreadCount()
        {
            if (Interlocked.Decrement(ref StartedThreads) == 0)
            {
                DoneEvent.Set();
            }
        }
    }
}