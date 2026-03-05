using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk.Model
{
    public class SofwareVersionClass
    {
        public string recon_product_id { get; set; }
        public string display_name { get; set; }
        public string svn { get; set; }
    }

    public class UnitClass
    {
        //public uint serial_number { get; set; }
        public string serial_number { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public SofwareVersionClass software_version { get; set; }
    }

    public class TripId
    {
        public string id_trip { get; set; }
    }
}
