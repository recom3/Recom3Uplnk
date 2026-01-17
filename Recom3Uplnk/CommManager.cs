using Newtonsoft.Json;
using Recom3Uplnk.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk
{
    class CommManager
    {
        private const string URL_CERT = "http://localhost:51192/api/";
        private const string URL_PRO = "https://www.recom3.com/api/";

        public const string URL = URL_PRO;

        public const string URL_BASE = "https://www.recom3.com/";

        public String doPost(String userName, String password, ref UserData userData)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL + "login");

            String query = String.Format("email={0}&password={1}", userName, password);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"email\":\"" + userName + "\"," +
                              "\"password\":\"" + password + "\"}";

                //streamWriter.Write(json);
                streamWriter.Write(query);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    userData = JsonConvert.DeserializeObject<UserData>(result);
                    //return result;
                    return userData.access_token;
                }
            }
            catch(Exception ex)
            {
                return "";
            }
        }

        public UserData Me(String secret)
        {
            UserData userData = null;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.recom3.com/api/me");

            //httpWebRequest.Method = "POST";

            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + secret);
            httpWebRequest.Accept = "application/json";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    userData = JsonConvert.DeserializeObject<UserData>(result);
                    return userData;
                }
            }
            catch (Exception ex)
            {
                return userData;
            }
        }

        public UnitClass MeUnits(String secret)
        {
            UnitClass userData = null;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.recom3.com/api/reconnunitss");

            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + secret);
            httpWebRequest.Accept = "application/json";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    userData = JsonConvert.DeserializeObject<UnitClass>(result);
                    return userData;
                }
            }
            catch (Exception ex)
            {
                return userData;
            }
        }

    }
}
