using System;
using System.Collections.Generic;
using System.Threading;
using Filmster.Crawler;
using Filmster.Data;
using Filmster.Utilities;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class HeadwebCrawler : Crawler
    {
        private string _crawlstart =
            "http://www.headweb.com/da/5906/film?&page={0}";

        public HeadwebCrawler()
        {
            AllowUpdatePlot = false;
        }

        public void Start()
        {
            var page = 0;
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            Logger.Log("Starting Headweb Crawler");

            while (resultContainsMovies)
            //while (page < 1)
            {
                page++;

                var doc = GetDocument(string.Format(_crawlstart, page));

                HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//div[contains(@class, 'coverEntry')]/a");

                if (doc.DocumentNode.SelectNodes("//a[@class='next']") == null)
                {
                    resultContainsMovies = false;
                }

                foreach (HtmlNode htmlNode in list)
                {
                    moviesToLoad.Add("http://www.headweb.com" + htmlNode.Attributes["href"].Value);
                }
            }

            StartedThreads = moviesToLoad.Count;

            Logger.Log("Found movies: " + StartedThreads);

            foreach (var movie in moviesToLoad)
            {
                ThreadPool.QueueUserWorkItem(LoadMovie, movie);
            }

            DoneEvent.WaitOne();
            Logger.Log("Ending Headweb crawler");
        }

        public void LoadMovie(object obj)
        {
            try
            {
                var repository = new FilmsterRepository();
                var movieUrl = (string)obj;

                var doc = GetDocument(movieUrl).DocumentNode;

                bool highDef = false;
                const int vendorId = 4;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//h1[@class='title']").InnerText.Trim();
                var plot = doc.SelectSingleNode("//div[contains(@class, 'plot')]").InnerText.Trim();
                var coverUrl = doc.SelectSingleNode("//div[@class='bigcover']/img").Attributes["src"].Value;
                highDef = (doc.SelectSingleNode("//div[@class='bigcover']/div[contains(@class, 'hd')]") != null);

                int.TryParse(doc.SelectNodes("//div[(@class='movie_info lineSpacingNormal')]/span")[1].InnerText.RemoveNonNumericChars(), out releaseYear);

                if (doc.InnerHtml.Contains("LEJ"))
                {
                    float.TryParse(
                        doc.InnerHtml.SubstringByStringToString("'Btn_RentMovie':'LEJ", " kr", false).RemoveNonNumericChars(),
                        out price);
                }
                else
                {
                    Logger.Log("Unparsable price, skipping movie");
                    throw new Exception("Unparsable price");
                }
                
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