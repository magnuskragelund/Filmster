using System;
using System.Collections.Generic;
using System.Threading;
using Filmster.Crawler;
using Filmster.Data;
using Filmster.Utilities;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class SFAnytimeCrawler : Crawlers.Crawler
    {
        private string _crawlstart = "http://sfanytime.com/Templates/Storefront/Pages/MovieListProxy.aspx?nc=da-DK-0&epslanguage=da-DK&stripes=1,2,3&rtr={0}&sr={1}&ob=&mob=Year+desc";

        public void Start()
        {
            var movieCount = 100;
            var moviesToLoad = new List<string>();
            var lastPage = false;
            var skip = 0;

            while (!lastPage)
            {
                var doc = GetDocument(string.Format(_crawlstart, movieCount, skip));

                HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//div[@class='backet_title']/a");

                foreach (HtmlNode htmlNode in list)
                {
                    var url = "http://sfanytime.com" + htmlNode.Attributes["href"].Value;
                    if (!moviesToLoad.Contains(url))
                    {
                        var movie = "http://sfanytime.com" + htmlNode.Attributes["href"].Value;
                        moviesToLoad.Add(movie);
                    }
                }

                skip += movieCount;

                lastPage = list.Count != movieCount;
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
                var movieUrl = ((string)obj);
                var doc = GetDocument(movieUrl).DocumentNode;

                bool highDef = false;
                const int vendorId = 5;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//div[@id='breadcrumbs']/a[last()]").InnerText.Trim();
                var plot = doc.SelectSingleNode("//div[@class='col50 first']/div[@class='text']").InnerText.Trim();
                var porn = doc.SelectNodes("//div[@class='moviedetails']")[0].InnerText.Contains("Voksen");
                var coverUrl = doc.SelectSingleNode("//a[@class='plus lightbox']").Attributes["href"].Value;


                if (title.Contains("- HD"))
                {
                    highDef = true;
                    title = title.Replace("- HD", "").Trim();
                }

                int.TryParse(doc.SelectSingleNode("//div[@class='moviedetails']").InnerText.TrySubstringByStringToString("Udgivelsesår", "Fra", false), out releaseYear);
                float.TryParse(doc.SelectSingleNode("//div[@class='bottom_hyr']").InnerText.RemoveNonNumericChars(), out price);

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