using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Filmster.Crawler;
using Filmster.Data;
using Filmster.Utilities;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class SputnikCrawler : Crawler
    {
        private string _crawlstart = "http://play.tv2.dk/lejefilm/";

        public SputnikCrawler()
        {
            DocumentEncoding = Encoding.GetEncoding("ISO-8859-1");
            AllowUpdatePlot = false;
        }

        public void Start()
        {
            Logger.Log("Starting SputnikCrawler");

            var moviesToLoad = new List<string>();
            var doc = GetDocument(_crawlstart);

            HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//div[@id='content']//a[contains(@href, '/lejefilm/')]");

            foreach (HtmlNode htmlNode in list)
            {
                var path = htmlNode.Attributes["href"].Value;
                if (path.Split(new [] { "/" }, StringSplitOptions.RemoveEmptyEntries).Length < 3) continue;
                moviesToLoad.Add("http://play.tv2.dk" + path);
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

                const bool porn = false;
                const int releaseYear = 0;
                const int vendorId = 2;
                
                float price = 0;

                var title = doc.SelectSingleNode("//div[@id='content']//div[@class='info']//h1").InnerText.Trim();
                var plot = doc.SelectSingleNode("//div[@class='content']").InnerText.Replace(title, "").Trim();
                var coverUrl = string.Empty;
                float.TryParse(doc.InnerText.SubstringByStringToString("\"price\":", ",", false), out price);

                ResolveRentalOption(repository, movieUrl, coverUrl, vendorId, title, plot, releaseYear, porn, false, price);

                repository.Save();

                Logger.LogVerbose("Done loading " + title.Trim());
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