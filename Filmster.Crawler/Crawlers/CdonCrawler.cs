using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Filmster.Common;
using Filmster.Crawler;
using Filmster.Data;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class CdonCrawler : Crawler
    {
        private string _crawlstart =
            "http://downloads.cdon.com/index.phtml?action=nav_page&navigation_id=24704&sortby=titleSort_DK_asc&category=movie&gridview=0&showall=1&page={0}&pagesize=100&country=dk";

        public CdonCrawler()
        {
            DocumentEncoding = Encoding.GetEncoding("ISO-8859-1");
        }

        public void Start()
        {
            var page = 0;
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            Logger.Log("Starting CDON Crawler");

            while (resultContainsMovies)
                //while (page == 0)
            {
                page++;

                var doc = GetDocument(string.Format(_crawlstart, page));

                HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//table[@class='product-list']//td[@class='title']//a[contains(@style, 'float:left')]");

                if (list == null || list.Count < 1)
                {
                    resultContainsMovies = false;
                    break;
                }

                foreach (HtmlNode htmlNode in list)
                {
                    moviesToLoad.Add("http://downloads.cdon.com" + htmlNode.Attributes["href"].Value);
                }
            }

            StartedThreads = moviesToLoad.Count;

            Logger.Log("Found movies: " + StartedThreads);

            foreach (var movie in moviesToLoad)
            {
                ThreadPool.QueueUserWorkItem(LoadMovie, movie);
            }

            DoneEvent.WaitOne();
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
                var plot = string.Empty;
                try
                {
                     plot = doc.SelectSingleNode("//div[@id='product_info_full']").InnerText.Trim().Replace("« Skjul teksten", "");
                }
                catch (Exception)
                {
                    plot = doc.SelectSingleNode("//div[@class='description-container']").InnerText.Trim();
                }
                
                var coverUrl = doc.SelectSingleNode("//a[@id='big_product_img2']").Attributes["href"].Value;

                if (title.Contains(" (HD)"))
                {
                    highDef = true;
                    title = title.Replace(" (HD)", "");
                }

                int.TryParse(doc.InnerText.TrySubstringByStringToString("Indspilnings&aring;r:", "&nbsp;", false).RemoveNonNumericChars(), out releaseYear);
                float.TryParse(
                        doc.SelectSingleNode("//div[@class='price']").InnerText.RemoveNonNumericChars(), NumberStyles.Any, new CultureInfo("en-US").NumberFormat,
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