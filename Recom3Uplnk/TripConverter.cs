// Include namespace system
using System;
using System.Collections.Generic;

using System.IO;

namespace Recom3Uplnk
{
    public enum TRACK_FORMAT { SNOW2_JET = 1, TRANSCEND = 2, JET0 = 3 };

    public class TrackPoint
    {
        public double lat;
        public double lon;
        public double speed;
        public int alt;
        public int hour;
        public int min;
        public int sec;
        public TrackPoint(double lat, double lon, int alt, double speed, int hour, int min, int sec)
        {
            this.lat = lat;
            this.lon = lon;
            this.speed = speed;
            this.alt = alt;
            this.hour = hour;
            this.min = min;
            this.sec = sec;
        }
        public String toString()
        {
            return String.Format("{0}:{1}:{2}  speed: %.1f km/h  alt: {3}   %.6f  %.6f", this.hour, this.min, this.sec, this.speed, this.alt, this.lat, this.lon);
        }
    }
    public class Track
    {
        public int day;
        public int month;
        public int year;
        public int num;
        public List<TrackPoint> points = new List<TrackPoint>();
        public String fileName;
        public String sourceFileName;
        public FlightConverter.TrackInfo trackInfo;

        public Track(int num)
        {
            this.num = num;
        }

        public Track(int month, int day, int year, int num)
        {
            this.day = day;
            this.month = month;
            this.year = year;
            this.num = num;
        }
        public String toString()
        {
            return "Track " + Convert.ToString(this.num);
        }
    }

    public class FlightConverter
    {
        public class TrackInfo
        {
            public double totDist = 0;
            public double totDiff = 0;
            public double maxSpeed = 0;
            public double totVert = 0;

            public double maxDist = 0;
            public double trackDist = 0;
            public double maxVert = 0;
            public double trackVert = 0;

            public double maxAlt = -9999;
            public double minAlt = 9999;

            public string fileName = "";

            //Example: Trip 2023-03-01 17:01
            public string name = "";
            //Example: USA, Spain...
            public string country = "";

            public TrackInfo()
            {

            }

            public TrackInfo(TrackInfo other)
            {
                this.totDist = other.totDist;
                this.totDiff = other.totDiff;
                this.maxSpeed = other.maxSpeed;
                this.totVert = other.totVert;

                this.maxDist = other.maxDist;
                this.trackDist = other.trackDist;
                this.maxVert = other.maxVert;
                this.trackVert = other.trackVert;

                this.maxAlt = other.maxAlt;
                this.minAlt = other.minAlt;

                this.fileName = other.fileName;
                this.name = other.name;
                this.country = other.country;
            }

            public void resetTrackCounters()
            {
                this.maxDist = Math.Max(this.maxDist, this.trackDist);
                this.trackDist = 0;
                this.maxVert = Math.Max(this.maxVert, this.trackVert);
                this.trackVert = 0;

                this.totDist = 0;
                this.totDiff = 0;
                this.totVert = 0;

                this.maxDist = 0;
                this.maxVert = 0;
            }
        }

