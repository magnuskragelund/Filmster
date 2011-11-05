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
            "http://beta.viaplay.dk/film/alle/250/alphabetical";

        public void Start()
        {
            var moviesToLoad = new List<string>();

            var doc = GetDocument(_crawlstart);

            HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//ul[@class='atoz-list']//a[contains(@href, '/film/')]");


            foreach (HtmlNode htmlNode in list)
            {
                moviesToLoad.Add("http://viaplay.dk" + htmlNode.Attributes["href"].Value);
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

                var title = doc.SelectSingleNode("//h1").InnerText.Trim();
                var plot = doc.SelectSingleNode("//div[@class='article-content']//p").InnerText.Trim();
                var coverUrl = doc.SelectNodes("//div[@id='productSelect']//img")[0].Attributes["src"].Value;

                if (title.Contains(" (HD)"))
                {
                    highDef = true;
                    title = title.Replace(" (HD)", "");
                }

                int.TryParse(doc.SelectSingleNode("//h2[(@class='genre')]").InnerText.RemoveNonNumericChars(), out releaseYear);
                //float.TryParse(
                //    doc.InnerHtml.SubstringByStringToString("&price=", "%2c", false),
                //    out price);
                
                var id = doc.SelectSingleNode("//div[@id='productSelect']").Attributes["data-product-id"].Value;
                var overlay = GetDocument("http://beta.viaplay.dk/ahah/overlay?productId=" + id).DocumentNode;
                var rent = overlay.SelectSingleNode("//div[@data-content='movies']//p[@class='alternative']/a");
                var subscription = overlay.SelectSingleNode("//div[@data-content='movies']//span[@class='price']");
                var subscriptionBased = false;
                
                if(rent.InnerText.Contains("lej denne film"))
                {
                    // for rent
                    movieUrl = "http://beta.viaplay.dk" + rent.Attributes["href"].Value;
                    float.TryParse(
                        rent.InnerHtml.RemoveNonNumericChars(),
                        out price);
                }
                else
                {
                    // subscriptionbased
                    float.TryParse(
                        subscription.InnerHtml.RemoveNonNumericChars(),
                        out price);
                    subscriptionBased = true;
                }



                ResolveRentalOption(repository, movieUrl, coverUrl, vendorId, title, plot, releaseYear, false, highDef, price, subscriptionBased);
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