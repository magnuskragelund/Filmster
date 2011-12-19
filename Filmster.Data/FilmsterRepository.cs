using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Filmster.Data
{
    public class FilmsterRepository : IFilmsterRepository
    {
        private FilmsterEntities _context;
        private string _luceneIndexPath = ConfigurationManager.AppSettings["LuceneIndexPath"];

        public FilmsterRepository()
        {
            _context = new FilmsterEntities();
        }

        public Movie FindMovie(string title, DateTime? releaseDate)
        {
            var movies = GetActiveMovies().Where(m => m.Title == title);

            if(!movies.Any())
            {
                if(title.StartsWith("The ", StringComparison.InvariantCultureIgnoreCase))
                {
                    movies = _context.Movies.Where(m => m.Title == title.Replace("The ", ""));
                }
                else
                {
                    movies = _context.Movies.Where(m => m.Title == "The " + title);
                }
            }

            if(releaseDate != null)
            {
                movies = movies.Where(m => m.ReleaseDate == releaseDate || m.ReleaseDate == null);
            }

            return movies.FirstOrDefault();
        }

        public Movie GetMovie(int id)
        {
            return _context.Movies.Where(m => m.Id == id).SingleOrDefault();
        }

        /// <summary>
        /// Get all movies, active an inactive
        /// </summary>
        /// <returns></returns>
        public List<Movie> GetAllMovies()
        {
            return _context.Movies.ToList();
        }

        /// <summary>
        /// Gets movies that have been deemed active, as IQueryable
        /// </summary>
        /// <returns></returns>
        public IQueryable<Movie> GetActiveMovies()
        {
            var blockingDate = DateTime.Now.AddDays(-40);
            return _context.Movies
                .Where(m => !m.Porn)
                .Where(m => m.RentalOptions.Where(r => r.LastSeen > blockingDate).Count() > 0);
        }

        public List<Movie> GetPopularMovies(int take)
        {
            return GetActiveMovies().OrderByDescending(m => m.Impressions + m.RentalOptions.Sum(r => r.Impressions)).Take(take).ToList();
        }

        public List<Movie> GetMoviesByTitleFistChar(string firstChar)
        {
            return GetActiveMovies()
                .Where(m => m.Title.Substring(0, 1) == firstChar)
                .OrderBy(m => m.Title)
                .ToList();
        }

        public void AddMovie(Movie movie)
        {
            _context.Movies.AddObject(movie);
        }

        public void AddVendor(Vendor vendor)
        {
            _context.Vendors.AddObject(vendor);
        }

        public Vendor GetVendor(int id)
        {
            return _context.Vendors.Where(v => v.Id == id).SingleOrDefault();
        }

        public RentalOption GetRentalOption(int rentalOptionId)
        {
            return _context.RentalOptions.Where(r => r.Id == rentalOptionId).FirstOrDefault();
        }

        public RentalOption GetRentalOption(int movieId, int vendorId, bool highDefinition)
        {
            return _context.RentalOptions
                .Where(r => r.Movie.Id == movieId && r.Vendor.Id == vendorId && r.HighDefinition == highDefinition)
                .SingleOrDefault();
        }

        public void AddRentalOption(RentalOption rentalOption)
        {
            _context.RentalOptions.AddObject(rentalOption);
        }

        public void AddImpression(Movie movie)
        {
            throw new NotImplementedException();
        }

        public void AddImpression(RentalOption rentalOption)
        {
            throw new NotImplementedException();
        }

        public List<Movie> Query(string q, bool titleOnly = false)
        {
            if(titleOnly)
            {
                return GetActiveMovies()
                    .Where(m => m.Title.Contains(q) && m.RentalOptions.Count > 0)
                    .Take(50).ToList();
            }
            
            var fields = new List<string> {"title", "plot"};
             
            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(_luceneIndexPath));
            Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_29);

            MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_29, fields.ToArray(), analyzer);
            
            parser.SetAllowLeadingWildcard(true);
            
            Query query = parser.Parse(q);
            IndexSearcher searcher = new IndexSearcher(directory);
            Hits hits = searcher.Search(query);

            var movies = new List<Movie>();

            var i = 0;
            while (movies.Count < 50 && i < hits.Length())
            {
                var document = hits.Doc(i);
                var movie = GetMovie(int.Parse(document.Get("id")));
                if(movie != null && movie.RentalOptions != null && movie.RentalOptions.Count > 0)
                {    
                    movies.Add(movie);
                }
                i++;
            }

            return movies;
        }

        public void Save(bool dispose = true)
        {
            _context.SaveChanges();
            if(dispose) _context.Dispose();
        }

        public void Undo()
        {
            _context.Dispose();
        }
    }
}
