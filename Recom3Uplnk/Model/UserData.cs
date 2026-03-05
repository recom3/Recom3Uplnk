using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk.Model
{
    public class UserProfileClass
    {
        public string id { get; set; }
        public bool auto_upload_trips { get; set; }
        public string phone_number { get; set; }
        public string gender { get; set; }
        public string city { get; set; }
        public string country_id { get; set; }
        public string bio { get; set; }
    }

    public class UserClass
    {
        public string id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string facebook_id { get; set; }
        public string mobile_active { get; set; }
        public string last_login { get; set; }
        public string measurement { get; set; }
        public bool buddy_tracking_enabled { get; set; }
        public bool buddy_tracking_stealth_mode_enabled { get; set; }
    }

    public class UserData
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public long expires { get; set; }
        public string refresh_token { get; set; }
        public UserClass User { get; set; }

        public UserProfileClass UserProfile { get; set; }
    }
}
