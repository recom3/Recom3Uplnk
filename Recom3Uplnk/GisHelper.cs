using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Recom3Uplnk
{
    public class GisHelper
    {
        //https://stackoverflow.com/questions/639695/how-to-convert-latitude-or-longitude-to-meters
        //https://en.wikipedia.org/wiki/Haversine_formula
        //Haversine_formula
        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {  // generally used geo measurement function
            var R = 6378.137; // Radius of earth in KM
            var dLat = lat2 * Math.PI / 180 - lat1 * Math.PI / 180;
            var dLon = lon2 * Math.PI / 180 - lon1 * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //c = 2 * Math.Asin(Math.Sqrt(a));
            var d = R * c;
            return d * 1000; // meters
        }

        //Reverse coor
        //https://nominatim.openstreetmap.org/reverse?format=xml&lat=37.073020&lon=-3.387628&zoom=18&addressdetails=1
        public static string reverseCoor(double dbLat, double dbLon)
        {
            //string sLat = dbLat.ToString("")
            string url = string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"),
                "https://nominatim.openstreetmap.org/reverse?format=xml&lat={0:N6}&lon={1:N6}&zoom=18&addressdetails=1&accept-language=en",
                dbLat, dbLon);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            CookieContainer cookies = new CookieContainer();
            httpWebRequest.Method = "GET";
            httpWebRequest.CookieContainer = cookies;

            httpWebRequest.UserAgent = @"recom3uplnk";
            httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var xmlData = streamReader.ReadToEnd();
                    XDocument doc = XDocument.Parse(xmlData); //or XDocument.Load(path)
                    string jsonText = JsonConvert.SerializeXNode(doc);
                    dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

                    //dynamic location = JsonConvert.DeserializeObject(result);

                    string country = dyn.reversegeocode.addressparts.country;

                    return country;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
