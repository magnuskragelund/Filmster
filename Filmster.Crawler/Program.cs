using System;
using System.Data.Entity;
using System.Threading;
using Filmster.Crawlers;
using Filmster.Data;

namespace Filmster.Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPool.SetMaxThreads(40, 40);
            Database.SetInitializer(new FilmsterInitializer());

            new ItunesCrawler().Start();
            //new SputnikCrawler().Start();
            //new YouSeeCrawler().Start();
            //new HeadwebCrawler().Start();
            //new ViaPlayCrawler().Start();
            //new SFAnytimeCrawler().Start();
            //new VoddlerCrawler().Start();
            //new FilmstribenCrawler().Start();
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
