using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MediaDevices;
using Newtonsoft.Json;

using System.IO.Compression;
using Recom3Uplnk.Model;
using Microsoft.Data.Sqlite;

namespace Recom3Uplnk
{
    class FileManager
    {
        Boolean m_isTripDataReaded = false;
        Boolean m_isTripDataUploaded = false;
        public List<int> m_lstTiles;

        //private const string URL_CERT = "http://localhost:51192/api/";
        //private const string URL_PRO = "https://www.recom3.com/api/";

        private const string URL = CommManager.URL;
        
        private static string urlParameters = "metripsupdate";

        private static string urlPackageUpdate = "packages";
        private static string urlPackagesUserUpload = "mepackagessupdate";

        private static string urlMapsUpdate = "memaps";

        public static string pathBase = @"%userprofile%\AppData\Local\Recon Instruments\";
        private static string fnSettings = "recon_t.json";
        private static string fnSettingRecom = "recom_pref.json";

        public class Unit
        {
            public string Id;
            public int MaxRib;
        }

        public class Config
        {
            public string Email;
            public Unit[] Unit;
        }

        public class ProgressBarReport
        {
            public enum BAR_TYPE { MAIN, FILE_DOWNLOAD};

            public BAR_TYPE barType;
            public string message;

            public ProgressBarReport(BAR_TYPE type, string msg)
            {
                barType = type;
                message = msg;
            }
        }

        Config mConfig;

        public FileManager()
        {
            Initialize();
        }

        public void Initialize()
        {

        }

        public void CreateRootDir(String sDir)
        {
            if (!Directory.Exists(sDir))
            {
                Directory.CreateDirectory(sDir);
            }
        }

