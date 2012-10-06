using System;
using System.Collections.Generic;
using System.Threading;
using Filmster.Crawler;
using Filmster.Data;
using Filmster.Utilities;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace Filmster.Crawlers
{
    internal class ViaPlayCrawler : Crawlers.Crawler, ICrawler
    {
        private string _crawlstart =
            "http://viaplay.dk/film/alle/250/alphabetical/4229412858";

        public void Start()
        {
            var doc = GetDocument(_crawlstart);

            HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//ul[@class='atoz-list']//a[contains(@href, '/film/')]");

            Parallel.ForEach(list, htmlNode =>
            {
                LoadMovie("http://viaplay.dk" + htmlNode.Attributes["href"].Value);
            });
        }

        public void LoadMovie(string url)
        {
            try
            {
                var repository = new FilmsterRepository();
                
                var doc = GetDocument(url).DocumentNode;

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

                int.TryParse(doc.SelectSingleNode("//li[contains(@class,'year')]").InnerText.RemoveNonNumericChars(), out releaseYear);
             
                var id = doc.SelectSingleNode("//div[@id='productSelect']").Attributes["data-product-id"].Value;
                var overlay = GetDocument("http://viaplay.dk/ahah/overlay?isBoxPackage=false&channel9=false&productId=" + id).DocumentNode;
                var rent = overlay.SelectSingleNode("//div[@data-content='movies']//p[@class='alternative']/a");
                var subscription = overlay.SelectSingleNode("//div[@data-content='movies']//span[@class='price']");
                var subscriptionBased = false;
                
                if(rent.InnerText.Contains("lej denne film"))
                {
                    // for rent
                    url = "http://viaplay.dk" + rent.Attributes["href"].Value;
                    float.TryParse(
                        rent.InnerHtml.RemoveNonNumericChars(),
                        out price);
                }
                else
                {
                    // subscriptionbased
                    price = 99;
                    subscriptionBased = true;
                }

                ResolveRentalOption(repository, url, coverUrl, vendorId, title, plot, releaseYear, false, highDef, price, subscriptionBased);
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