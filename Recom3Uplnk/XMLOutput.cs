using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk
{
    class XMLOutput
    {
        static void writeFileHeader(StreamWriter fd)
        {
            fd.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\" ?>\n");
            fd.Write("<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" creator=\"Recon Instruments MOD / Flight HUD\" version=\"1.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd\">\n");
        }

        static void writeTrackHeader(StreamWriter fd, Track t)
        {
            fd.Write("  <trk>\n");
            fd.Write("    <name>Track {0}</name>\n", t.num);
            fd.Write("    <desc>{0:00}-{1:00}-{2:0000}</desc>\n", t.month, t.day, t.year);
            fd.Write("    <trkseg>\n");
        }

        static void writePoint(StreamWriter fd, Track t, TrackPoint pt)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            fd.Write("      <trkpt lat=\"{0}\" lon=\"{1}\">\n", 
                pt.lat.ToString("N6", CultureInfo.GetCultureInfo("en-GB")), 
                pt.lon.ToString("N6", CultureInfo.GetCultureInfo("en-GB")));
            fd.Write("        <ele>{0}</ele>\n", pt.alt);
            fd.Write("        <name>{0} km/h</name>\n", pt.speed.ToString("N1", CultureInfo.GetCultureInfo("en-GB")));
            fd.Write("        <time>{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}Z</time>\n", t.year, t.month, t.day, pt.hour, pt.min, pt.sec);
            fd.Write("      </trkpt>\n");
        }

        static void writeTrackFooter(StreamWriter fd)
        {
            fd.Write("    </trkseg>\n");
            fd.Write("  </trk>\n");
        }

        static void writeFileFooter(StreamWriter fd)
        {
            fd.Write("</gpx>\n");
        }

        public static void writeFile(String filename, List<Track> tracks)// throws IOException
        {
            using (StreamWriter fd = new StreamWriter(filename))
            {
                writeFileHeader(fd);
                foreach (Track t in tracks)
                {
                    writeTrackHeader(fd, t);

                    foreach (TrackPoint pt in t.points)
                        writePoint(fd, t, pt);

                    writeTrackFooter(fd);
                }
                writeFileFooter(fd);

                fd.Flush();
            }
	    }

        public static List<Track> writeFile2(String filename, List<Track> tracks)// throws IOException
        {
            String fName = Path.GetFileName(filename);
            String fNameWithPath;
            List<Track> outTracks = new List<Track>();

            int limitCounter = 0;
            int counter = 0;
            foreach (Track t in tracks)
            {
                if (t.points.Count >= 10)
                {
                    if(limitCounter>0 && counter>=limitCounter)
                    {
                        break;
                    }
                    if (counter > 0)
                    {
                        fName = $"{Path.GetFileNameWithoutExtension(filename)}_{t.num:D3}{Path.GetExtension(filename)}";
                    }
                    t.fileName = fName;

                    Track outT = new Track(t.month,t.day,t.year,t.num);
                    outT.points = t.points;
                    outT.fileName = t.fileName;
                    outT.trackInfo = t.trackInfo;
                    outT.trackInfo.fileName = t.fileName;
                    outTracks.Add(outT);

                    fNameWithPath = Path.GetDirectoryName(filename) + "\\" + fName;
                    using (StreamWriter fd = new StreamWriter(fNameWithPath))
                    {
                        writeFileHeader(fd);

                        writeTrackHeader(fd, t);

                        foreach (TrackPoint pt in t.points)
                            writePoint(fd, t, pt);

                        writeTrackFooter(fd);

                        writeFileFooter(fd);

                        fd.Flush();

                        counter++;
                    }
                }
                else
                {
                    Console.WriteLine(String.Format("Track {0} with {1} points", t.num,t.points.Count));
                }
            }
            return outTracks;
        }
    }
}
