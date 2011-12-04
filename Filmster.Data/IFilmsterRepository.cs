using System;
using System.Collections.Generic;

namespace Filmster.Data
{
    public interface IFilmsterRepository
    {
        Movie FindMovie(string title, DateTime? releaseDate);
        Movie GetMovie(int id);
        List<Movie> GetMovies();
        List<Movie> GetActiveMovies();
        List<Movie> GetRandomMovies(int take, int minRentalOptionCount);
        List<Movie> GetPopularMovies(int take);
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