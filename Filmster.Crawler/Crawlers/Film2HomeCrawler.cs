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
    internal class Film2HomeCrawler : Crawler
    {
        private const string _crawlstart = "http://www.film2home.dk/dk/Find-din-film.aspx?sort=9&p={0}";
        private CultureInfo culture = new CultureInfo("da-DK");

        public void Start()
        {
            var page = 1;
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            while (resultContainsMovies)
            //while (page == 1)
            {
                Logger.LogVerbose("Fetching Film2Home page " + page);
                var doc = GetDocument(string.Format(_crawlstart, page));
                page++;

                HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//div[@class='hoverThis']");

                if(list != null && list.Count > 0)
                {
                    foreach (HtmlNode htmlNode in list)
                    {
                        if (htmlNode.SelectSingleNode("a") != null && htmlNode.SelectSingleNode("//span[@class='rent']") != null)
                        {
                            moviesToLoad.Add("http://www.film2home.dk" + htmlNode.SelectSingleNode("a").Attributes["href"].Value);
                        }

                    }
                }
                else
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
                const int vendorId = 9;
                int releaseYear = 0;
                float price = 0;

                var titleNode = doc.SelectSingleNode("//h1");
                var title = culture.TextInfo.ToTitleCase(titleNode.InnerText.Trim());
                var plot = doc.SelectSingleNode("//div[@id='content-columns']/div[@class='column-double first-column']/p").InnerText.Trim();
                var coverUrl = "http://www.film2home.dk" + doc.SelectSingleNode("//img[@class='packshot defaultimg']").Attributes["src"].Value;

                if (title.Contains(" (Hd)"))
                {
                    highDef = true;
                    title = title.Replace(" (Hd)", "");
                }

                int.TryParse(doc.SelectSingleNode("//h1/small").InnerText.RemoveNonNumericChars(), out releaseYear);
                releaseYear = releaseYear - 1;

                float.TryParse(doc.SelectSingleNode("//a[@href='#stream']/strong").InnerText.RemoveNonNumericChars(), out price);

                if (price == 0)
                {
                    throw new Exception("Price is zero, movie is purchase only");
                }

                ResolveRentalOption(repository, movieUrl, coverUrl, vendorId, title, plot, releaseYear, false, highDef, price);
                repository.Save();

                Logger.LogVerbose(title.Trim());
            }
            catch (Exception ex)
            {
                LogCrawlerError(ex);
            }
            finally
            {
                if (Interlocked.Decrement(ref StartedThreads) == 0)
                {
                    DoneEvent.Set();
                }
            }
        }
    }
}