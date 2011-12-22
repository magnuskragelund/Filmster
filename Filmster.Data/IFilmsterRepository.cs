using System;
using System.Collections.Generic;
using System.Linq;

namespace Filmster.Data
{
    public interface IFilmsterRepository
    {
        Movie FindMovie(string title, DateTime? releaseDate);
        Movie GetMovie(int id);
        List<Movie> GetAllMovies();
        IQueryable<Movie> GetActiveMovies();
        List<Movie> GetPopularMovies(int take);
        //List<Movie> GetLatestMovies(int take);
        List<Movie> GetMoviesByTitleFistChar(string firstChar);
        void AddMovie(Movie movie);
        void AddVendor(Vendor vendor);
        Vendor GetVendor(int id);
        RentalOption GetRentalOption(int rentalOptionId);
        RentalOption GetRentalOption(int movieId, int vendorId, bool highDefinition);
        void AddRentalOption(RentalOption rentalOption);
        void AddImpression(Movie movie);
        void AddImpression(RentalOption rentalOption);
        List<Movie> Query(string query, bool titleOnly = false);
        void Save(bool dispose = true);
        void Undo();
    }
}