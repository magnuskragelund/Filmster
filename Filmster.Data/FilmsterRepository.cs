using System;
using System.Collections.Generic;
using System.Linq;

namespace Filmster.Data
{
    public class FilmsterRepository : IFilmsterRepository
    {
        private FilmsterMovies _context;

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

        public List<Movie> Query(string query)
        {
            return _context.Movies
                .Where(m => m.Title.Contains(query))
                .Take(20).ToList();
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
