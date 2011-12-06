using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Filmster.Crawler;
using Filmster.Data;
using Filmster.Utilities;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class ItunesCrawler : Crawler
    {
        private string _crawlstart = "http://itunes.apple.com/dk/genre/films/id33";

        public ItunesCrawler()
        {
            AllowUpdatePlot = false;
        }

        public void Start()
        {
            Logger.Log("Starting iTunes Crawler");
            var moviesToLoad = new List<string>();

            var index = GetDocument(_crawlstart);
            var genreLinks = index.DocumentNode.SelectNodes("//ul[contains(@class, 'list column')]//a");
            
            foreach (var genreLink in genreLinks)
            {
                var genreDoc = GetDocument(genreLink.Attributes["href"].Value);

                var movieLinks = genreDoc.DocumentNode.SelectNodes("//div[contains(@class, 'column')]//a[contains(@href, 'movie')]");

                if(movieLinks == null) continue;

                foreach (var movieLink in movieLinks)
                {
                    moviesToLoad.Add(movieLink.Attributes["href"].Value);
                }
            }


            StartedThreads = moviesToLoad.Count;
            Logger.Log("Found " + moviesToLoad.Count + " movies");

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
                var movieUrl = (string)obj;
                var doc = GetDocument(movieUrl).DocumentNode;

                bool highDef = false;
                const int vendorId = 7;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//h1").InnerText.Trim();
                var plot = doc.SelectSingleNode("//div[@class='plot-summary']/p").InnerText.Trim();
                var coverUrl = doc.SelectSingleNode("//div[@class='lockup product movie video']//div[@class='artwork']//img").Attributes["src"].Value;
                var porn = false;

                int.TryParse(doc.SelectSingleNode("//li[@class='release-date']").InnerHtml.SubstringByStringToString("Released: </span>", "copyright", false).RemoveNonNumericChars(), out releaseYear);
                float.TryParse(doc.SelectSingleNode("//span[@class='price']").InnerText.Replace("Kr", "").Replace("kr", ""), NumberStyles.Any, new CultureInfo("en-US").NumberFormat, out price);

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