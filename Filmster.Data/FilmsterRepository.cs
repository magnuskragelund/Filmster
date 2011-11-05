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
        private FilmsterMovies _context;
        private string _luceneIndexPath = ConfigurationManager.AppSettings["LuceneIndexPath"];

        public FilmsterRepository()
        {
            _context = new FilmsterMovies();
        }

        public Movie GetMovie(string title, DateTime? releaseDate)
        {
            var movies = _context.Movies.Where(m => m.Title == title);

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

        public List<Movie> GetMoviesByTitleFistChar(string firstChar)
        {
            return _context.Movies
                .Where(m => m.Title.Substring(0, 1) == firstChar)
                .OrderBy(m => m.Title)
                .ToList();
        }

        public void AddMovie(Movie movie)
        {
            _context.Movies.Add(movie);
        }

        public void AddVendor(Vendor vendor)
        {
            _context.Vendors.Add(vendor);
        }

        public Vendor GetVendor(int id)
        {
            return _context.Vendors.Where(v => v.Id == id).SingleOrDefault();
        }

        public RentalOption GetRentalOption(int movieId, int vendorId, bool highDefinition)
        {
            return _context.RentalOptions
                .Where(r => r.Movie.Id == movieId && r.Vendor.Id == vendorId && r.HighDefinition == highDefinition)
                .SingleOrDefault();
        }

        public void AddRentalOption(RentalOption rentalOption)
        {
            _context.RentalOptions.Add(rentalOption);
        }

        public List<Movie> Query(string q)
        {
            Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(_luceneIndexPath));
            Analyzer analyzer = new StandardAnalyzer(Version.LUCENE_29);

            MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_29, new[] { "plot", "title" }, analyzer);
            Query query = parser.Parse(q);
            IndexSearcher searcher = new IndexSearcher(directory);
            Hits hits = searcher.Search(query);

            var movies = new List<Movie>();

            for(var i = 0; i < hits.Length(); i++)
            {
                var document = hits.Doc(i);
                movies.Add(GetMovie(int.Parse(document.Get("id"))));
            }

            return movies;
            //return _context.Movies
            //    .Where(m => m.Title.Contains(query))
            //    .Take(20).ToList();
        }

        public void Save()
        {
            _context.SaveChanges();
            _context.Dispose();
        }

        public void Undo()
        {
            _context.Dispose();
        }
    }
}
