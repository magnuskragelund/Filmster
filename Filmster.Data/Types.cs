using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Filmster.Data
{
    public class Status
    {
        public string Name { get; set; }
        public DateTime LatestRentalOption { get; set; }
        public int TotalActiveRentalOptions { get; set; }
        public int TotalRentalOptions { get; set; }
    }
}
