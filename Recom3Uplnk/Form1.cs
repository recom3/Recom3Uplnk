using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using MediaDevices;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Recom3Uplnk.Maps;
using Recom3Uplnk.Model;

namespace Recom3Uplnk
{
    public partial class Form1 : Form
    {
        String secret;

        CommManager mCommManager;
        FileManager mFileManager;

        public Form1()
        {
            InitializeComponent();

            mCommManager = new CommManager();
            mFileManager = new FileManager();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbEmail.Text = mFileManager.GetEmailJson();

            //tbPassword.Text = "Rev1900p!";
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Text = "Uplink";
            this.Icon = Properties.Resources.ic_launcher;
        }

        private void ScanUsbDevices()
        {
            var usbDevices = UsbManager.GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                Console.WriteLine("Device ID: {0}, PNP Device ID: {1}, Description: {2}",
                    usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description);
            }
        }

        private void ToggleLoginVisibility(bool isLoginVisible)
        {
            lbEmail.Visible = isLoginVisible;
            tbEmail.Visible = isLoginVisible;
            lbPassword.Visible = isLoginVisible;
            tbPassword.Visible = isLoginVisible;
            btLogin.Visible = isLoginVisible;

            if(isLoginVisible)
            {
                pbGoggles.Visible = false;
                lbOwner.Visible = false;
                lbReconOS.Visible = false;
                lbSerial.Visible = false;
                lbMapsVer.Visible = false;

                //Sync buttons
                btRetry.Visible = false;
                btSync.Visible = false;
                //For debug maps
                btMap.Visible = true;
                //btMap.Visible = false;
            }
        }

        public void doBetaDownloadMaps(string password)
        {
            bool bMaps = false;
            if (bMaps)
            {
                mFileManager.DownloadMapFromServer("areas/6001.shp", password);
            }

            bool bZipMaps = false;
            if(bZipMaps)
            {
                //Input params
                string zipPath = @"C:\Users\xxxx\AppData\Local\Recon Instruments\mdmaps.zip";
                string[] files = new string[] {@"C:\Users\xxxx\AppData\Local\Recon Instruments\areas\6001.dbf"};

                using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
                {
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        zipArchive.CreateEntryFromFile(fileInfo.FullName, "areas/"+ fileInfo.Name);
                    }
                }
            }

