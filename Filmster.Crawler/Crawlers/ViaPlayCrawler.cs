using System;
using System.Collections.Generic;
using System.Threading;
using Filmster.Common;
using Filmster.Crawler;
using Filmster.Data;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class ViaPlayCrawler : Crawlers.Crawler
    {
        private string _crawlstart =
            "http://viaplay.dk/Handlers/SelectorSearchHandler.ashx?searchString=%26f1%3DUserDef18%26d1%3DPC%26o1%3Dctn%26f2%3DUserDef14%26d2%3D%26o2%3Dctn%26co2%3DAND&searchAtoZString=&sortBy=AtoZ&sortOrder=asc&pageSize=100&currentPage={0}&ViewType=None&_ViewType=&categoryIds=&category=movies&filter=rental&RowId=1&Genre=&GenreName=&movieCategoryId=0&isTelevision=false&ClientID=ctl00_cpBody_SearchResults&ShowNewArrivalsIfEmpty=false&Newtitlescategories=";

        public void Start()
        {
            var page = 1;
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            while (resultContainsMovies)
                //while (page == 1)
            {
                Logger.LogVerbose("Fetching page " + page);
                page++;

                var doc = GetDocument(string.Format(_crawlstart, page));

                HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//a[contains(@href, '/Movies/')]");

                if (list == null)
                {
                    resultContainsMovies = false;
                    break;
                }

                foreach (HtmlNode htmlNode in list)
                {
                    moviesToLoad.Add("http://viaplay.dk" + htmlNode.Attributes["href"].Value);
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
                const int vendorId = 1;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//h1[@class='tit-h1']").InnerText.Trim();
                var plot = doc.SelectSingleNode("//div[@id='plProduct']//p[@class='info-vid']").InnerText.Trim();
                var coverUrl = doc.SelectSingleNode("//div[@id='plProduct']//img").Attributes["src"].Value;

                if (title.Contains(" (HD)"))
                {
                    highDef = true;
                    title = title.Replace(" (HD)", "");
                }

                int.TryParse(doc.SelectNodes("//div[(@id='calification')]//div[@class='row cali']")[2].InnerText.RemoveNonNumericChars(), out releaseYear);
                float.TryParse(
                    doc.InnerHtml.SubstringByStringToString("&price=", "%2c", false),
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