        int GetMaxRif(String fileName)
        {
            int maxRib = 0;
            if (File.Exists(fileName))
            {
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        try
                        {
                            maxRib = Int32.Parse(line.Trim());
                        }
                        catch (Exception ex)
                        {

                        }
                        break;
                    }
                }
            }
            return maxRib;
        }

        int GetMaxRifJson(String fileName, String idUnit)
        {
            int maxRib = 0;
            if (File.Exists(fileName))
            {
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    String output = "";
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        output += line;
                    }

                    try
                    {
                        mConfig = JsonConvert.DeserializeObject<Config>(output);
                    }
                    catch (Exception ex)
                    {

                    }

                    int index = 0;
                    String Id = "";
                    while (index < mConfig.Unit.Length)
                    {
                        Id = mConfig.Unit[index].Id;
                        if (Id == idUnit)
                        {
                            maxRib = mConfig.Unit[index].MaxRib;
                        }
                        index++;
                    }
                }
            }
            return maxRib;
        }

        public String GetEmailJson()
        {
            int maxRib = 0;
            string fileName = Environment.ExpandEnvironmentVariables(pathBase + fnSettingRecom);

            if (File.Exists(fileName))
            {
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    String output = "";
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        output += line;
                    }

                    try
                    {
                        mConfig = JsonConvert.DeserializeObject<Config>(output);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            if (mConfig != null)
            {
                return mConfig.Email;
            }
            else
            {
                return "";
            }
        }

        void WriteMaxRif(String fileName, int maxRib)
        {
            var exists = File.Exists(fileName);
            var fileMode = exists ? FileMode.Truncate : FileMode.CreateNew;

            using (var fs = File.Open(fileName, fileMode))
            {
                // writing data in string
                string dataasstring = string.Format("{0}", maxRib);
                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                fs.Write(info, 0, info.Length);
            }
        }

        void WriteMaxRifJson(String fileName, String idUnit, int maxRib, String mail)
        {
            var exists = File.Exists(fileName);
            var fileMode = exists ? FileMode.Truncate : FileMode.CreateNew;

            using (var fs = File.Open(fileName, fileMode))
            {
                // writing data in string
                if (mConfig==null)
                {
                    mConfig = new Config();
                }
                if(mConfig.Unit==null)
                {
                    mConfig.Unit = new Unit[1];
                    mConfig.Unit[0] = new Unit();
                    mConfig.Unit[0].Id = idUnit;
                }
                mConfig.Email = mail;

                int index = 0;
                String Id = "";
                bool isUnitInPref = false;
                while(index<mConfig.Unit.Length)
                {
                    Id = mConfig.Unit[index].Id;
                    if (Id==idUnit)
                    {
                        mConfig.Unit[index].MaxRib = maxRib;
                        isUnitInPref = true;
                    }
                    index++;
                }

                if(!isUnitInPref)
                {
                    Unit[] Units = new Unit[mConfig.Unit.Length+1];
                    for (int i = 0; i < mConfig.Unit.Length; i++)
                    {
                        Units[i] = new Unit();
                        Units[i].Id = mConfig.Unit[i].Id;
                        Units[i].MaxRib = mConfig.Unit[i].MaxRib;
                    }
                    Units[mConfig.Unit.Length] = new Unit();
                    Units[mConfig.Unit.Length].Id = idUnit;
                    Units[mConfig.Unit.Length].MaxRib = maxRib;
                    mConfig.Unit = Units;
                }

                string dataasstring = JsonConvert.SerializeObject(mConfig);
                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                fs.Write(info, 0, info.Length);
            }
        }

        SortedDictionary<String, MediaFileInfo> ReadFilesFromDevice(MediaDevice device, string logPath, ref int maxRib)
        {
            //device.Connect();
            var photoDir = device.GetDirectoryInfo(@"\Internal storage\ReconApps\TripData");

            var files = photoDir.EnumerateFiles("DAY*.RIB", SearchOption.TopDirectoryOnly);

            //Copy to sorted list
            SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();
            //var sortedDict = from entry in myDict orderby entry.Value ascending select entry;

            string combinedString = string.Join(Environment.NewLine, sdFiles.Keys.ToArray());
            File.AppendAllText(logPath, Environment.NewLine + string.Format("Available files: {0}", combinedString));

            Regex regex = new Regex(@"\d+");

            ulong minBytes = 10000;//20000

            int tempMaxRib = maxRib;

            //1st Iter.
            foreach (var file in files)
            {
                if (!sdFiles.ContainsKey(file.Name))
                {
                    Match match = regex.Match(file.Name);
                    if (match.Success)
                    {
                        try
                        {
                            ulong len = file.Length;
                            if (len >= minBytes && Int32.Parse(match.Value) > maxRib)
                            {
                                tempMaxRib = Math.Max(Int32.Parse(match.Value), tempMaxRib);
                                sdFiles.Add(file.Name, file);
                            }
                            else
                            {
                                File.AppendAllText(logPath, Environment.NewLine + string.Format("File with len {0} and rib num. {1} out of criteria", len, match.Value));
                            }
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, Environment.NewLine + string.Format("Exception in loop: {0}", ex.Message));
                        }
                    }
                    else
                    {
                        File.AppendAllText(logPath, Environment.NewLine + string.Format("Not match file: {0}", file.Name));
                    }
                }
            }

            maxRib = tempMaxRib;

            return sdFiles;
        }

        SortedDictionary<String, MediaFileInfo> ReadTilesFromDevice(MediaDevice device)
        {
            try
            {
                var baseDir = device.GetDirectoryInfo(@"\Internal storage\ReconApps\GeodataService");

                var files = baseDir.EnumerateFiles("DownloadedOSMTiles-Base-filelisting.txt", SearchOption.TopDirectoryOnly);

                //Copy to sorted list
                SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();

                foreach (var file in files)
                {
                    if (!sdFiles.ContainsKey(file.Name))
                    {
                        sdFiles.Add(file.Name, file);
                    }
                }

                files = baseDir.EnumerateFiles("DownloadedOSMTiles-Base-version.txt", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (!sdFiles.ContainsKey(file.Name))
                    {
                        sdFiles.Add(file.Name, file);
                    }
                }

                return sdFiles;
            }
            catch(Exception ex)
            {
                SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();
                return sdFiles;
            }
        }

        SortedDictionary<String, MediaFileInfo> ReadMapsFromDevice(MediaDevice device)
        {
            var photoDir = device.GetDirectoryInfo(@"\Internal storage\ReconApps\\MapData");

            var files = photoDir.EnumerateFiles("db_info.xml", SearchOption.TopDirectoryOnly);

            //Copy to sorted list
            SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();

            Regex regex = new Regex(@"db_info\.xml");

            //1st Iter.
            foreach (var file in files)
            {
                if (!sdFiles.ContainsKey(file.Name))
                {
                    sdFiles.Add(file.Name, file);
                }
            }

            return sdFiles;
        }

        SortedDictionary<String, MediaFileInfo> ReadMapLstFromDevice(MediaDevice device)
        {
            var photoDir = device.GetDirectoryInfo(@"\Internal storage\ReconApps\\MapData");

            var files = photoDir.EnumerateFiles("resortinfo.db", SearchOption.TopDirectoryOnly);

            //Copy to sorted list
            SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();

            //1st Iter.
            foreach (var file in files)
            {
                if (!sdFiles.ContainsKey(file.Name))
                {
                    sdFiles.Add(file.Name, file);
                }
            }

            files = photoDir.EnumerateFiles("mdmaps.zip", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (!sdFiles.ContainsKey(file.Name))
                {
                    sdFiles.Add(file.Name, file);
                }
            }

            return sdFiles;
        }

        SortedDictionary<String, MediaFileInfo> ReadPackagesFromDevice(MediaDevice device)
        {
            var photoDir = device.GetDirectoryInfo(@"\Internal storage\ReconApps\\\Installer");

            var files = photoDir.EnumerateFiles("installed_packages.xml", SearchOption.TopDirectoryOnly);

            //Copy to sorted list
            SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();

            //1st Iter.
            foreach (var file in files)
            {
                if (!sdFiles.ContainsKey(file.Name))
                {
                    sdFiles.Add(file.Name, file);
                }
            }

            return sdFiles;
        }

        SortedDictionary<String, MediaFileInfo> ReadIdFromDevice(MediaDevice device)
        {
            device.Connect();
            var photoDir = device.GetDirectoryInfo(@"\Internal storage\ReconApps");

            var files = photoDir.EnumerateFiles("ID.RIB", SearchOption.TopDirectoryOnly);

            //Copy to sorted list
            SortedDictionary<String, MediaFileInfo> sdFiles = new SortedDictionary<string, MediaFileInfo>();

            //1st Iter.
            foreach (var file in files)
            {
                if (!sdFiles.ContainsKey(file.Name))
                {
                    sdFiles.Add(file.Name, file);
                }
            }

            return sdFiles;
        }

        List<string> CopyRibFilesToDisk(SortedDictionary<String, MediaFileInfo> sdFiles, MediaDevice device)
        {
            var sortedDictEnum = sdFiles.GetEnumerator();
            List<string> lsResult = new List<string>();

            while (sortedDictEnum.MoveNext())
            {
                MemoryStream memoryStream = new System.IO.MemoryStream();
                device.DownloadFile(sortedDictEnum.Current.Value.FullName, memoryStream);
                memoryStream.Position = 0;
                string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{sortedDictEnum.Current.Value.Name}");

                WriteSreamToDisk(localName, memoryStream);

                lsResult.Add(localName);
            }

            return lsResult;
        }

        List<string> CopyRibFilesToDisk(SortedDictionary<String, MediaFileInfo> sdFiles, MediaDevice device, string relPath)
        {
            var sortedDictEnum = sdFiles.GetEnumerator();
            List<string> lsResult = new List<string>();

            while (sortedDictEnum.MoveNext())
            {
                MemoryStream memoryStream = new System.IO.MemoryStream();
                device.DownloadFile(sortedDictEnum.Current.Value.FullName, memoryStream);
                memoryStream.Position = 0;
                string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{relPath}{sortedDictEnum.Current.Value.Name}");

                WriteSreamToDisk(localName, memoryStream);

                lsResult.Add(localName);
            }

            return lsResult;
        }

        /// <summary>
        /// Copy a list of files to devices
        /// </summary>
        /// <param name="sdFiles">List of files to copy without path</param>
        /// <param name="device">Device to copy to</param>
        void CopyFilesToDevice(SortedDictionary<String, String> sdFiles, MediaDevice device)
        {
            var sortedDictEnum = sdFiles.GetEnumerator();

            while (sortedDictEnum.MoveNext())
            {
                string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{sortedDictEnum.Current.Key}");
                try
                {
                    device.DeleteFile(sortedDictEnum.Current.Value);
                }
                catch(Exception ex)
                {

                }
                device.UploadFile(localName, sortedDictEnum.Current.Value);
            }
        }

        //https://stackoverflow.com/questions/25026799/how-to-read-files-on-android-phone-from-c-sharp-program-on-windows-7
        public bool ReadTrips(String secret, String mail, ref String mapInfo, ref String serial, ref bool isJet, BackgroundWorker bgw, bool tryUpt)
        {
            m_isTripDataReaded = false;

            try
            {
                //---------------------------------------------------------------------------------------
                String pathUdtPackage = "Snow2-v3.0-to-v4.5";
                String pathUpt = Environment.ExpandEnvironmentVariables(pathBase + pathUdtPackage);
                String pathUptZip = Environment.ExpandEnvironmentVariables(pathBase + "snow2_30_to_45.zip");
                if (tryUpt && Directory.Exists(pathUpt))
                {   
                    //Ask user and proceed
                    CopyUpdatePckToDevice(pathUpt);
                    return true;
                }
                //---------------------------------------------------------------------------------------

                var devices = MediaDevice.GetDevices();
                SortedDictionary<String, MediaFileInfo> sdFiles;
                SortedDictionary<String, MediaFileInfo> sdFilesMap;
                SortedDictionary<String, MediaFileInfo> sdFilesMapLst;

                SortedDictionary<String, MediaFileInfo> sdFilesTiles;

                foreach (var device in devices)
                {
                    List<PackageInfoExt> lstPackageDevice = null;
                    string sDevice = device.Description.Trim().ToLower();

                    //Este equipo\Snow2\Internal storage\ReconApps\TripData
                    if (device.Description.Trim().ToLower().IndexOf("snow2") >= 0
                        || device.Description.Trim().ToLower().IndexOf("huawei p10") >= 0
                        || device.Description.Trim().ToLower().IndexOf("jet") >= 0
                        )
                    {
                        if (device.Description.Trim().ToLower().IndexOf("jet") >= 0)
                        {
                            isJet = true;
                        }
                        else
                        {
                            isJet = false;
                        }

                        using (device)
                        {
                            int maxRib = 0;

                            string fileName = Environment.ExpandEnvironmentVariables(pathBase + fnSettings);
                            string fileNamePref = Environment.ExpandEnvironmentVariables(pathBase + fnSettingRecom);

                            //Create local directories part
                            //---------------------------------------------------------------------
                            //Load maxRib from file
                            CreateRootDir(Environment.ExpandEnvironmentVariables(pathBase));
                            //maxRib = GetMaxRif(fileName);

                            //Create temp directory
                            CreateRootDir(Environment.ExpandEnvironmentVariables(pathBase + "areas"));
                            CreateRootDir(Environment.ExpandEnvironmentVariables(pathBase + "lines"));
                            CreateRootDir(Environment.ExpandEnvironmentVariables(pathBase + "points"));

                            //Create root dir for files
                            CreateRootDir(Environment.ExpandEnvironmentVariables(pathBase + "tiles"));

                            //Create backup directory
                            CreateRootDir(Environment.ExpandEnvironmentVariables(pathBase + "backup"));

                            //---------------------------------------------------------------------

                            //Get id unit
                            //To test unit id used is: "283543590" -> Recon Jet Pink?
                            SortedDictionary<String, MediaFileInfo> sdFilesId = ReadIdFromDevice(device);
                            if (sdFilesId.Count >= 1)
                            {
                                CopyRibFilesToDisk(sdFilesId, device);
                                serial = ReadSerialInfo(sdFilesId).Trim();
                                if (serial.Length > 0)
                                {
                                    sDevice = serial;
                                }
                            }

                            string pathPackagesTemp = @"D:\pull_test\app";

                            //Create root dir for unit
                            pathPackagesTemp = Environment.ExpandEnvironmentVariables(pathBase) + sDevice;
                            CreateRootDir(pathPackagesTemp);
                            //CreateRootDir(pathPackagesTemp + @"\app");

                            bgw.ReportProgress(10, "Retrieved unid ID");

                            //Copy packages to server block
                            //These packages are pushed from a previous run
                            //-----------------------------------------------------------------------------------------------------

                            bool isZipFileCreated = createZip(pathPackagesTemp);

                            string[] zipFiles = Directory.GetFiles(pathPackagesTemp, "*.zip", SearchOption.AllDirectories);

                            if (isZipFileCreated)
                            {
                                List<PackageInfoExt> lst = new List<PackageInfoExt>();

                                foreach (string fPath in zipFiles)
                                {
                                    PackageInfoExt pkg = new PackageInfoExt(Path.GetFileName(fPath), "", "", fPath, sDevice);
                                    lst.Add(pkg);
                                }

                                PushAllUserPackagesBinToSever(lst, secret);
                            }
                            //-----------------------------------------------------------------------------------------------------

                            bgw.ReportProgress(30, "Process previous tasks");

                            //New config file json
                            maxRib = Math.Max(GetMaxRifJson(fileNamePref, sDevice), maxRib);

                            string logPath = Environment.ExpandEnvironmentVariables($@"{pathBase}recom3.log");
                            File.WriteAllText(logPath, string.Format("MaxRib is {0}", maxRib));

                            sdFiles = ReadFilesFromDevice(device, logPath, ref maxRib);

                            string combinedString = string.Join(Environment.NewLine, sdFiles.Keys.ToArray());
                            File.AppendAllText(logPath, Environment.NewLine + string.Format("Read files: {0}", combinedString));
                            File.AppendAllText(logPath, Environment.NewLine + string.Format("MaxRib after is {0}", maxRib));

                            sdFilesMap = ReadMapsFromDevice(device);

                            //resortinfo.db, mdmaps.zip
                            sdFilesMapLst = ReadMapLstFromDevice(device);

                            //Read tile info from device
                            sdFilesTiles = ReadTilesFromDevice(device);

                            bgw.ReportProgress(40, "Retrieved tile info from device");

                            SortedDictionary<String, MediaFileInfo> sdFilesPackages = ReadPackagesFromDevice(device);

                            //Write maxRib to file
                            //WriteMaxRif(fileName, maxRib);
                            WriteMaxRifJson(fileNamePref, sDevice, maxRib, mail);

                            //Copy rib files and others to disk
                            //-------------------------------------------------------------------------------------------------
                            List<string> lstRib = CopyRibFilesToDisk(sdFiles, device);
                            combinedString = string.Join(Environment.NewLine, lstRib.ToArray());
                            File.AppendAllText(logPath, string.Format(Environment.NewLine + "Written Rib files: {0}", combinedString));

                            CopyRibFilesToDisk(sdFilesMap, device);

                            CopyRibFilesToDisk(sdFilesPackages, device);

                            if (m_lstTiles == null || m_lstTiles.Count == 0)
                            {
                                CopyRibFilesToDisk(sdFilesTiles, device, @"tiles\");
                            }

                            //Copy maps here resortinfo.db, mdmaps.zip
                            try
                            {
                                CopyRibFilesToDisk(sdFilesMapLst, device);
                            }
                            catch (Exception ex)
                            {
                                string msg = ex.Message;
                                sdFilesMapLst = new SortedDictionary<string, MediaFileInfo>();
                            }
                            //-------------------------------------------------------------------------------------------------

                            bgw.ReportProgress(50, "Files got from device...");

                            //Read packages block: refactor to other function
                            //-------------------------------------------------------------------------------------

                            //Download from server
                            //---------------------------------------------------------------------
                            SortedDictionary<String, String> sdFilesPackagesUpdates = new SortedDictionary<String, String>();

                            bool testDownload = true;
                            if (testDownload)
                            {
                                Dictionary<String, PackageInfo> dicPackages = DoPreparePackagesUpdatesFromServer(secret);

                                string fileTest = "installed_packages.xml";
                                SortedDictionary<string, MediaFileInfo> testDict = new SortedDictionary<string, MediaFileInfo>();
                                testDict.Add(fileTest, null);
                                lstPackageDevice = ReadPackagesInfo(testDict, sDevice);

                                //Prepare list to download
                                List<PackageInfo> lstPackageDownload = DoPrepareListToDownload(dicPackages, lstPackageDevice);

                                PackageInfoExt result = lstPackageDevice.Find(x => x.name == "com.reconinstruments.applauncher");

                                //Update package section
                                bool tryDownloadUdt = false;
                                bool tryUnzipFilesPkg = false;
                                if (!isJet)
                                {
                                    if (result != null)
                                    {
                                        tryDownloadUdt = result.versionName != "4.5";
                                        tryUnzipFilesPkg = result.versionName != "4.5";

                                        //If zip exist and we are in 4.5 delete folder
                                        if (!tryUnzipFilesPkg && Directory.Exists(pathUpt))
                                        {
                                            DirectoryInfo dirInfo = new DirectoryInfo(pathUpt);
                                            RecursiveDelete(dirInfo);
                                        }
                                        else if(result.versionName != "4.5")
                                        {
                                            //If we dont find com.reconinstruments.applauncher try to update anyway
                                            tryDownloadUdt = true;
                                            tryUnzipFilesPkg = true;
                                        }

                                        //Don't download if it exists
                                        if (File.Exists(pathUptZip))
                                        {
                                            tryDownloadUdt = false;
                                        }
                                    }

                                    //Inject here to download update
                                    if (tryDownloadUdt)
                                    {
                                        PackageInfo pkgSnowUpdate = new PackageInfo("snow2_30_to_45.zip", "", "", "snow2_30_to_45.zip", "");
                                        lstPackageDownload.Add(pkgSnowUpdate);
                                        bgw.ReportProgress(55, "Start download update. This can take a while. 200Mb about 30 min, depending of speed. Don't close the app.");
                                    }
                                    else
                                    {
                                        bgw.ReportProgress(55, "Packages download list prepared. Start download");
                                    }

                                    //Download files
                                    bool downloadFiles = true;
                                    if (downloadFiles)
                                    {
                                        lstPackageDownload.ForEach(item =>
                                        {
                                            if (!item.fileName.ToLower().EndsWith(".zip"))
                                            {
                                                DownloadPackageFromServer(item.fileName, secret);
                                            }
                                            else
                                            {
                                                DownloadUpdatePackageFromServer(item.fileName, secret, bgw);
                                            }

                                            while (!m_isReady)
                                            {
                                                System.Threading.Thread.Sleep(200);
                                            }

                                            m_isReady = false;
                                        });
                                    }

                                    //---------------------------------------------------------------------

                                    //Copy to device section
                                    //Inject here to copy update package
                                    //-----------------------------------------------------------------

                                    if (tryUnzipFilesPkg)
                                    {
                                        if (!tryDownloadUdt)
                                        {
                                            PackageInfo pkgSnowUpdate = new PackageInfo("snow2_30_to_45.zip", "", "", "snow2_30_to_45.zip", "");
                                            lstPackageDownload.Add(pkgSnowUpdate);
                                            bgw.ReportProgress(55, "Packages download list prepared. Start download update. This can take a while");
                                        }

                                        lstPackageDownload.ForEach(item =>
                                        {
                                            if (item.fileName.ToLower().EndsWith(".zip"))
                                            {
                                                bgw.ReportProgress(60, "Unzip update packages");

                                            //Unzip
                                            string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{item.fileName}");
                                                string pathTemp = pathBase.Substring(0, pathBase.Length - 1);

                                                string subFolder = "";
                                                try
                                                {
                                                    subFolder = UnzipToFolder(localName, Environment.ExpandEnvironmentVariables($@"{pathTemp}"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    Debug.Write("Ex:" + ex.Message.ToString());
                                                    logPath = Environment.ExpandEnvironmentVariables($@"{pathBase}recom3.log");
                                                    File.WriteAllText(logPath, ex.Message);
                                                }

                                                bgw.ReportProgress(62, "Copy update packages to device. This can take about 10 min. Don't close the app.");

                                            //Ready for test
                                            CopyUpdatePckToDevice(Environment.ExpandEnvironmentVariables($@"{pathBase}{subFolder}\ReconApps"));
                                            }
                                        });
                                    }

                                    bgw.ReportProgress(65, "Copy packages to device");

                                    lstPackageDownload.ForEach(item =>
                                    {
                                        if (!item.fileName.ToLower().EndsWith(".zip"))
                                        {
                                            sdFilesPackagesUpdates.Add(item.fileName, "\\Internal storage\\ReconApps\\Installer\\" + item.fileName);
                                        }
                                    });

                                    CopyFilesToDevice(sdFilesPackagesUpdates, device);

                                    //Copy packages from device
                                    if (!isZipFileCreated && (zipFiles == null || zipFiles.Length == 0))
                                    {
                                        bgw.ReportProgress(65, "Copy packages from device");
                                        CopyPackagesFromDevice(pathPackagesTemp);
                                    }

                                }// end if test download
                                else
                                {
                                    //Download files
                                    bool downloadFiles = true;
                                    if (downloadFiles)
                                    {
                                        lstPackageDownload.ForEach(item =>
                                        {
                                            if (!item.fileName.ToLower().EndsWith(".zip"))
                                            {
                                                DownloadPackageFromServer(item.fileName, secret);
                                            }
                                            else
                                            {
                                                DownloadUpdatePackageFromServer(item.fileName, secret, bgw);
                                            }

                                            while (!m_isReady)
                                            {
                                                System.Threading.Thread.Sleep(200);
                                            }

                                            m_isReady = false;
                                        });
                                    }

                                    bgw.ReportProgress(65, "Copy packages to device");

                                    lstPackageDownload.ForEach(item =>
                                    {
                                        if (!item.fileName.ToLower().EndsWith(".zip"))
                                        {
                                            sdFilesPackagesUpdates.Add(item.fileName, "\\Internal storage\\ReconApps\\Installer\\" + item.fileName);
                                        }
                                    });

                                    CopyFilesToDevice(sdFilesPackagesUpdates, device);
                                }

                                bgw.ReportProgress(70, "New packages downloaded from server");

                                //Now try to update the maps here
                                if (sdFilesMapLst.Count > 0)
                                {
                                    DownloadMapsFromServer(secret, device);
                                }

                                if (m_lstTiles != null && m_lstTiles.Count > 0)
                                {
                                    //Copy maps to device
                                    /*
                                    DownloadedOSMTiles-Test-filelisting.txt
                                    DownloadedOSMTiles-Base-version.txt
                                    */

                                    string[] mapFiles = new string[] { @"tiles\DownloadedOSMTiles-Base-filelisting.txt", @"tiles\DownloadedOSMTiles-Base-version.txt" };
                                    string[] mapFilesDest = new string[] { @"DownloadedOSMTiles-Base-filelisting.txt", "DownloadedOSMTiles-Base-version.txt" };

                                    int index = 0;
                                    foreach (string localFileName in mapFiles)
                                    {
                                        sdFilesPackagesUpdates.Add(localFileName, "\\Internal storage\\ReconApps\\GeodataService\\" + mapFilesDest[index]);
                                        index++;
                                    }

                                    //foreach(KeyValuePair<string,string> kv in m_lstTiles)
                                    foreach (int tile in m_lstTiles)
                                    {
                                        sdFilesPackagesUpdates.Add(string.Format(@"tiles\{0}.rgz", tile), string.Format("\\Internal storage\\ReconApps\\GeodataService\\DownloadedOSMTiles\\Base\\{0}.rgz", tile));
                                    }

                                    //Make destination directories
                                    try
                                    {
                                        device.CreateDirectory("\\Internal storage\\ReconApps\\GeodataService");
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    try
                                    {
                                        device.CreateDirectory("\\Internal storage\\ReconApps\\GeodataService\\DownloadedOSMTiles");
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    try
                                    {
                                        device.CreateDirectory("\\Internal storage\\ReconApps\\GeodataService\\DownloadedOSMTiles\\Base");
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    bool isCopyToDevice = true;
                                    if (isCopyToDevice)
                                    {
                                        CopyFilesToDevice(sdFilesPackagesUpdates, device);
                                    }
                                }

                                bgw.ReportProgress(80, "New maps downloaded from server");

                                //-------------------------------------------------------------------------------------

                                device.Disconnect();

                            }

                            //3rd push trips to server
                            //Credential cred = CredentialManager.ReadCredential("Uplink engage.reconinstruments.com");

                            if (sdFiles.Count > 0)
                            {
                                //New code
                                //-------------------------------------------------------------------------------------
                                List<FileManager.PackageInfoExt> lstPackageDeviceTrips = new List<FileManager.PackageInfoExt>();

                                var sortedDictEnum = sdFiles.GetEnumerator();

                                while (sortedDictEnum.MoveNext())
                                {
                                    string trackFile = sortedDictEnum.Current.Key;

                                    //File name will be overwritten
                                    lstPackageDeviceTrips.Add(new FileManager.PackageInfoExt(trackFile, "", "", "gpxFileName", ""));
                                }

                                //Push trips to DB and make name unique
                                FileManager.DoPushAllTrips(lstPackageDeviceTrips, secret);

                                //Change the source file name
                                //foreach (FileManager.PackageInfoExt trackInfo in lstPackageDeviceTrips)
                                //{
                                //    trackInfo.fileName = Environment.ExpandEnvironmentVariables(FileManager.pathBase + trackInfo.name);
                                //}

                                //Push the gpx
                                FileManager.DoPushAllTripBinToServer(lstPackageDeviceTrips, secret);
                                //-------------------------------------------------------------------------------------
                            }
                            else
                            {
                                var dir = new DirectoryInfo(Environment.ExpandEnvironmentVariables(pathBase));

                                foreach (var file in dir.EnumerateFiles("DAY*.RIB"))
                                {
                                    file.Delete();
                                }
                            }

                            if (sdFilesMap.Count > 0)
                            {
                                mapInfo = ReadMapInfo(sdFilesMap);
                            }

                            //Push packages
                            if (lstPackageDevice != null)
                            {
                                PushAllUserPackagesToSever(lstPackageDevice, secret);

                                //PushAllUserPackagesBinToSever(lstPackageDevice, secret);
                            }

                            bgw.ReportProgress(90, "Trips pushed to server. Process done");

                            m_isTripDataReaded = true;
                        }//End using
                    }//if device snow2, jet...
                }//for each device
            }
            catch (Exception ex)
            {
                Debug.Write("Ex:" + ex.Message.ToString());
                string logPath = Environment.ExpandEnvironmentVariables($@"{pathBase}recom3.log");
                File.WriteAllText(logPath, ex.Message);
            }

            return m_isTripDataReaded;
        }

        /// <summary>
        /// Downloads maps updates from server
        /// </summary>
        /// <param name="secret"></param>
        void DownloadMapsFromServer(string secret, MediaDevice device)
        {
            SortedDictionary<String, String> sdFilesPackagesUpdates = new SortedDictionary<String, String>();

            //We check what is available in the server
            Dictionary<String, MapInfo> dicMaps = DoPrepareMapsUpdatesFromServer(secret);

            //Prepare list to download
            List<MapInfo> lstPackageDownload = DoPrepareMapListToDownload(dicMaps, null);

            //Download files
            bool downloadFiles = true;
            if (downloadFiles)
            {
                lstPackageDownload.ForEach(item =>
                {
                    DownloadMapFromServer(item.name, secret);

                    while (!m_isReady)
                    {
                        System.Threading.Thread.Sleep(200);
                    }

                    m_isReady = false;
                });
            }

            //DoBackupOfMapsDb();

            //Then add entry to database
            bool isAddedToDb = true;
            isAddedToDb = AddEntriesToDb(dicMaps);

            //Now add to zip
            if (isAddedToDb)
            {
                DoBackupOfMapsDb();

                string zipFile = "mdmaps.zip";
                string zipPath = Environment.ExpandEnvironmentVariables($@"{pathBase}{zipFile}");
                AddFilesToZip(zipPath, lstPackageDownload);

                File.Copy(Environment.ExpandEnvironmentVariables($@"{pathBase}resortinfo.db"),
                    Environment.ExpandEnvironmentVariables($@"{pathBase}resortinfo1.db"), true);

                string[] mapFiles = new string[] { "mdmaps.zip", "resortinfo1.db" };
                string[] mapFilesDest = new string[] { "mdmaps.zip", "resortinfo.db" };
                int index = 0;
                foreach (string localFileName in mapFiles)
                {
                    sdFilesPackagesUpdates.Add(localFileName, "\\Internal storage\\ReconApps\\MapData\\" + mapFilesDest[index]);
                    index++;
                }

                bool isCopyToDevice = true;
                if (isCopyToDevice)
                {
                    CopyFilesToDevice(sdFilesPackagesUpdates, device);
                }
            }
        }

        private void DoBackupOfMapsDb()
        {
            string[] mapFiles = new string[] { "mdmaps.zip", "resortinfo.db" };

            string strDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            CreateRootDir(Environment.ExpandEnvironmentVariables($@"{pathBase}backup\{strDateTime}"));

            foreach (string localFileName in mapFiles)
            {
                try
                {
                    string destFile = "";

                    File.Copy(Environment.ExpandEnvironmentVariables($@"{pathBase}{localFileName}"),
                        Environment.ExpandEnvironmentVariables($@"{pathBase}backup\{strDateTime}\{localFileName}"));
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Exceptio while creating backup {0}", ex.Message);
                }
            }
        }

        private bool AddEntriesToDb(Dictionary<String, MapInfo> dicMaps)
        {
            //string path = @"D:\Downloads\Recon-Oakley\MAPS\temp";
            //string dbpath = Path.Combine(path, "resortinfo.db");
            string localFileName = "resortinfo.db";
            string dbpath = Environment.ExpandEnvironmentVariables($@"{pathBase}{localFileName}");
            bool isAddedToDb = false;

            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                List<MapInfo> lstPackageDownload = new List<MapInfo>();
                foreach (KeyValuePair<String, MapInfo> kvp in dicMaps)
                {
                    //In case we have to delete previously
                    bool isForceUpdate = false;
                    if (isForceUpdate)
                    {
                        SqliteCommand deleteCommand = new SqliteCommand
                            (string.Format("DELETE from resortLocation where id={0}", kvp.Value.id), db);
                        deleteCommand.ExecuteNonQuery();

                        deleteCommand.Dispose();
                    }

                    SqliteCommand selectCommand = new SqliteCommand
                        (string.Format("SELECT * from resortLocation where id={0}", kvp.Value.id), db);

                    SqliteDataReader query = selectCommand.ExecuteReader();

                    if (query.Read())
                    {
                        selectCommand.Dispose();
                        query.Close();
                        continue;
                    }

                    query.Close();
                    selectCommand.Dispose();

                    //To test
                    //isAddedToDb = true;
                    //continue;

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    int id = int.Parse(kvp.Value.id);
                    string name = kvp.Value.name;
                    int resortType = 0;

                    //This is hardcoded: should come from api!!!
                    int countryID = 164;

                    //double hubLatitude = 36.507302;
                    //double hubLongitude = -4.788646;
                    double hubLatitude = int.Parse(kvp.Value.latitude) / 1000000.0d;
                    double hubLongitude = int.Parse(kvp.Value.longitude) / 1000000.0d;

                    double hubAltitude = 0;
                    //double minLatitude = 36.4878;
                    //double minLongitude = -4.8137;
                    //double maxLatitude = 36.5190;
                    //double maxLongitude = -4.7598;

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

                    insertCommand.ExecuteNonQuery();

                    insertCommand.Dispose();

                    isAddedToDb = true;
                }

                db.Close();

                db.Dispose();

                GC.Collect();

                GC.WaitForPendingFinalizers();
            }

            return isAddedToDb;
        }

        /// <summary>
        /// Push all the trips to server: start a thread
        /// </summary>
        /// <param name="sdFiles">Files to upload</param>
        /// <param name="password">Secret for setting bearer token</param>
        void PushAllTripsToSever(SortedDictionary<string, MediaFileInfo> sdFiles, string password)
        {
            if (!m_isTripDataUploaded)
            {
                m_isTripDataUploaded = true;
                var t = new Thread(() => DoPushAllTrips(sdFiles, password));
                t.Start();
            }
        }

        void PushAllUserPackagesToSever(List<PackageInfoExt> lstPackageDevice, string password)
        {
            //if (!m_isTripDataUploaded)
            //{
                m_isTripDataUploaded = true;
                var t = new Thread(() => DoPushAllUserPackagesToServer(lstPackageDevice, password));
                t.Start();
            //}
        }

        void PushAllUserPackagesBinToSever(List<PackageInfoExt> lstPackageDevice, string password)
        {
            m_isTripDataUploaded = true;
            var t = new Thread(() => DoPushAllUserPackagesBinToServer(lstPackageDevice, password));
            t.Start();
        }

        void PushAllTripsBinToSever(List<PackageInfoExt> lstPackageDevice, string password)
        {
            m_isTripDataUploaded = true;
            var t = new Thread(() => DoPushAllTripBinToServer(lstPackageDevice, password));
            t.Start();
        }

        List<PackageInfo> DoPrepareListToDownload(Dictionary<String,PackageInfo> dicPackages, List<PackageInfoExt> lstPackageDevice)
        {
            List<PackageInfo> lstPackageDownload = new List<PackageInfo>();
            foreach (KeyValuePair<String, PackageInfo> kvp in dicPackages)
            {
                PackageInfoExt result = lstPackageDevice.Find(x => x.name == kvp.Key);

                if (result != null)
                {
                    try
                    {
                        int vCodeDevice = Int32.Parse(result.versionCode);
                        int vCodeServer = Int32.Parse(kvp.Value.versionCode);

                        if (vCodeServer > vCodeDevice)
                        {
                            lstPackageDownload.Add(kvp.Value);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    lstPackageDownload.Add(kvp.Value);
                }
            }

            return lstPackageDownload;
        }

        /// <summary>
        /// Compare map date with date assigned to user and download if 1st>2nd
        /// </summary>
        /// <param name="dicMaps"></param>
        /// <param name="lstPackageDevice"></param>
        /// <returns></returns>
        List<MapInfo> DoPrepareMapListToDownload(Dictionary<String, MapInfo> dicMaps, List<MapInfo> lstPackageDevice)
        {
            List<MapInfo> lstPackageDownload = new List<MapInfo>();
            foreach (KeyValuePair<String, MapInfo> kvp in dicMaps)
            {
                /*
                PackageInfoExt result = lstPackageDevice.Find(x => x.name == kvp.Key);

                if (result != null)
                {
                    try
                    {
                        int vCodeDevice = Int32.Parse(result.versionCode);
                        int vCodeServer = Int32.Parse(kvp.Value.versionCode);

                        if (vCodeServer > vCodeDevice)
                        {
                            lstPackageDownload.Add(kvp.Value);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    lstPackageDownload.Add(kvp.Value);
                }
                */

                string[] geom = new string[] { "areas", "lines", "points" };
                string[] ext = new string[] { "dbf", "shp" };

                var combos = CartesianProductSmart(geom, ext);

                foreach (var combo in combos)
                {
                    string fileName = string.Format("{0}/{2}.{1}", combo[0], combo[1], kvp.Value.id);
                    MapInfo mapInfo = new MapInfo(kvp.Value);
                    mapInfo.name = fileName;
                    lstPackageDownload.Add(mapInfo);
                }
            }

            return lstPackageDownload;
        }

        private static string[][] CartesianProductSmart(string[] arr1, string[] arr2)
        {
            // for each s1 in arr1, extract arr2, 
            // then pass s1 and s2 into a newly-made string array.
            return arr1.SelectMany(s1 => arr2, (s1, s2) => new string[] { s1, s2 })
                .ToArray();
        }

        Dictionary<String, PackageInfo> DoPreparePackagesUpdatesFromServer(string secret)
        {
            Dictionary<String, PackageInfo> dicPackages = new Dictionary<string, PackageInfo>();
            List<PackageInfo> lstPackageServer = GetPackageUpdatesFromSever(secret);
            lstPackageServer.ForEach(item =>
            {
                bool isCandidateToDownload;
                try
                {
                    //com.recom3.recom3temperat is the Unblocker app and is important to download
                    isCandidateToDownload = item.name.Trim().ToLower() == "com.recom3.recom3temperat" ||
                        (!string.IsNullOrEmpty(item.id_user) && Int32.Parse(item.id_user) > 0);
                }
                catch(Exception ex)
                {
                    isCandidateToDownload = false;
                }
                //Try to download
                if (isCandidateToDownload)
                {
                    if (dicPackages.ContainsKey(item.name))
                    {
                        int verInDic = 1;
                        try
                        {
                            verInDic = Int32.Parse(dicPackages[item.name].versionCode);
                        }
                        catch (Exception ex)
                        {

                        }
                        int verInLst = 1;
                        try
                        {
                            verInLst = Int32.Parse(item.versionCode);
                        }
                        catch (Exception ex)
                        {

                        }
                        if (verInLst > verInDic)
                        {
                            dicPackages[item.name] = item;
                        }
                    }
                    else
                    {
                        dicPackages.Add(item.name, item);
                    }
                }
            }
            );
            return dicPackages;
        }

        Dictionary<String, MapInfo> DoPrepareMapsUpdatesFromServer(string secret)
        {
            Dictionary<String, MapInfo> dicPackages = new Dictionary<string, MapInfo>();
            List<MapInfo> lstPackageServer = GetMapsUpdatesFromSever(secret);
            lstPackageServer.ForEach(item =>
            {
                if (!dicPackages.ContainsKey(item.name))
                {
                    int asigned = 0;
                    try
                    {
                        asigned = Int32.Parse(item.id_user);
                    }
                    catch (Exception ex)
                    {

                    }
                    if (asigned != 0)
                    {
                        dicPackages.Add(item.id, item);
                    }
                }
            });
            return dicPackages;
        }

        List<PackageInfo> GetPackageUpdatesFromSever(string password)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", password);

            string payload = JsonConvert.SerializeObject(new { });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(urlPackageUpdate, content).Result;

            string jsonString = response.Content.ReadAsStringAsync()
                                                           .Result
                                                           .Replace("\\", "")
                                                           .Trim(new char[1] { '"' });

            List<PackageInfo> vwAisItemMasterList = JsonConvert.DeserializeObject<List<PackageInfo>>(jsonString);

            client.Dispose();

            return vwAisItemMasterList;
        }

        //id,name,dateCreate,latitude,longitudeid_user
        List<MapInfo> GetMapsUpdatesFromSever(string password)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", password);

            string payload = JsonConvert.SerializeObject(new { });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(urlMapsUpdate, content).Result;

            string jsonString = response.Content.ReadAsStringAsync()
                                                           .Result
                                                           .Replace("\\", "")
                                                           .Trim(new char[1] { '"' });

            List<MapInfo> vwAisItemMasterList = JsonConvert.DeserializeObject<List<MapInfo>>(jsonString);

            client.Dispose();

            return vwAisItemMasterList;
        }

        private static bool m_isReady = false;

        void DownloadPackageFromServer(string fileName, string secret)
        {
            string remoteUri = URL;
            string myStringWebResource = null;

            // Create a new WebClient instance.
            using (WebClient myWebClient = new WebClient())
            {
                //myWebClient.Headers.Add("Bearer", secret);
                myWebClient.Headers.Add("Authorization", "Bearer " + secret);

                //myStringWebResource = "https://www.recom3.com/priv/" + fileName;
                //myStringWebResource = URL + "downloadrif";
                //15.09.2023
                myStringWebResource = URL + "packagedownload/" + fileName;

                // Download the Web resource and save it into the current filesystem folder.
                string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{fileName}");
                //myWebClient.DownloadFileCompleted += client_DownloadFileCompleted;

                //myWebClient.DownloadFileAsync(new Uri(myStringWebResource), localName);
                myWebClient.DownloadFile(myStringWebResource, localName);

                m_isReady = true;
            }
        }

        void DownloadUpdatePackageFromServer(string fileName, string secret, BackgroundWorker bgw)
        {
            string myStringWebResource = null;

            // Create a new WebClient instance.
            using (WebClient myWebClient = new WebClient())
            {
                //myWebClient.Headers.Add("Bearer", secret);
                myWebClient.Headers.Add("Authorization", "Bearer " + secret);

                myWebClient.UseDefaultCredentials = false;
                myWebClient.Credentials = new NetworkCredential("recom3updater", "v4aBB6unAa1UI0IouuAm");

                myStringWebResource = CommManager.URL_BASE + "web/update/" + fileName;

                // Download the Web resource and save it into the current filesystem folder.
                string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{fileName}");

                myWebClient.DownloadProgressChanged += (s, e) =>
                {
                    bgw.ReportProgress(e.ProgressPercentage, new ProgressBarReport(ProgressBarReport.BAR_TYPE.FILE_DOWNLOAD, ""));

                };

                myWebClient.DownloadFileCompleted += (s, e) =>
                {
                    m_isReady = true;
                };

                myWebClient.DownloadFileAsync(new Uri(myStringWebResource), localName);
            }
        }

        /// <summary>
        /// Download a map part from server
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="secret"></param>
        public void DownloadMapFromServer(string fileName, string secret)
        {
            string remoteUri = URL;
            string myStringWebResource = null;

            // Create a new WebClient instance.
            using (WebClient myWebClient = new WebClient())
            {
                //myWebClient.Headers.Add("Bearer", secret);
                myWebClient.Headers.Add("Authorization", "Bearer " + secret);

                myStringWebResource = URL + "downloadfile/" + fileName;
                //myStringWebResource = "https://www.recom3.com/priv/" + fileName;

                string localFileName = fileName.Replace("/", "\\");

                // Download the Web resource and save it into the current filesystem folder.
                string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{localFileName}");
                //myWebClient.DownloadFileCompleted += client_DownloadFileCompleted;

                //myWebClient.DownloadFileAsync(new Uri(myStringWebResource), localName);
                myWebClient.DownloadFile(myStringWebResource, localName);

                m_isReady = true;
            }
        }

        public static void AddFilesToZip(string zipPath, List<MapInfo> files)
        {
            if (files == null || files.Count == 0)
            {
                return;
            }

            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                foreach (var file in files)
                {
                    string localName = Environment.ExpandEnvironmentVariables($@"{pathBase}{file.name.Replace("/","\\")}");

                    var fileInfo = new FileInfo(localName);

                    zipArchive.CreateEntryFromFile(localName, file.name.Replace("/", "\\"));
                }
            }
        }

        public static void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Do stuff like comparing the file to another, renaming, copying, etc.
            m_isReady = true;
        }

        string ReadMapInfo(SortedDictionary<string, MediaFileInfo> sdFiles)
        {
            XmlTextReader reader = null;
            string sMaspInfo = "";

            try
            {
                if (sdFiles.Keys.Count >= 1)
                {
                    String fileName = Environment.ExpandEnvironmentVariables(pathBase + sdFiles.Keys.ElementAt(0));

                    // Load the reader with the data file and ignore all white space nodes.
                    reader = new XmlTextReader(fileName);
                    reader.WhitespaceHandling = WhitespaceHandling.None;

                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                //Console.Write("<{0}>", reader.Name);
                                sMaspInfo = reader.GetAttribute("db_version");
                                break;
                            case XmlNodeType.Text:
                                //Console.Write(reader.Value);
                                break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                msg = "";
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return sMaspInfo; 
        }

        class PackageInfo
        {
            public string name;
            public string versionCode;
            public string versionName;
            public string fileName;
            public string id_user;

            public PackageInfo(string name, string versionCode, string versionName, string fileName,
                string id_user)
            {
                this.name = name;
                this.versionCode = versionCode;
                this.versionName = versionName;
                this.fileName = fileName;
                this.id_user = id_user;
            }
        }

        public class PackageInfoExt
        {
            public string name;
            public string versionCode;
            public string versionName;
            public string fileName;
            public string idUnit;
            public List<Track> tracks;

            public PackageInfoExt(string name, string versionCode, string versionName, string fileName, string idUnit)
            {
                this.name = name;
                this.versionCode = versionCode;
                this.versionName = versionName;
                this.fileName = fileName;
                this.idUnit = idUnit;
            }
        }

        public class MapInfo
        {
            public string id;
            public string name;
            public string dateCreate;
            public string latitude;
            public string longitude;
            public string id_user;

            [JsonConstructor]
            public MapInfo(string id, string name, string dateCreate, string latitude, string longitude, string id_user)
            {
                this.id = id;
                this.name = name;
                this.dateCreate = dateCreate;
                this.latitude = latitude;
                this.longitude = longitude;
                this.id_user = id_user;
            }

            public MapInfo(MapInfo mapInfo)
            {
                this.id = mapInfo.id;
                this.name = mapInfo.name;
                this.dateCreate = mapInfo.dateCreate;
                this.latitude = mapInfo.latitude;
                this.longitude = mapInfo.longitude;
                this.id_user = mapInfo.id_user;
            }
        }

        List<PackageInfoExt> ReadPackageInformation(XmlReader reader, string sDevice)
        {
            string name,versionCode,versionName;
            List<PackageInfoExt> lstResult = new List<PackageInfoExt>();

            try
            {
                // if something raise an exception here
                // the main reader is not really affected by that
                // because when the methods ends, the "subReader" is closed
                // and then the main reader is set on the closing tag.
                // so the main XML reader can continue to analyze the xml stream
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            //Console.Write("<{0}>", reader.Name);
                            if (reader.Name.ToLower() == "package")
                            {
                                name = reader.GetAttribute("name");
                                versionCode = reader.GetAttribute("versionCode");
                                versionName = reader.GetAttribute("versionName");
                                lstResult.Add(new PackageInfoExt(name, versionCode, versionName, "", sDevice));
                            }
                            break;
                        case XmlNodeType.Text:
                            //Console.Write(reader.Value);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //...
            }

            return lstResult;
        }

        List<PackageInfoExt> ReadPackagesInfo(SortedDictionary<string, MediaFileInfo> sdFiles, string sDevice)
        {
            XmlTextReader reader = null;
            string sMaspInfo = "";
            List<PackageInfoExt> lstResult = null;

            try
            {
                if (sdFiles.Keys.Count >= 1)
                {
                    String fileName = Environment.ExpandEnvironmentVariables(pathBase + sdFiles.Keys.ElementAt(0));

                    // Load the reader with the data file and ignore all white space nodes.
                    reader = new XmlTextReader(fileName);
                    reader.WhitespaceHandling = WhitespaceHandling.None;

                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                //Console.Write("<{0}>", reader.Name);
                                sMaspInfo = reader.GetAttribute("db_version");
                                if (reader.Name.ToLower() == "installed-packages")
                                {
                                    lstResult=ReadPackageInformation(reader.ReadSubtree(), sDevice);
                                }
                                break;
                            case XmlNodeType.Text:
                                //Console.Write(reader.Value);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                msg = "";
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return lstResult;
        }

        string ReadSerialInfo(SortedDictionary<string, MediaFileInfo> sdFiles)
        {
            XmlTextReader reader = null;
            string sMaspInfo = "";

            try
            {
                if (sdFiles.Keys.Count >= 1)
                {
                    String fileName = Environment.ExpandEnvironmentVariables(pathBase + sdFiles.Keys.ElementAt(0));

                    //String readText = File.ReadAllText(fileName);

                    byte[] myBytes = File.ReadAllBytes(fileName);

                    if(myBytes.Length>=4)
                    {
                        //for(int i=myBytes.Length-4;i<myBytes.Length;i++)
                        //{
                        //    sMaspInfo += String.Format("{0}", myBytes[i]);
                        //}
                        byte[] buffer = new byte[4];
                        Array.Copy(myBytes, myBytes.Length - 4, buffer, 0, 4);
                        Array.Reverse(buffer);
                        uint iSerial = BitConverter.ToUInt32(buffer, 0);
                        sMaspInfo = String.Format("{0}", iSerial);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                msg = "";
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return sMaspInfo;
        }

        public static void DoPushAllTrips(SortedDictionary<string, MediaFileInfo> sdFiles, string password)
        {
            var sortedDictEnum = sdFiles.GetEnumerator();

            while (sortedDictEnum.MoveNext())
            {
                String[] args = new String[1];
                args[0] = Environment.ExpandEnvironmentVariables(pathBase + sortedDictEnum.Current.Key);

                //Convert and upload one trip
                FlightConverter.TrackInfo trackInfo = FlightConverter.MainTripConv(args);
                trackInfo.fileName = sortedDictEnum.Current.Key;

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
                }

                client.Dispose();

                Thread.Sleep(100);
            }
        }

        public static void DoPushAllTrips(List<FileManager.PackageInfoExt> lstPackageDevice, string password)
        {
            foreach (var pkgDevice in lstPackageDevice)
            {
                //String[] args = new String[1];
                //args[0] = Environment.ExpandEnvironmentVariables(pathBase + pkgDevice.name);
                string ribFileName = Environment.ExpandEnvironmentVariables(pathBase + pkgDevice.name);

                //Convert and upload one trip
                FlightConverter.TrackInfo trackInfo = new FlightConverter.TrackInfo();
                List<Track> tracks = FlightConverter.parseFile(ribFileName, ref trackInfo);
                //FlightConverter.TrackInfo trackInfo = FlightConverter.MainTripConv(args);
                //trackInfo.fileName = pkgDevice.name;
                //Change file name for upload to server
                string gpxName = Path.GetFileNameWithoutExtension(pkgDevice.name) + ".gpx";
                trackInfo.fileName = gpxName;
                //Get more information from tracks
                //GisHelper.getCoutry();

                //Write converted file to disk
                string gpxFileName = Environment.ExpandEnvironmentVariables(FileManager.pathBase + gpxName);
                tracks = XMLOutput.writeFile2(gpxFileName, tracks);    

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
                catch(Exception ex)
                {
                    country = "USA";
                    name = string.Format("Trip yyyy-MM-dd HH:mm:ss",
                        DateTime.Now);
                }
                trackInfo.name = name;
                trackInfo.country = country;

                //Need to iterate all the tracks to post n files
                //----------------------------------------------------------------------------------------
                int count = 0;
                foreach (Track t in tracks)
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(URL);

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", password);

                    if(String.IsNullOrEmpty(t.trackInfo.country))
                    {
                        t.trackInfo.country = trackInfo.country;
                    }
                    if (String.IsNullOrEmpty(t.trackInfo.name))
                    {
                        t.trackInfo.name = trackInfo.name;
                    }
                    if(count>0)
                    {
                        t.trackInfo.name = t.trackInfo.name + "_" + count.ToString("D3");
                    }
                    string payload = JsonConvert.SerializeObject(t.trackInfo);

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
                        pkgDevice.name = tripId.id_trip + "_" + t.fileName;
                        pkgDevice.fileName = gpxFileName;

                        t.sourceFileName = Path.GetDirectoryName(gpxFileName) + "\\" + t.fileName;
                        t.fileName = tripId.id_trip + "_" + t.fileName;
                    }

                    client.Dispose();

                    count++;

                    Thread.Sleep(100);
                }

                //------------------------------------------------------------------------------------

                pkgDevice.tracks = tracks;
            }
        }

        /// <summary>
        /// Push packages to server
        /// </summary>
        /// <param name="lstPackageDevice"></param>
        /// <param name="password"></param>
        private static void DoPushAllUserPackagesToServer(List<PackageInfoExt> lstPackageDevice, string password)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", password);

            string payload = JsonConvert.SerializeObject(lstPackageDevice);

            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(urlPackagesUserUpload, content).Result;

            client.Dispose();

            Thread.Sleep(100);
        }

        /// <summary>
        /// Create zip files from apk packages
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="zipFile"></param>
        private bool createZip(string dirPath)
        {
            bool isAnythingProc = false;
            string tempPath = dirPath;
            string zipFile;

            string[] files = Directory.GetFiles(tempPath, "*.apk", SearchOption.AllDirectories);

            foreach (string fPath in files)
            {
                try
                {
                    zipFile = Path.GetFileNameWithoutExtension(fPath) + ".zip";

                    using (ZipArchive zip = ZipFile.Open(tempPath + @"\" + zipFile, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(fPath, Path.GetFileName(fPath));
                    }

                    File.Delete(fPath);

                    isAnythingProc = true;
                }
                catch(Exception ex)
                {

                }
            }

            return isAnythingProc;
        }

        /// <summary>
        /// Push packahges binaries to server
        /// This function is under development
        /// </summary>
        /// <param name="lstPackageDevice"></param>
        /// <param name="password"></param>
        private static void DoPushAllUserPackagesBinToServer(List<PackageInfoExt> lstPackageDevice, string password)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", password);

            //client.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer", password);
            //string payload = JsonConvert.SerializeObject(lstPackageDevice);
            //var content = new StringContent(payload, Encoding.UTF8, "application/json");
            //HttpResponseMessage response = client.PostAsync(urlPackagesUserUpload, content).Result;
            //client.Dispose();

            //--------------------------------------------

            foreach (var pkgDevice in lstPackageDevice)
            {
                var filePath = pkgDevice.fileName;//sourceFileName;

                using (var multipartFormContent = new MultipartFormDataContent())
                {
                    //Load the file and set the file's Content-Type header
                    var fileStreamContent = new StreamContent(File.OpenRead(filePath));
                    fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    //HttpContent stringContent = new StringContent(pkgDevice.idUnit);
                    //multipartFormContent.Add(stringContent, "unit", "unit");

                    //stringContent = new StringContent(pkgDevice.name);
                    //multipartFormContent.Add(stringContent, "name", "name");

                    multipartFormContent.Add(new StringContent(pkgDevice.idUnit), "unit");
                    multipartFormContent.Add(new StringContent(pkgDevice.name), "name");

                    //Add the file
                    multipartFormContent.Add(fileStreamContent, name: "file", fileName: pkgDevice.name);

                    //Send it
                    var response = client.PostAsync("mepackagessupdatebin", multipartFormContent);

                    string formattedJson = JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
                    //Console.WriteLine(formattedJson);
                    //logger.Debug(formattedJson);

                    if (response.Result.StatusCode == HttpStatusCode.OK)
                    {
                        var contents = response.Result.Content.ReadAsStringAsync().Result;
                    }
                }
            }

            client.Dispose();
            //--------------------------------------------

            Thread.Sleep(100);
        }

        /// <summary>
        /// Push packahges binaries to server
        /// This function is under development
        /// </summary>
        /// <param name="lstPackageDevice"></param>
        /// <param name="password"></param>
        public static void DoPushAllTripBinToServer(List<PackageInfoExt> lstPackageDevice, string password)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", password);

            foreach (var pkgDevice in lstPackageDevice)
            {
                foreach (Track t in pkgDevice.tracks)
                {
                    //var filePath = pkgDevice.fileName;
                    var filePath = t.sourceFileName;

                    using (var multipartFormContent = new MultipartFormDataContent())
                    {
                        //Load the file and set the file's Content-Type header
                        var fileStreamContent = new StreamContent(File.OpenRead(filePath));
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                        multipartFormContent.Add(new StringContent(pkgDevice.idUnit), "unit");
                        //multipartFormContent.Add(new StringContent(pkgDevice.name), "name");
                        multipartFormContent.Add(new StringContent(t.fileName), "name");

                        //Add the file
                        //multipartFormContent.Add(fileStreamContent, name: "file", fileName: pkgDevice.name);
                        multipartFormContent.Add(fileStreamContent, name: "file", fileName: t.fileName);

                        //Send it
                        var response = client.PostAsync("metripsupdatebin", multipartFormContent);

                        string formattedJson = JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
                        //Console.WriteLine(formattedJson);
                        //logger.Debug(formattedJson);

                        if (response.Result.StatusCode == HttpStatusCode.OK)
                        {
                            var contents = response.Result.Content.ReadAsStringAsync().Result;
                        }
                    }
                }

                Thread.Sleep(100);
            }

            client.Dispose();
        }

        void WriteSreamToDisk(string filePath, MemoryStream memoryStream)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[memoryStream.Length];
                memoryStream.Read(bytes, 0, (int)memoryStream.Length);
                file.Write(bytes, 0, bytes.Length);
                memoryStream.Close();
            }
        }

        /// <summary>
        /// Copies adb packages from device to make a backup
        /// </summary>
        void CopyPackagesFromDevice(string pathHdd)
        {
            //%LOCALAPPDATA%\Android\sdk\platform-tools\adb.exe pull /system/app/ReconMessageCenter.apk D:/pull

            string unixpathHdd = pathHdd.Replace("\\", "/").Trim();
            //Replace bars at the end
            unixpathHdd = Regex.Replace(unixpathHdd, @"//+$", "");

            string fullPath = @".\adb4win\adb.exe";
            fullPath = Directory.GetCurrentDirectory() + @"\adb.exe";
            //fullPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Android\sdk\platform-tools\adb.exe");
            string args = "pull /system/app D:/pull_test";

            args = "pull /system/app " + "\"" + unixpathHdd + "\"";

            //If the UseShellExecute property is true or the UserName and Password properties are not null, 
            //the CreateNoWindow property value is ignored and a new window is created.

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = fullPath;
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process processTemp = new Process();
            processTemp.StartInfo = startInfo;
            processTemp.EnableRaisingEvents = true;
            try
            {
                //string strCmdText;
                //strCmdText = "/C " + fullPath + " " + args;
                //System.Diagnostics.Process.Start("CMD.exe", strCmdText);
                //cmd.exe /C D:\git\RecomProjects\Recom3Uplnk\Recom3Uplnk\bin\Debug\adb.exe pull /system/app "C:/Users/Chus/AppData/Local/Recon Instruments/283242880"
                //adb: error: failed to get feature set: no devices/ emulators found

                processTemp.Start();
            }
            catch (Exception e)
            {
                //throw;
                string msg = e.Message;
            }
        }

        /// <summary>
        /// Copies adb packages to device to update
        /// </summary>
        void CopyUpdatePckToDevice(string pathHdd)
        {
            //%LOCALAPPDATA%\Android\sdk\platform-tools\adb.exe pull /system/app/ReconMessageCenter.apk D:/pull

            string unixpathHdd = pathHdd.Replace("\\", "/").Trim();
            //Replace bars at the end
            unixpathHdd = Regex.Replace(unixpathHdd, @"//+$", "");

            string fullPath = @".\adb4win\adb.exe";
            fullPath = Directory.GetCurrentDirectory() + @"\adb.exe";
            string args1 = "adb push ReconApps/. /mnt/sdcard/ReconApps";

            args1 = "pull /system/app " + "\"" + unixpathHdd + "\"";
            //If using following line it is copied to /mnt/sdcard/ReconApps/ReconApps
            //args1 = string.Format("push \"{0}/.\" /mnt/sdcard/ReconApps", unixpathHdd);
            //So it is better to do so. Not to create a new subfolder
            args1 = string.Format("push \"{0}/.\" /mnt/sdcard", unixpathHdd);

            //If the UseShellExecute property is true or the UserName and Password properties are not null, 
            //the CreateNoWindow property value is ignored and a new window is created.

            //18.04.2024
            //It seems not doing anything, so CreateNoWindow set to false to check

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = fullPath;
            startInfo.Arguments = args1;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;//false
            startInfo.CreateNoWindow = false;//true;

            Process processTemp = new Process();
            processTemp.StartInfo = startInfo;
            processTemp.EnableRaisingEvents = true;
            try
            {
                var sb = new StringBuilder();

                // hookup the eventhandlers to capture the data that is received
                processTemp.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
                processTemp.ErrorDataReceived += (sender, args) => sb.AppendLine(args.Data);

                string strCmdText = "/C " + fullPath + " " + args1;
                System.Diagnostics.Process.Start("CMD.exe", strCmdText);

                //Try to use this:
                //cmd /C D:\git\RecomProjects\Recom3Uplnk\Recom3Uplnk\bin\Debug\adb.exe push "C:/Users/Chus/AppData/Local/Recon Instruments/Snow2-v3.0-to-v4.5/ReconApps/." /mnt/sdcard/ReconApps

                /*
                processTemp.Start();

                processTemp.BeginOutputReadLine();
                processTemp.BeginErrorReadLine();

                // until we are done
                processTemp.WaitForExit();
                */
            }
            catch (Exception e)
            {
                //throw;
                string msg = e.Message;
            }
        }

        public static async Task<string> Upload(byte[] image)
        {
            using (var client = new HttpClient())
            {
                using (var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new MemoryStream(image)), "bilddatei", "upload.jpg");

                    using (
                       var message =
                           await client.PostAsync("http://www.directupload.net/index.php?mode=upload", content))
                    {
                        var input = await message.Content.ReadAsStringAsync();

                        return !string.IsNullOrWhiteSpace(input) ? Regex.Match(input, @"http://\w*\.directupload\.net/images/\d*/\w*\.[a-z]{3}").Value : null;
                    }
                }
            }
        }

        /// <summary>
        /// This will be used to unzip package update
        /// </summary>
        /// <param name="args"></param>
        public static string UnzipToFolder(string zipPath, string path)
        {
            string result = "";
            string extractPath = path;

            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch(Exception ex)
            {
                Debug.Write("Ex:" + ex.Message.ToString());
                string logPath = Environment.ExpandEnvironmentVariables($@"{pathBase}recom3.log");
                File.WriteAllText(logPath, ex.Message);
            }

            string[] directories = Directory.GetDirectories(path);
            //return (directories != null && directories.Length > 0 ? directories[0] : "");
            if(directories != null && directories.Length > 0)
            {
                for (int i = 0; i < directories.Length; i++)
                {
                    if (directories[i].ToLower().IndexOf("snow2")>=0)
                    {
                        int index = directories[i].LastIndexOf(@"\") + 1;
                        result = directories[i].Substring(index, directories[i].Length-index);
                        break;
                    }
                }
            }
            return result;
        }

        protected static void DownloadAsync(string url)
        {
            WebClient webClient = new WebClient();
            webClient.UseDefaultCredentials = false;
            webClient.Credentials = new NetworkCredential("this.UserName", "this.Password");
            //webClient.Headers.Add(HttpRequestHeader.Cookie, "_gat=1; b46467afcb0b4bf5a47b2c6b22e3d284=mt84peq7u4r0bst72ejs5lb7p6; https://docs.stlucieclerk.com/=1,1; _ga=GA1.2.12049534.1467911267");
            //webClient.DownloadFile(webaddress, localname);

            webClient.DownloadProgressChanged += (s, e) =>
            {
                //progressBar.Value = e.ProgressPercentage;
            };
            webClient.DownloadFileCompleted += (s, e) =>
            {
                //progressBar.Visible = false;
                // any other code to process the file
            };
            webClient.DownloadFileAsync(new Uri("http://example.com/largefile.dat"),
                @"C:\Path\To\Output.dat");
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            baseDir.Delete(true);
        }
    }
}
