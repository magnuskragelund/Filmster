using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Filmster.Data
{
    public class Movie
    {
        public int Id { get; set;}
        public string Title { get; set; }
        public string Plot { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public bool Porn { get; set; }
        public virtual ICollection<RentalOption> RentalOptions { get; set; }
    }

    public class Vendor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class RentalOption
    {
        public int Id { get; set; }
        public float Price { get; set; }
        public bool HighDefinition { get; set; }
        public string Url { get; set; }
        public string CoverUrl { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastSeen { get; set; }
        public virtual Movie Movie { get; set; }
        public virtual Vendor Vendor { get; set; }
    }

    public class FilmsterMovies : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<RentalOption> RentalOptions { get; set; }
    }

    public class FilmsterInitializer : DropCreateDatabaseIfModelChanges<FilmsterMovies>
    {
        protected override void Seed(FilmsterMovies context)
        {
            IFilmsterRepository repository = new FilmsterRepository();
            List<Vendor> vendors = new List<Vendor>()
                                       {
                                           new Vendor() { Id = 1, Name = "Viaplay", Url = "http://viaplay.dk"},
                                           new Vendor() { Id = 2, Name = "TV2 Sputnik", Url = "http://sputnik.tv2.dk"},
                                           new Vendor() { Id = 3, Name = "Voddler", Url = "http://voddler.com"},
                                           new Vendor() { Id = 4, Name = "Headweb", Url = "http://headweb.com/da"},
                                           new Vendor() { Id = 5, Name = "SF-Anytime", Url = "http://sfanytime.com"},
                                           new Vendor() { Id = 6, Name = "YouSee TV", Url = "http://yousee.tv"},
                                           new Vendor() { Id = 7, Name = "Apple iTunes", Url = "http://itunes.com"},
                                       };

            foreach (var vendor in vendors)
            {
                repository.AddVendor(vendor);
            }

            repository.Save();
        }
    }
}
