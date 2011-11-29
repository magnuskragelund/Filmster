using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Threading;
using Filmster.Crawlers;
using Filmster.Data;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = System.IO.Directory;

namespace Filmster.Crawler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if(args.Length > 0 && args[0] == "-index")
            {
                Index();
            }
            else
            {
                Crawl();
                Index();
            }
        }

        public static void Crawl()
        {
            Logger.Log("Initiating Crawl");
            ThreadPool.SetMinThreads(40, 40);
            ThreadPool.SetMaxThreads(120, 120);
            
            new Film2HomeCrawler().Start();
            new CdonCrawler().Start();
            new ItunesCrawler().Start();
            new VoddlerCrawler().Start();
            new HeadwebCrawler().Start();
            new ViaPlayCrawler().Start();
            new SFAnytimeCrawler().Start();
            new YouSeeCrawler().Start();
            new SputnikCrawler().Start();
            //new FilmstribenCrawler().Start();
        }

        public static void Index()
        { 
            try
            {

                Logger.Log("Initiating Index");
                var indexPath = ConfigurationManager.AppSettings["LuceneIndex"];
                var directory = FSDirectory.Open(new DirectoryInfo(indexPath));

                Logger.Log("Deleting index");
                string[] filePaths = Directory.GetFiles(indexPath);
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }

                var repository = new FilmsterRepository();
                Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);

                Logger.Log("Creating new index");
                var i = 0;
                IndexWriter writer = new IndexWriter(directory, analyzer);
                var moviesToIndex = repository.GetActiveMovies();
                foreach (var movie in moviesToIndex)
                {
                    Document doc = new Document();
                    doc.Add(new Field("id", movie.Id.ToString(), Field.Store.YES, Field.Index.NO));
                    doc.Add(new Field("title", movie.Title, Field.Store.YES, Field.Index.ANALYZED));
                    doc.Add(new Field("plot", movie.Plot, Field.Store.YES, Field.Index.ANALYZED));
                    foreach(var rental in movie.RentalOptions)
                    {
                        doc.Add(new Field("vendor", rental.Vendor.Name, Field.Store.YES, Field.Index.ANALYZED));
                    }
                    writer.AddDocument(doc);
                    i++;
                }
                writer.Close();
                Logger.Log(string.Format("Indexed {0} movies", i));
            }
            catch (Exception exp)
            {
                Logger.LogException("Indexing error", exp);
            }
        }
    }

    public class Logger
    {
        public static void Log(string str)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        public static void LogVerbose(string str)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        public static void LogException(string str, Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.WriteLine(exception);
            Console.ResetColor();
        }
    }

}