            bool bCoor = false;
            if(bCoor)
            {
                GisHelper.reverseCoor(37.073020, -3.387628);
            }
        }

        public void doBetaCodeMaps(string password)
        {
            bool bMaps = false;
            if (bMaps)
            {
                List<String> entries = new List<string>();

                //await ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
                string path = @"D:\Downloads\Recon-Oakley\MAPS\temp";
                string dbpath = Path.Combine(path, "resortinfo.db");
                using (SqliteConnection db =
                   new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    SqliteCommand selectCommand = new SqliteCommand
                        ("SELECT * from resortLocation where name like 'Embarus'", db);

                    SqliteDataReader query = selectCommand.ExecuteReader();

                    double searchLat = 59.91;
                    double searchLon = 10.76;
                    double margin = 1.0d;

                    //Delete
                    if(query.Read())
                    {
                        SqliteCommand deleteCommand = new SqliteCommand
                            ("DELETE from resortLocation where name like 'Embarus'", db);

                        deleteCommand.ExecuteNonQuery();
                    }

                    if(!query.Read())
                    {
                        SqliteCommand insertCommand = new SqliteCommand();
                        insertCommand.Connection = db;

                        int id = 6000;
                        string name = "Embarus";
                        int resortType = 0;
                        int countryID = 164;
                        double hubLatitude = 35.000000;
                        double hubLongitude = 4.000000;

                        double hubAltitude = 0;

                        double minLatitude = 0;
                        double minLongitude = 0;
                        double maxLatitude = 0;
                        double maxLongitude = 0;

                        int active = 0;
                        int mapVersion = 3;

                        // Use parameterized query to prevent SQL injection attacks
                        insertCommand.CommandText = @"INSERT INTO resortLocation 
                            VALUES (@id, @name, @resortType, @countryID, @hubLatitude, @hubLongitude, 
                            @hubAltitude, @minLatitude, @minLongitude, @maxLatitude, @maxLongitude, @active, @mapVersion);";
                        insertCommand.Parameters.AddWithValue("@id", id);
                        insertCommand.Parameters.AddWithValue("@name", name);
                        insertCommand.Parameters.AddWithValue("@resortType", resortType);
                        insertCommand.Parameters.AddWithValue("@countryID", countryID);
                        insertCommand.Parameters.AddWithValue("@hubLatitude", hubLatitude);
                        insertCommand.Parameters.AddWithValue("@hubLongitude", hubLongitude);
                        insertCommand.Parameters.AddWithValue("@hubAltitude", hubAltitude);
                        insertCommand.Parameters.AddWithValue("@minLatitude", minLatitude);
                        insertCommand.Parameters.AddWithValue("@minLongitude", minLongitude);
                        insertCommand.Parameters.AddWithValue("@maxLatitude", maxLatitude);
                        insertCommand.Parameters.AddWithValue("@maxLongitude", maxLongitude);
                        insertCommand.Parameters.AddWithValue("@active", active);
                        insertCommand.Parameters.AddWithValue("@mapVersion", mapVersion);

                        insertCommand.ExecuteReader();
                    }

                    while (query.Read())
                    {
                        //string sdata = query.GetValue();
                        //int idata = query.GetOrdinal("minLatitude");

                        double minLat = double.Parse(query.GetValue(query.GetOrdinal("minLatitude")).ToString());
                        double maxLat = double.Parse(query.GetValue(query.GetOrdinal("maxLatitude")).ToString());
                        double hudLat = double.Parse(query.GetValue(query.GetOrdinal("hubLatitude")).ToString());

                        double minLon = double.Parse(query.GetValue(query.GetOrdinal("minLongitude")).ToString());
                        double maxLon = double.Parse(query.GetValue(query.GetOrdinal("maxLongitude")).ToString());
                        double hudLon = double.Parse(query.GetValue(query.GetOrdinal("hubLongitude")).ToString());

                        if ((Math.Abs(minLat - searchLat)<=margin || Math.Abs(maxLat - searchLat) <= margin
                            || Math.Abs(hudLat - searchLat) <= margin)
                            //&&
                            //(Math.Abs(minLon - searchLon) <= margin || Math.Abs(maxLon - searchLon) <= margin
                            //|| Math.Abs(hudLon - searchLon) <= margin)
                            )
                        {
                            Console.WriteLine("{0}", query.GetValue(query.GetOrdinal("name")));
                        }

                        entries.Add(query.GetString(0));
                    }
                }
            }
        }

        public void doBetaCode(string password)
        {
            //Test: never true in pro!
            bool isTestTrackCvt = false;
            if (isTestTrackCvt)
            {
                bool enabled1stTest = false;
                if (enabled1stTest)
                {
                    //PART1: Just to test conversion test code

                    //Format of these files?
                    //string inFile = @"D:\Downloads\Recon-Oakley\Backup my Oakley\ReconApps\TripData\DAY02.RIB";
                    //string inFile = @"D:\Downloads\Recon-Oakley\Backup my Oakley after update\ReconApps\TripData\DAY04.RIB";
                    //string inFile = @"C:\Users\xxxx\AppData\Local\Recon Instruments\DAY06.RIB";
                    //string inFile = @"D:\git\RecomProjects\rib-to-gpx-file-converter\example\DAY35.RIB";

                    //Format 1: most normal
                    //string inFile = @"C:\Users\xxxx\Downloads\download\DAY302.RIB";

                    //Format 3
                    //string inFile = @"C:\Users\xxxx\AppData\Local\Recon Instruments\DAY05.RIB";
                    //string inFile = @"C:\Users\xxxx\Documents\From jet white\DAY07.RIB";
                    string inFile = @"D:\PROYECTOS\Recom3\De_Andy_Brochocky\DAY112.RIB";

                    FlightConverter.TrackInfo trackInfo = new FlightConverter.TrackInfo();
                    List<Track> tracks = FlightConverter.parseFile(inFile, ref trackInfo);
                    string gpxName = Path.GetFileNameWithoutExtension(inFile) + ".gpx";
                    trackInfo.fileName = gpxName;
                    //Write converted file to disk
                    string gpxFileName = Environment.ExpandEnvironmentVariables(FileManager.pathBase + gpxName);
                    XMLOutput.writeFile2(gpxFileName, tracks);

                    //Asign country and name
                    string country = "";
                    string name = "";
                    try
                    {
                        if (tracks != null && tracks.Count > 0)
                        {
                            List<TrackPoint> lst = tracks[0].points;
                            if (lst != null && lst.Count > 0)
                            {
                                TrackPoint point = lst[0];
                                if (point != null)
                                {
                                    country = GisHelper.reverseCoor(point.lat, point.lon);
                                    //date("Y-m-d H:i")
                                    name = string.Format("Trip {0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}",
                                        tracks[0].year,
                                        tracks[0].month,
                                        tracks[0].day,
                                        point.hour, point.min);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        country = "USA";
                        name = string.Format("Trip yyyy-MM-dd HH:mm:ss",
                            DateTime.Now);
                    }
                    trackInfo.name = name;
                    trackInfo.country = country;
                    /*
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(URL);

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", password);

                    string payload = JsonConvert.SerializeObject(trackInfo);

                    var content = new StringContent(payload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = client.PostAsync(urlParameters, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        // by calling .Result you are synchronously reading the result
                        string responseString = responseContent.ReadAsStringAsync().Result;

                        TripId tripId = JsonConvert.DeserializeObject<TripId>(responseString);
                        //Console.WriteLine(responseString);

                        //Now we have the trip id so we can upload the file itself
                        pkgDevice.name = tripId.id_trip + "_" + gpxName;
                        pkgDevice.fileName = gpxFileName;
                    }

                    client.Dispose();
                    */
                }
                //PART2: Complete cycle to test upload to server
                //Test push trips to server

                List<FileManager.PackageInfoExt> lstPackageDevice = new List<FileManager.PackageInfoExt>();
                //File name will be overwritten
                string fileName = Environment.ExpandEnvironmentVariables(FileManager.pathBase + "DAY112.RIB");
                lstPackageDevice.Add(new FileManager.PackageInfoExt("DAY112.RIB", "", "", fileName, ""));

                //SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();
                //sdFiles.Add("DAY35.RIB", null);

                //After this all the trips should "come" with the ID prefix
                FileManager.DoPushAllTrips(lstPackageDevice, password);

                FileManager.DoPushAllTripBinToServer(lstPackageDevice, password);
            }
        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            //Test code
            bool isTestXml = false;
            if (isTestXml)
            {
                loadTilesFromXml();
                writeXMLTilesToDisk();
            }

            doBetaCode("eyJhbGciOiJxxxxxxxxxxxxxxxxxxxxx");
            //doBetaCodeMaps("password");
            doBetaDownloadMaps("password");

            if (tbEmail.Text.Trim().Length > 0 && tbPassword.Text.Trim().Length > 0)
            {
                UserData me = null;
                secret = mCommManager.doPost(tbEmail.Text, tbPassword.Text, ref me);

                if (secret != null && secret.Trim() != "")
                {
                    doBetaCode(secret);

                    //var me = mCommManager.Me(secret);
                    mCommManager.Me(secret);

                    if (me != null)
                    {
                        lbOwner.Text = String.Format("Owner: {0} {1}", me.User.first_name, me.User.last_name);
                        lbOwner.Visible = true;

                        var units = mCommManager.MeUnits(secret);

                        if (me != null)
                        {
                            lbReconOS.Text = String.Format("ReconOS: {0}", units.software_version.display_name);
                            lbReconOS.Visible = true;

                            ToggleLoginVisibility(false);

                            lbMsg.Text = "";

                            DoSynchro(tbEmail.Text);
                        }
                        else
                        {
                            lbMsg.Text = "Communication problem with the server";
                        }
                    }
                    else
                    {
                        lbMsg.Text = "Communication problem with the server";
                    }
                }
                else
                {
                    lbMsg.Text = "Login failed";
                    System.Threading.Thread.Sleep(1000);
                }
            }
            else
            {
                MessageBox.Show("Please fill user & password");
            }
        }

        private void DoProcess()
        {

        }

        private void btHelp_MouseHover(object sender, EventArgs e)
        {
            //btHelp.ImageIndex = 1;
        }

        private void btHelp_MouseLeave(object sender, EventArgs e)
        {
            //btHelp.ImageIndex = 0;
        }

        private void btHelp_Click(object sender, EventArgs e)
        {
            OpenUrl("https://www.recom3.com/web/#/profile");
        }

        private void btHome_Click(object sender, EventArgs e)
        {
            OpenUrl("https://www.recom3.com");
        }

        private void btLogout_Click(object sender, EventArgs e)
        {
            secret = "";
            ToggleLoginVisibility(true);
        }

        private void btRetry_Click(object sender, EventArgs e)
        {
            lbMsg.Text = "";
            System.Threading.Thread.Sleep(1000);
            DoSynchro(tbEmail.Text);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                //// hack because of this: https://github.com/dotnet/corefx/issues/10361
                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                //{
                //    url = url.Replace("&", "^&");
                //    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                //}
                //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                //{
                //    Process.Start("xdg-open", url);
                //}
                //else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                //{
                //    Process.Start("open", url);
                //}
                //else
                //{
                //    throw;
                //}
            }
        }

        BackgroundWorker bgw;

        class BGWorkerResult
        {
            public String mapInfo = "";
            public String serial = "";
            public bool isJet = false;
            public bool isDone = false;
        }

        private void WorkerDone(object sender, RunWorkerCompletedEventArgs e)
        {
            //finishing up stuff, perhaps hide the bar or something?
            //progressBar1.Visible = false;

            BGWorkerResult bgwResult = (BGWorkerResult)e.Result;

            if(!bgwResult.isDone)
            {
                lbMsg.Text = "No device connected";
                lbSerial.Visible = false;
                lbMapsVer.Visible = false;
                pbGoggles.Visible = false;
                pbSnow2.Visible = false;
                btRetry.Visible = true;
                btSync.Visible = false;

                //!!!
                btMap.Visible = true;
            }
            else
            {
                lbMsg.Text = "Everything up to date";
                if (bgwResult.serial != "")
                {
                    lbSerial.Text = String.Format("Serial Number: {0}", bgwResult.serial);
                }
                if (bgwResult.mapInfo != "")
                {
                    lbMapsVer.Text = String.Format("Map Ver: {0}", bgwResult.mapInfo);
                }
                if (bgwResult.isJet)
                {
                    pbGoggles.Image = Properties.Resources.jet_goggle;
                    pbSnow2.Image = Properties.Resources.jet_logo;
                }
                else
                {
                    pbGoggles.Image = Properties.Resources.snow_goggle;
                    pbSnow2.Image = Properties.Resources.anow2_logo;
                }
                lbSerial.Visible = true;
                lbMapsVer.Visible = true;
                pbGoggles.Visible = true;
                pbSnow2.Visible = true;
                btRetry.Visible = false;
                btSync.Visible = true;
                btMap.Visible = true;
            }

            progressBar1.Visible = false;
            progressBar2.Visible = false;
        }

        private void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null && e.UserState.GetType() == typeof(FileManager.ProgressBarReport))
            {
                if (((FileManager.ProgressBarReport)e.UserState).barType == FileManager.ProgressBarReport.BAR_TYPE.MAIN)
                {
                    progressBar1.Visible = true;
                    progressBar1.Value = e.ProgressPercentage;
                    lbMsg.Text = ((FileManager.ProgressBarReport)e.UserState).message;
                }
                else
                {
                    progressBar2.Visible = true;
                    progressBar2.Value = e.ProgressPercentage;
                }
            }
            else
            {
                progressBar1.Visible = true;
                progressBar1.Value = e.ProgressPercentage;
                if (e.UserState != null)
                {
                    lbMsg.Text = e.UserState.ToString();
                }
            }
        }

        string myMail = "";

        /// <summary>
        /// Manin worker function for sync
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkerLoop(object sender, DoWorkEventArgs e)
        {
            bool value = (Boolean)e.Argument;
            String mail = myMail;

            String mapInfo = "";
            String serial = "";
            bool isJet = false;

            bool result = mFileManager.ReadTrips(secret, mail, ref mapInfo, ref serial, ref isJet, bgw, value);

            bgw.ReportProgress(100);

            if (!result)
            {
                System.Threading.Thread.Sleep(1000);
            }
            else
            {
                System.Threading.Thread.Sleep(500);
            }

            BGWorkerResult bgwResult = new BGWorkerResult();
            bgwResult.isDone = result;
            bgwResult.mapInfo = mapInfo;
            bgwResult.serial = serial;
            bgwResult.isJet = isJet;

            e.Result = bgwResult;
        }

        private void DoSynchro(string mail)
        {
            myMail = mail;
            Boolean value = false;

            String pathUdtPackage = "Snow2-v3.0-to-v4.5";
            String pathUpt = Environment.ExpandEnvironmentVariables(FileManager.pathBase + pathUdtPackage);
            if (Directory.Exists(pathUpt))
            {
                var confirmResult = MessageBox.Show("Are you sure to proceed with the update? If yes, a new window will pop up with the push status to the device. Wait till the end, please. If you device is already updated to version 4.5, answer NO.",
                                 "Confirm proceed",
                                 MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    // If 'Yes', do something here.
                    value = true;
                }
                else
                {
                    // If 'No', do something here.
                }
            }

            bgw = new BackgroundWorker();
            bgw.DoWork += WorkerLoop;
            bgw.ProgressChanged += UpdateProgressBar;
            bgw.RunWorkerCompleted += WorkerDone;
            bgw.WorkerReportsProgress = true;
            bgw.RunWorkerAsync(argument: value);

            return;

            String mapInfo = "";
            String serial = "";
            bool isJet = false;

            if (!mFileManager.ReadTrips(secret, mail, ref mapInfo, ref serial, ref isJet, bgw, false))
            {
                System.Threading.Thread.Sleep(1000);
                lbMsg.Text = "No device connected";
                lbSerial.Visible = false;
                lbMapsVer.Visible = false;
                pbGoggles.Visible = false;
                pbSnow2.Visible = false;
                btRetry.Visible = true;
                btSync.Visible = false;
            }
            else
            {
                lbMsg.Text = "Everything up to date";
                if (serial != "")
                {
                    lbSerial.Text = String.Format("Serial Number: {0}", serial);
                }
                if (mapInfo != "")
                {
                    lbMapsVer.Text = String.Format("Map Ver: {0}", mapInfo);
                }
                if(isJet)
                {
                    pbGoggles.Image = Properties.Resources.jet_goggle;
                    pbSnow2.Image = Properties.Resources.jet_logo;
                }
                else
                {
                    pbGoggles.Image = Properties.Resources.snow_goggle;
                    pbSnow2.Image = Properties.Resources.anow2_logo;
                }
                lbSerial.Visible = true;
                lbMapsVer.Visible = true;
                pbGoggles.Visible = true;
                pbSnow2.Visible = true;
                btRetry.Visible = false;
                btSync.Visible = true;
            }
        }

        private void btSync_Click(object sender, EventArgs e)
        {
            lbMsg.Text = "";
            System.Threading.Thread.Sleep(1000);
            DoSynchro(tbEmail.Text);
        }

        XmlDocument doc;
        Dictionary<string, string> tilesInDevice = new Dictionary<string, string>();

        /*
         * DownloadedOSMTiles-Base-filelisting.txt
        <list>
        <s>ls /storage/sdcard0/ReconApps/GeodataService/DownloadedOSMTiles/Base</s>
        <list>
        <s>500000001.rgz</s>
        <s>version.txt</s>
        </list>
        </list> 
        */
        private void createBlankXMLMap()
        {
            doc = new XmlDocument();
            XmlElement rootList = doc.CreateElement("list");
            XmlElement sLs = doc.CreateElement("s");
            sLs.InnerText = "ls /storage/sdcard0/ReconApps/GeodataService/DownloadedOSMTiles/Base";
            rootList.AppendChild(sLs);

            if(tilesInDevice.Count==0)
            {
                tilesInDevice.Add("version", "version.txt");
            }

            XmlElement childList = doc.CreateElement("list");
            foreach (KeyValuePair<string, string> vc in tilesInDevice)
            {
                XmlElement sTile = doc.CreateElement("s");
                sTile.InnerText = vc.Value;
                childList.AppendChild(sTile);
            }
            rootList.AppendChild(childList);

            doc.AppendChild(rootList);
        }

        private void writeXMLTilesToDisk()
        {
            string pathTiles = Environment.ExpandEnvironmentVariables(FileManager.pathBase + "tiles");

            foreach (XmlNode xnode1 in doc.ChildNodes)
            {
                XmlNode oldChild = null;
                foreach (XmlNode xnode2 in xnode1.ChildNodes)
                {
                    string name = xnode2.Name;
                    string value = xnode2.InnerText;

                    if (value.IndexOf("ls ") >= 0)
                    {
                        continue;
                    }

                    oldChild = xnode2;
                    break;
                }

                XmlElement newChild = doc.CreateElement("list");

                foreach(KeyValuePair<string, string> vc in tilesInDevice)
                {
                    XmlElement element = doc.CreateElement("s");
                    element.InnerText = vc.Value;
                    newChild.AppendChild(element);
                }
                xnode1.ReplaceChild(newChild, oldChild);
            }

            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }

            /*
            DownloadedOSMTiles-Test-filelisting.txt
            DownloadedOSMTiles-Base-version.txt
             */
            File.WriteAllText(Environment.ExpandEnvironmentVariables(pathTiles) + @"\" + "DownloadedOSMTiles-Base-filelisting.txt", sb.ToString());

            File.WriteAllText(Environment.ExpandEnvironmentVariables(pathTiles) + @"\" + "DownloadedOSMTiles-Base-version.txt", "1");
        }

        private void loadTilesFromXml()
        {
            string pathTiles = Environment.ExpandEnvironmentVariables(FileManager.pathBase + "tiles");

            //Process xml
            try
            {
                string xmlData = "";
                string myPath = Environment.ExpandEnvironmentVariables(pathTiles) + @"\" + "DownloadedOSMTiles-Base-filelisting.txt";
                xmlData = File.ReadAllText(myPath);
                doc = new XmlDocument();
                doc.LoadXml(xmlData);
            }
            catch(Exception ex)
            {
                //Can not load xml
                return;
            }

            foreach (XmlNode xnode1 in doc.ChildNodes)
            {
                XmlNode oldChild = null;
                foreach (XmlNode xnode2 in xnode1.ChildNodes)
                {
                    string name = xnode2.Name;
                    string value = xnode2.InnerText;

                    if (value.IndexOf("ls ") >= 0)
                    {
                        continue;
                    }

                    oldChild = xnode2;
                    break;
                }

                foreach (XmlNode xnode3 in oldChild.ChildNodes)
                {
                    string tile = xnode3.InnerText;
                    if (!tilesInDevice.ContainsKey(Path.GetFileNameWithoutExtension(tile)))
                    {
                        tilesInDevice.Add(Path.GetFileNameWithoutExtension(tile), tile);
                    }
                }
            }

            /*
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            JObject jObj = JObject.Parse(jsonText);

            foreach (var node1 in jObj["list"])
            {
                try
                {
                    foreach (JObject obj in (JToken)node1)
                    {
                        foreach (var x in obj)
                        {
                            string sT = x.GetType().ToString();

                            string name = x.Key;
                            JArray value = (JArray)x.Value;

                            foreach (JValue v in value)
                            {
                                string tile = v.Value.ToString();
                                if (!tilesInDevice.ContainsKey(tile))
                                {
                                    tilesInDevice.Add(Path.GetFileNameWithoutExtension(tile), tile);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }
            */

            /*
            string[] parameterNames = new string[] { "Test1", "Test2", "Test3" };
            JArray jarrayObj = new JArray();
            foreach (string parameterName in parameterNames)
            {
                jarrayObj.Add(parameterName);
            }

            string txtBday = "2011-05-06";
            string txtemail = "dude@test.com";
            JObject UpdateAccProfile = new JObject(
                                           new JProperty("_delete", jarrayObj),
                                           new JProperty("birthday", txtBday),
                                           new JProperty("email", txtemail));

            Console.WriteLine(UpdateAccProfile.ToString());
            */
        }

        private void mixTilesWithExisting(List<int> lstTiles)
        {
            foreach(int tile in lstTiles)
            {
                if(!tilesInDevice.ContainsKey(tile.ToString()))
                {
                    tilesInDevice.Add(tile.ToString(),string.Format("{0}.rgz",tile));
                }
            }
        }
        
        private void btMap_Click(object sender, EventArgs e)
        {
            //loadTilesFromXml();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "osm files (*.osm)|*.osm";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openFileDialog1.FileName;
                RectXY rect = new RectXY(0, 0, 0, 0);
                OsmXmlReader osmXmlReader = new OsmXmlReader();
                //Never for pro! This apply offset to bring the ski resort to your place for testing
                //osmXmlReader.readFile(selectedFileName, ref rect, -1.40004656, -0.564256192);
                osmXmlReader.readFile(selectedFileName, ref rect, 0, 0);

                //Now we get bound from previous call
                GeoRegion geoRegion = new GeoRegion();
                geoRegion.makeUsingBoundingBox(rect.left, rect.top, rect.right, rect.bottom);
                List<int> ignoreTiles = new List<int>();
                List<int> lstTiles = GeoTile.getTileListForGeoRegion(geoRegion, ignoreTiles);

                string pathTiles = Environment.ExpandEnvironmentVariables(FileManager.pathBase + "tiles");

                mFileManager.CreateRootDir(pathTiles);

                //Never for pro!
                //Dif
                //Y = -0,564256192 X = -1,40004656
                //-1.40004656,-0.564256192
                //osmXmlReader.WriteResultToDisk(pathTiles, pathTiles + @"\500000001",
                //    -1.40004656, -0.564256192, lstTiles);
                osmXmlReader.WriteResultToDisk(pathTiles, pathTiles + @"\500000001",
                    0, 0, lstTiles);

                loadTilesFromXml();

                if(doc==null)
                {
                    createBlankXMLMap();
                }

                mixTilesWithExisting(lstTiles);

                writeXMLTilesToDisk();

                this.mFileManager.m_lstTiles = lstTiles;
            }
        }

        private void testCodeSer()
        {
            string pathTiles = Environment.ExpandEnvironmentVariables(FileManager.pathBase + "tiles");

            //Process xml
            bool testProc = false;
            if (testProc)
            {
                string xmlData = "";
                string myPath = Environment.ExpandEnvironmentVariables(pathTiles) + @"\" + "DownloadedOSMTiles-Base-filelisting.txt";
                xmlData = File.ReadAllText(myPath);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlData);

                string jsonText = JsonConvert.SerializeXmlNode(doc);

                var dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

                //------------------------------------------------------------------------------------

                string jsonString = JsonConvert.SerializeObject(dyn);

                XmlDocument doc1 = JsonConvert.DeserializeXmlNode(jsonString);

                //StringWriter stringWriter = new StringWriter();
                //XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
                //doc1.WriteTo(xmlTextWriter);

                //string s = Regex.Replace(stringWriter.ToString(), @"(<.*/>)", @"$1\r\n");
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = "",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };
                using (XmlWriter writer = XmlWriter.Create(sb, settings))
                {
                    doc1.Save(writer);
                }

                File.WriteAllText(Environment.ExpandEnvironmentVariables(pathTiles) + @"\" + "DownloadedOSMTiles-Test-filelisting.txt", sb.ToString());
            }

        }
    }
}
