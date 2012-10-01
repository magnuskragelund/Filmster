using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Filmster.Data
{
    public static class MovieExtensions
    {
        public static IEnumerable<RentalOption> ActiveRentalOptions(this Movie movie)
        {
            var blockingDate = DateTime.Now.AddDays(0 - FilmsterRepository.RENTAL_OPTION_VALIDATION_PERIOD_IN_DAYS);
            return movie.RentalOptions.Where(r => r.LastSeen > blockingDate);
        }
    }
}
