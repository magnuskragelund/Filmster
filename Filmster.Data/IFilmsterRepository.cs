using System;
using System.Collections.Generic;

namespace Filmster.Data
{
    public interface IFilmsterRepository
    {
        Movie GetMovie(string title, DateTime? releaseDate);
        Movie GetMovie(int id);
        List<Movie> GetMoviesByTitleFistChar(string firstChar);
        void AddMovie(Movie movie);
        void AddVendor(Vendor vendor);
        Vendor GetVendor(int id);
        RentalOption GetRentalOption(int movieId, int vendorId, bool highDefinition);
        void AddRentalOption(RentalOption rentalOption);
        List<Movie> Query(string query, bool titleOnly = false);
        void Save();
        void Undo();
    }
}