        public static double decodeCoordinate(byte[] coord, int offset)
        {
            int b0;
            int b1;
            int b2;
            int b3;
            double deg;
            double min;
            b0 = coord[offset + 0] & 255;
            b1 = coord[offset + 1] & 255;
            b2 = coord[offset + 2] & 255;
            b3 = coord[offset + 3] & 255;
            deg = b0;
            min = (b1 & 127) + b2 * 0.01F + b3 * 1.0E-4F;
            if ((b1 & 128) != 0)
            {
                return -(deg + (min / 60.0F));
            }
            else
            {
                return deg + (min / 60.0F);
            }
        }
        public static int decodeHalfword(byte[] b, int offset)
        {
            // Who the hell uses signed bytes, anyway?
            if ((b[offset] & 128) != 0)
            {
                byte b1 = b[offset];
                byte b2 = b[offset + 1];
                int result = (((int)b[offset]) & 0x127) * 256 + (((int)b[offset + 1]) & 255);

                byte[] temp_arr = new byte[2];
                Array.Copy(b, offset, temp_arr, 0, 2);
                Array.Reverse(temp_arr);

                short converted = BitConverter.ToInt16(temp_arr, 0);
                return converted;
            }
            else
            {
                return (((int)b[offset]) & 255) * 256 + (((int)b[offset + 1]) & 255);
            }
        }
        public static List<Track> parseFile(String filename, ref TrackInfo trackInfo)
        {
            int mode = 2;
            int offset = 0;
            int[] lenHeader = { 14, 9, 13 };
            int[] lenEntry = { 32, 20, 28 };

            var tracks = new List<Track>();
            //var inputFile = new File(filename);
            //var input = new FileStream(inputFile);
            var input = File.OpenRead(filename);
            byte[] header = new byte[9];
            if (input.Read(header, 0, header.Length) != 9)
            {
                throw new Exception("Error reading header;");
            }

            if(header[0] == 0x0d)
            {
                //Mode 1 else mode 2
                mode = (int)TRACK_FORMAT.SNOW2_JET;
                offset = 4;

                byte[] headerRest = new byte[lenHeader[mode-1] - lenHeader[2-1]];
                if (input.Read(headerRest, 0, headerRest.Length) != headerRest.Length)
                {
                    throw new Exception("Error reading header mode 1;");
                }
            }
            else if (header[0] == 0x0c)
            {
                //Mode 1 else mode 2
                mode = (int)TRACK_FORMAT.JET0;
                offset = 0;

                byte[] headerRest = new byte[lenHeader[mode - 1] - lenHeader[2 - 1]];
                if (input.Read(headerRest, 0, headerRest.Length) != headerRest.Length)
                {
                    throw new Exception("Error reading header mode 3;");
                }
            }
            byte[] log_entry = new byte[lenEntry[mode-1]];

            int len;
            var n_track = 0;
            Track curTrack = null;
            TrackPoint ptPrev = null;

            //Parameters to store in web
            //-------------------------------------------------------------------------------------
            //Put all this in a class
            //TrackInfo trackInfo = new TrackInfo();

            //Smooth control (put in a different class)
            int indexPoint = 0;
            List<TrackPoint> lstPointCache = new List<TrackPoint>();

            int lenLstCache = 50;
            double acuVert = 0;
            double avgAlt = 0;
            double avgAltPrev = -9999;
            //-------------------------------------------------------------------------------------

            do
            {
                len = input.Read(log_entry, 0, log_entry.Length);
                if (len != lenEntry[mode-1])
                {
                    break;
                }
                // Timestamp upper bit indicates new track
                if (mode==(int)TRACK_FORMAT.TRANSCEND && (log_entry[0] & 128) != 0)
                {
                    int day;
                    int month;
                    int year;
                    n_track++;
                    Console.WriteLine("Reading track " + n_track.ToString());
                    year = 2000 + (log_entry[0] & 127);
                    month = log_entry[1];
                    day = log_entry[2];

                    curTrack = new Track(month, day, year, n_track);
                    tracks.Add(curTrack);

                    //Restart per track variables and counters
                    if (tracks.Count >= 2)
                    {
                        tracks[tracks.Count - 2].trackInfo = new TrackInfo(trackInfo);
                    }
                    trackInfo.resetTrackCounters();

                    //Smooth vert
                    indexPoint = 0;
                    lstPointCache.Clear();
                    acuVert = 0;
                    avgAlt = 0;
                    avgAltPrev = -9999;
                    continue;
                }
                else if (mode == (int)TRACK_FORMAT.JET0 && (log_entry[0] & 128) != 0)
                {
                    int day;
                    int month;
                    int year;
                    n_track++;
                    Console.WriteLine("Reading track " + n_track.ToString());
                    year = 2000 + (log_entry[0] & 127);
                    month = log_entry[1];
                    day = log_entry[2];

                    curTrack = new Track(month, day, year, n_track);
                    tracks.Add(curTrack);

                    //Restart per track variables and counters
                    if (tracks.Count >= 2)
                    {
                        tracks[tracks.Count - 2].trackInfo = new TrackInfo(trackInfo);
                    }
                    trackInfo.resetTrackCounters();

                    //Smooth vert
                    indexPoint = 0;
                    lstPointCache.Clear();
                    acuVert = 0;
                    avgAlt = 0;
                    avgAltPrev = -9999;
                    continue;
                }
                else if(mode == (int)TRACK_FORMAT.SNOW2_JET && (indexPoint==0|| log_entry[14] == 0x0d))
                {
                    int day;
                    int month;
                    int year;
                    n_track++;
                    Console.WriteLine("Reading track " + n_track.ToString());
                    year = 2000 + (log_entry[0] & 127);
                    month = log_entry[1];
                    day = log_entry[2];

                    bool isSameTrack = false;
                    if (ptPrev != null)
                    {
                        int hour1 = 0;// log_entry[0 + offset];
                        int min1 = log_entry[1 + offset];
                        int sec1 = log_entry[2 + offset];

                        TimeSpan time1 = new TimeSpan(hour1, min1, sec1);
                        TimeSpan time2 = new TimeSpan(0/*ptPrev.hour*/, ptPrev.min, ptPrev.sec);

                        // Calculate the difference
                        TimeSpan difference = time1 - time2;
                        double secondsDifference = difference.TotalSeconds;

                        string formatted = $"Current: {time1.Hours:D2}:{time1.Minutes:D2}:{time1.Seconds:D2}";
                        Console.WriteLine(formatted);
                        formatted = $"Previous: {time2.Hours:D2}:{time2.Minutes:D2}:{time2.Seconds:D2}";
                        Console.WriteLine(formatted);
                        Console.WriteLine($"Diff: {secondsDifference:F2}");

                        if(secondsDifference<=10)
                        {
                            isSameTrack = true;
                        }
                        else
                        {
                            isSameTrack = false;
                        }
                    }

                    if (!isSameTrack)
                    {
                        curTrack = new Track(month, day, year, n_track);
                        tracks.Add(curTrack);

                        //Restart per track variables and counters
                        if (tracks.Count >= 2)
                        {
                            tracks[tracks.Count - 2].trackInfo = new TrackInfo(trackInfo);
                        }
                        trackInfo.resetTrackCounters();

                        //Smooth vert
                        indexPoint = 0;
                        lstPointCache.Clear();
                        acuVert = 0;
                        avgAlt = 0;
                        avgAltPrev = -9999;
                    }
                }

                int hour;
                int min;
                int sec;
                int alt;
                double lat;
                double lon;
                double speed;

                hour = log_entry[0 + offset];
                min = log_entry[1 + offset];
                sec = log_entry[2 + offset];

                if (mode == (int)TRACK_FORMAT.SNOW2_JET)
                {
                    byte[] byteA = new byte[4];
                    Array.Copy(log_entry, byteA, byteA.Length);
                    Array.Reverse(byteA);

                    int iData = BitConverter.ToInt32(byteA, 0);

                    DateTime dt = UnixTimeStampToDateTime(iData);

                    if(indexPoint==0)
                    {
                        curTrack.year = dt.Year;
                        curTrack.month = dt.Month;
                        curTrack.day = dt.Day;
                        tracks[tracks.Count - 1] = curTrack;
                    }

                    hour = dt.Hour;
                    min = dt.Minute;
                    sec = dt.Second;
                }

                lat = FlightConverter.decodeCoordinate(log_entry, 3 + offset);
                lon = FlightConverter.decodeCoordinate(log_entry, 7 + offset);
                speed = FlightConverter.decodeHalfword(log_entry, 11 + offset) * 0.1F;
                alt = FlightConverter.decodeHalfword(log_entry, 13 + offset);

                byte[] temp_barr = new byte[4];
                Array.Copy(log_entry, 13 + offset, temp_barr, 0, temp_barr.Length);
                string s = BitConverter.ToString(temp_barr).Replace("-", " ");
                Console.WriteLine("{0}", s);

                if (alt > 4000)
                {
                    Console.WriteLine("Pt={0};Alt={1};hour={2}", indexPoint, alt, hour);
                }

                int temp_maybe = log_entry[17 + offset];// - 40;

                trackInfo.maxSpeed = Math.Max(speed, trackInfo.maxSpeed);

                TrackPoint pt = new TrackPoint(lat, lon, alt, speed, hour, min, sec);
                if (curTrack == null)
                {
                    throw new Exception("No current track - malformed file?");
                }

                //Prev point
                if (ptPrev != null)
                {
                    double dist = Math.Abs(GisHelper.Haversine(ptPrev.lat, ptPrev.lon, lat, lon));
                    double diff = 0;
                    if (pt.hour == ptPrev.hour)
                    {
                        if (pt.min == ptPrev.min)
                        {
                            diff = Math.Abs(pt.sec - ptPrev.sec);
                        }
                        else
                        {
                            diff = Math.Abs(pt.min - ptPrev.min - 1) * 60 + Math.Abs(60 - ptPrev.sec) + pt.sec;
                        }
                    }
                    else
                    {
                        diff = Math.Abs(60 - ptPrev.min - 1) * 60 + Math.Abs(60 - ptPrev.sec) + pt.sec;
                    }

                    trackInfo.totDist += dist;
                    trackInfo.trackDist += dist;
                    trackInfo.totDiff += diff;

                    //We are using intead a smoothing function
                    //totVert += Math.Abs(pt.alt - ptPrev.alt);
                    //trackVert += Math.Abs(pt.alt - ptPrev.alt);
                }
                ptPrev = pt;

                trackInfo.maxAlt = Math.Max(trackInfo.maxAlt, pt.alt);
                trackInfo.minAlt = Math.Min(trackInfo.minAlt, pt.alt);

                //Smooth vertical altitude
                //---------------------------------------------------------------------------------
                lstPointCache.Add(pt);
                
                if(lstPointCache.Count>lenLstCache)
                {
                    if(indexPoint%lenLstCache==0)
                    {
                        avgAlt = acuVert / lenLstCache;
                        if (avgAltPrev != -9999)
                        {
                            trackInfo.trackVert += Math.Abs(avgAlt - avgAltPrev);
                            trackInfo.totVert += Math.Abs(avgAlt - avgAltPrev); ;
                        }

                        avgAltPrev = avgAlt;
                    }

                    acuVert -= lstPointCache[0].alt;
                    lstPointCache.RemoveAt(0);
                }

                acuVert += pt.alt;
                //---------------------------------------------------------------------------------

                curTrack.points.Add(pt);

                indexPoint++;
            } while (len == lenEntry[mode - 1]);

            double kmhAvgSpeed = trackInfo.totDist / trackInfo.totDiff * 3600.0d / 1000.0d;
            trackInfo.maxDist = Math.Max(trackInfo.maxDist, trackInfo.trackDist);
            trackInfo.maxVert = Math.Max(trackInfo.maxVert, trackInfo.trackVert);

            if (tracks.Count != 0)
            {
                tracks[tracks.Count - 1].trackInfo = new TrackInfo(trackInfo);
                return tracks;
            }
            else
            {
                return null;
            }
        }

        public static TrackInfo MainTripConv(String[] args)
        {
            TrackInfo trackInfo = new TrackInfo();

            Console.WriteLine("\nRIB to GPX converter, version 0.1");
            Console.WriteLine("This program is extremely beta - please be gentle");
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: FlightConverter.jar <rib file> [more rib files...]");
                Environment.Exit(-1);
            }
            foreach (String inFile in args)
            {
                Console.WriteLine();
                Console.WriteLine("Reading " + inFile);
                List<Track> tracks = null;
                try
                {
                    tracks = FlightConverter.parseFile(inFile, ref trackInfo);
                }
                catch (Exception e)
                {
                    //e.printStackTrace();
                    Console.Write(e.Message);
                }
                if (tracks != null)
                {
                    var outFile = inFile + ".gpx";
                    Console.WriteLine("Writing " + outFile);
                    //XMLOutput.writeFile(outFile, tracks);
                }
                else
                {
                    Console.WriteLine("No tracks found in " + inFile);
                }
            }
            Console.WriteLine("Done");

            return trackInfo;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}