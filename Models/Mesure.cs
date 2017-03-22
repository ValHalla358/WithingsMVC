using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WithingsTest.Models
{
    public class Mesure
    {
        public int userid { get; set; }
        public DateTime date { get; set; }
        public DateTime startdateymd { get; set; }
        public DateTime enddateymd { get; set; }
        public string timezone { get; set; }
        public int steps { get; set; }
        public int distance { get; set; }
        public int calories { get; set; }
        public int totalcalories { get; set; }
        public int elevation { get; set; }
        public int soft { get; set; }
        public int moderate { get; set; }
        public int intense { get; set; }
        public string status { get; set; }
        public Dictionary<string,string>  body { get; set; }
    }
}