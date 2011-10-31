using System;
using System.Collections.Generic;
using System.Threading;
using Filmster.Common;
using Filmster.Crawler;
using Filmster.Data;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class VoddlerCrawler : Crawler
    {
        private const string _crawlstart = "http://www.voddler.com/en/movie/browse/premium/alphabetical/all/online/{0}/24";

        public void Start()
        {
            var page = 0;
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            while (resultContainsMovies)
            //while (page == 0)
            {
                Logger.LogVerbose("Fetching Voddler page " + page + ": " + string.Format(_crawlstart, page * 24));
                var doc = GetDocument(string.Format(_crawlstart, page * 24));
                page++;


                HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//a[@class='title']");

                foreach (HtmlNode htmlNode in list)
                {
                    moviesToLoad.Add("http://www.voddler.com" + htmlNode.Attributes["href"].Value);                        
                }

                if (doc.DocumentNode.SelectSingleNode("//a[@class='goright inactive']") != null)
                {
                    resultContainsMovies = false;
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
                var movieUrl = (string)obj;

                var doc = GetDocument(movieUrl).DocumentNode;

                bool highDef = false;
                const int vendorId = 3;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//div[@id='screencycle_movieinfo']//h2").InnerText.Trim();
                var plot = doc.SelectSingleNode("//p[@id='plot']").InnerText.Trim();
                var coverUrl = doc.SelectSingleNode("//div[@id='moviecovercontainer']//img").Attributes["src"].Value;

                if (title.Contains(" (HD)"))
                {
                    highDef = true;
                    title = title.Replace(" (HD)", "");
                }

                int.TryParse(doc.InnerHtml.SubstringByStringToString("\"productionYear\":", ",", false).RemoveNonNumericChars(), out releaseYear);
                float.TryParse(
                    doc.SelectSingleNode("//div[@id='buttoncontainer']//span[@class='price']").InnerText.RemoveNonNumericChars(),
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