using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Recom3Uplnk.Maps
{
    class OsmXmlReader
    {
        public static double m_dLon0 = 0.0d;
        public static double m_dLat0 = 0.0d;

        //char chWaterWay = (char)29;
        //Test with LINE_NATIONAL_BORDER=17 in recon white->not working
        //char chWaterWay = (char)17;
        char chWaterWay = (char)0x08;

        //This is not working: blank tile in map
        //char chResidential = (char)26;
        //char chFootway = (char)26;
        char chResidential = (char)0x0b;//(char)0x12;//Gives problems
        char chFootway = (char)0x0b;//(char)0x09;//Too thick
        char chArteryPrimary = (char)0x08;
        char chArterySecondary = (char)0x09;
        char chArteryTertiary = (char)0x0a;

        Dictionary<string, Dictionary<string, BaseDataTypes>> DictBaseDataTypes;

        public class Way
        {
            public enum WayType { Highway, Waterway, Power, Lanes, Area, Building, Landuse, Leisure, Golf,
                amenity, natural, tourism, highway_motorway, trunk, trunk_link
            };
            public Way(string iId, string n)
            {
                nodes = new List<Node>();
                id = iId;
                name = n;
                wayType = WayType.Highway;
            }

            public Way(Way way)
            {
                id = way.id;
                name = way.name;
                wayType = way.wayType;
                typeHighway = way.typeHighway;
                baseDataType = way.baseDataType;
                nodes = new List<Node>(way.nodes.Count);
                way.nodes.ForEach((item) =>
                {
                    nodes.Add(new Node(item));
                });
            }

            public string id;
            public string name;
            public List<Node> nodes;
            public RectXY boundingBox = null;
            public WayType wayType;
            public string typeHighway = "";
            public BaseDataTypes baseDataType;

            public RectXY getBoundingBox()
            {
                if (boundingBox == null)
                {
                    float minLon = float.MaxValue, maxLon = float.MinValue, minLat = float.MaxValue, maxLat = float.MinValue;
                    foreach (Node node in nodes)
                    {
                        minLon = Math.Min(minLon, Convert.ToSingle(node.lon));
                        minLat = Math.Min(minLat, Convert.ToSingle(node.lat));
                        maxLon = Math.Max(maxLon, Convert.ToSingle(node.lon));
                        maxLat = Math.Max(maxLat, Convert.ToSingle(node.lat));
                    }
                    boundingBox = new RectXY(minLon, maxLat, maxLon, minLat);
                }
                return boundingBox;
            }
        }

        public class Node
        {
            public Node(string sId, double lt, double lo)
            {
                id = sId;
                lat = lt;
                lon = lo;
            }

            public Node(Node node)
            {
                id = node.id;
                lat = node.lat;
                lon = node.lon;
                name = node.name;
            }

            public string id;
            public double lat;
            public double lon;
            public string name;
        }

        public Dictionary<string, Node> nodes;
        public Dictionary<string, Way> ways;

        public static string NodeToString(System.Xml.XmlNode node, int indentation)
        {
            using (var sw = new System.IO.StringWriter())
            {
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    xw.Indentation = indentation;
                    node.WriteContentTo(xw);
                }
                return sw.ToString();
            }
        }

        public BaseDataTypes AssignBaseDataType(string k, string v)
        {
            BaseDataTypes bdtResult = BaseDataTypes.AREA_LAND;
            if (DictBaseDataTypes.ContainsKey(k))
            {
                if (DictBaseDataTypes[k].ContainsKey(v))
                {
                    bdtResult = DictBaseDataTypes[k][v];
                }
                else if (DictBaseDataTypes[k].ContainsKey(""))
                {
                    bdtResult = DictBaseDataTypes[k][""];
                }
            }
            else if (DictBaseDataTypes.ContainsKey(""))
            {
                if (DictBaseDataTypes[""].ContainsKey(""))
                {
                    bdtResult = DictBaseDataTypes[k][""];
                }
            }
            return bdtResult;
        }

        /// <summary>
        /// Load dictionary from disk
        /// Dictionary key: from OSM
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, BaseDataTypes>> readMappingTable()
        {
            string fileName = "osm_mapping_table.csv";
            string fullPath;
            fullPath = Directory.GetCurrentDirectory() + @"\" + fileName;
            Dictionary<string,Dictionary<string, BaseDataTypes>> myDict = new Dictionary<string, Dictionary<string, BaseDataTypes>>();

            if (File.Exists(fullPath))
            {
                const Int32 BufferSize = 128;
                using (var fileStream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    int counter = 0;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (counter >= 1)
                        {
                            try
                            {
                                string[] arr = line.Split(';');

                                for (int i = 0; i < arr.Length; i++)
                                {
                                    arr[i] = arr[i].Trim();
                                }

                                int posK = 0;
                                int posV = 1;
                                int posBase = 2;

                                Enum.TryParse(arr[posBase], out BaseDataTypes baseDataTypes);

                                if (!myDict.ContainsKey(arr[posK]))
                                {
                                    myDict.Add(arr[posK], new Dictionary<string, BaseDataTypes>());
                                }
                                if (!myDict[arr[posK]].ContainsKey(arr[posV]))
                                {
                                    myDict[arr[posK]].Add(arr[posV], baseDataTypes);
                                }
                                else
                                {
                                    myDict[arr[posK]][arr[posV]] = baseDataTypes;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        counter++;
                    }
                }
            }

            return myDict;
        }

        /// <summary>
        /// Read an osm file parsing it
        /// </summary>
        /// <param name="fullFileName">Full filename with path</param>
        /// <param name="rect">Output bounding region</param>
        public void readFile(String fullFileName, ref RectXY rect, double lonOffset0, double latOffset0)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(fullFileName);

            //string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            //if (xml.StartsWith(_byteOrderMarkUtf8))
            //{
            //    xml = xml.Remove(0, _byteOrderMarkUtf8.Length);
            //}

            nodes = new Dictionary<string, Node>();

            //1st Iteration
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                //Loop nodes (node tag)
                string text = node.InnerText; //or loop through its children as well

                if (node.Name.Trim().ToLower() == "node")
                {
                    string sLat = node.Attributes["lat"].Value;
                    string sLon = node.Attributes["lon"].Value;

                    if (node.HasChildNodes)
                    {
                        foreach (XmlNode tag in node.ChildNodes)
                        {
                            if (tag.Name.Trim().ToLower() == "tag"
                                && tag.Attributes["k"].Value.Trim().ToLower() == "name")
                            {
                                text = tag.Attributes["v"].Value;
                            }
                        }
                    }

                    Node nodeObj = new Node(node.Attributes["id"].Value, Double.Parse(sLat, CultureInfo.InvariantCulture) + latOffset0, Double.Parse(sLon, CultureInfo.InvariantCulture) + lonOffset0);
                    nodeObj.name = text;
                    nodes.Add(node.Attributes["id"].Value, nodeObj);
                }
                else if (node.Name.Trim().ToLower() == "bounds")
                {
                    //<bounds minlat="36.4878000" minlon="-4.8137000" maxlat="36.5190000" maxlon="-4.7598000"/>

                    string minlat = node.Attributes["minlat"].Value;
                    string minlon = node.Attributes["minlon"].Value;
                    string maxlat = node.Attributes["maxlat"].Value;
                    string maxlon = node.Attributes["maxlon"].Value;

                    CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");

                    rect = new RectXY(float.Parse(minlon, ci) + (float)lonOffset0, float.Parse(maxlat, ci) + (float)latOffset0,
                        float.Parse(maxlon, ci) + (float)lonOffset0, float.Parse(minlat, ci) + (float)latOffset0);
                }
            }

            ways = new Dictionary<string, Way>();
            bool isDebugWayTypes = true;
            StringBuilder sb = new StringBuilder();

            //2nd Iteration

            DictBaseDataTypes = readMappingTable();

            //We have to write all the ways to file
            //< way id = "23359742" visible = "true" version = "15" changeset = "114932546" timestamp = "2021-12-14T17:10:55Z" user = "AlvaroRC" uid = "3538570" >
            //              < nd ref= "13842448" />
            //               < nd ref= "5596850356" />
            //(...)
            //< nd ref= "1833489382" />
            // < tag k = "highway" v = "motorway" />
            // < tag k = "int_ref" v = "E 15" />
            // < tag k = "name" v = "Autopista del Mediterráneo" />
            // < tag k = "old_ref" v = "A-7" />
            // < tag k = "oneway" v = "yes" />
            // < tag k = "ref" v = "AP-7" />
            // < tag k = "source" v = "PNOA;datos oficiales" />
            // < tag k = "toll" v = "yes" />
            // </ way >
            Dictionary<string, string> dictHighway = new Dictionary<string, string>();
            Dictionary<string, string> dictTotal = new Dictionary<string, string>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                //Loop nodes (node tag)
                string text = node.InnerText; //or loop through its children as well
                string firstChild = "";

                if (node.Name.Trim().ToLower() == "way")
                {
                    try
                    {
                        string s = node.OuterXml;
                        //For example:
                        /*
                        <way id="19380658" visible="true" version="7" changeset="118379665" timestamp="2022-03-11T21:01:45Z" user="Mrvn9" uid="10342822">
	                        <nd ref="201522366" />
	                        <nd ref="201522368" />
	                        <nd ref="5667300897" />
	                        <nd ref="201522369" />
	                        <nd ref="201522370" />
	                        <nd ref="5596850449" />
	                        <nd ref="201523061" />
	                        <nd ref="201522366" />
	                        <tag k="highway" v="residential" />
	                        <tag k="junction" v="roundabout" />
                        </way>
                        */
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    if (node.HasChildNodes)
                    {
                        Way wayObj = new Way(node.Attributes["id"].Value, "");

                        if(wayObj.id == "26952869")
                        {
                            int stopHere = 1;
                        }

                        bool isHighway = false;
                        bool isFiltered = false;
                        bool isLanduse = false;

                        foreach (XmlNode tag in node.ChildNodes)
                        {

                            if (firstChild == "")
                            {
                                if (tag.Name.Trim().ToLower() == "nd")
                                {
                                    firstChild = tag.Attributes["ref"].Value;
                                }
                                else
                                {
                                    firstChild = tag.Name;
                                }
                            }

                            if (tag.Name.Trim().ToLower() == "nd")
                            {
                                string myRef = tag.Attributes["ref"].Value;

                                //Look in nodes dictionary
                                if (nodes.ContainsKey(myRef))
                                {
                                    wayObj.nodes.Add(nodes[myRef]);
                                }
                            }
                            else if (tag.Name.Trim().ToLower() == "tag")
                            {
                                if (tag.Name.Trim().ToLower() == "tag"
                                    && tag.Attributes["k"].Value.Trim().ToLower() == "waterway")
                                {
                                    int stopHere = 1;
                                }

                                //Assign based on dictionary
                                string k = tag.Attributes["k"].Value.Trim().ToLower();

                                //if (tag.Attributes["k"].Value.Trim().ToLower() == "highway")
                                if (DictBaseDataTypes.ContainsKey(k))
                                {
                                    if (k == "highway")
                                    {
                                        isHighway = true;
                                    }
                                    string v = tag.Attributes["v"] != null ? tag.Attributes["v"].Value.Trim().ToLower() : "";

                                    //Sample motorway values
                                    /*
                                    Hightway type: footway
                                    Hightway type: living_street
                                    Hightway type: motorway//primary
                                    Hightway type: path
                                    Hightway type: pedestrian
                                    Hightway type: residential
                                    Hightway type: service
                                    Hightway type: steps
                                    Hightway type: track
                                    Hightway type: trunk//secondary
                                    Hightway type: trunk_link//terciarz
                                    Hightway type: unclassified
                                    */

                                    //Assign using dictionary
                                    if (DictBaseDataTypes[k].ContainsKey(v))
                                    {
                                        wayObj.baseDataType = DictBaseDataTypes[k][v];
                                        //System.Diagnostics.Debug.WriteLine(string.Format("Dictionary contains k:{0} v:{1}", k, v));
                                    }
                                    else if (DictBaseDataTypes[k].ContainsKey(""))
                                    {
                                        wayObj.baseDataType = DictBaseDataTypes[k][""];
                                        //System.Diagnostics.Debug.WriteLine(string.Format("Dictionary not contains k:{0} v:{1}, assigned default for {0}", k, v));
                                    }
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "name")
                                {
                                    text = tag.Attributes["v"].Value;
                                    if (text.ToLower().IndexOf("zaragoza") >= 0)
                                    {
                                        int stopHere = 1;
                                    }
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "power")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.Power;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "building")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.Building;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "area")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.Area;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "landuse")
                                {
                                    isFiltered = true;
                                    isLanduse = true;
                                    wayObj.wayType = Way.WayType.Landuse;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "leisure")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.Leisure;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "golf")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.Golf;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "parking")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.Golf;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "amenity")
                                {
                                    //For example:
                                    /*
                                     * <tag k="amenity" v="restaurant"/>
                                     */
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.amenity;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "natural")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.natural;
                                }
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "tourism")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.tourism;
                                }
                                //-----------------------------------------------------------------------------------------
                                //Why filtering lanes?
                                /*
                                else if (tag.Attributes["k"].Value.Trim().ToLower() == "lanes")
                                {
                                    isFiltered = true;
                                    wayObj.wayType = Way.WayType.Lanes;
                                }
                                */
                                //Control for unknown tags
                                else if (tag.Attributes["k"].Value.Trim().ToLower() != "highway")
                                {
                                    string key = tag.Attributes["k"].Value.Trim().ToLower();
                                    string val = tag.Attributes["v"].Value.Trim().ToLower();
                                    if (!dictTotal.ContainsKey(key))
                                    {
                                        dictTotal.Add(key, val);
                                    }
                                    else
                                    {
                                        bool isAddEmpty = true;
                                        if (isAddEmpty || text != "")
                                        {
                                            if (dictTotal[key].Length > 0) dictTotal[key] += ",";
                                            dictTotal[key] += val;
                                        }
                                    }
                                }

                            }
                        }

                        //Debug code
                        foreach (XmlNode tag in node.ChildNodes)
                        {
                            if (tag.Name.Trim().ToLower() == "tag"
                                && tag.Attributes["k"].Value.Trim().ToLower() == "highway")
                            {
                                string v = tag.Attributes["v"].Value.Trim().ToLower();
                                wayObj.typeHighway = v;

                                if(wayObj.baseDataType==default(BaseDataTypes))
                                {
                                    string s = v;
                                    System.Diagnostics.Debug.WriteLine(string.Format("Hightway type: {0}", s));
                                }

                                if (!dictHighway.ContainsKey(v))
                                {
                                    dictHighway.Add(v, text);
                                }
                                else
                                {
                                    bool isAddEmpty = true;
                                    if (isAddEmpty || text != "")
                                    {
                                        if (dictHighway[v].Length > 0) dictHighway[v] += ",";
                                        dictHighway[v] += text;
                                    }
                                }
                            }
                        }

                        //Is highway
                        if (!isHighway && !isFiltered)
                        {
                            if (isDebugWayTypes)
                            {
                                sb.Append(node.InnerXml);
                                sb.Append(@"\r\n");
                            }
                        }

                        if (text != "")
                        {
                            wayObj.name = text;
                        }
                        else
                        {
                            //wayObj.name = firstChild;
                        }

                        //Allowing only highways appear extrange white squares in recon jet glasses
                        //if (isHighway)
                        //{
                        if(!isLanduse && wayObj.wayType != Way.WayType.Power)
                            ways.Add(node.Attributes["id"].Value, wayObj);
                        //}

                    }//if node has children
                }
            }

            //Test
            var asString = string.Join(Environment.NewLine, dictTotal);

            string str = sb.ToString();

            int end = 1;
        }

        public void WriteWaysToDisk(String fileName, double lonOffset0, double latOffset0, List<Way> lstWays)
        {
            using (var stream = File.Open(fileName, FileMode.Create))
            {
                using (var writer = new BinaryWriter2(stream, Encoding.UTF8, false))
                {
                    short lenTot = (short)lstWays.Count;
                    //byte[] bytes = BitConverter.GetBytes(lenTot);
                    //Array.Reverse(bytes);
                    writer.Write(lenTot);

                    foreach (Way entry in lstWays)
                    {
                        if (entry.nodes.Count >= 2)
                        {
                            bool testColor = false;
                            if (testColor)
                            {
                                if (entry.name.ToLower().IndexOf("xxxx") >= 0)
                                {
                                    writer.Write((char)0x08);
                                }
                                else if (entry.name.ToLower().IndexOf("xxxx") >= 0)
                                {
                                    writer.Write((char)0x07);
                                }
                                else if (entry.name.ToLower().IndexOf("xxxx") >= 0)
                                {
                                    writer.Write((char)0x06);
                                }
                                else if (entry.name.ToLower().IndexOf("xxxx") >= 0)
                                {
                                    writer.Write((char)0x04);
                                }
                                else
                                {
                                    writer.Write((char)0x0b);
                                }
                            }
                            else
                            {
                                //Asign via table
                                if (entry.baseDataType != default(BaseDataTypes))
                                {
                                    string s = entry.baseDataType.ToString();
                                    //System.Diagnostics.Debug.WriteLine(string.Format("Hightway type: {0}", s));
                                    writer.Write((char)entry.baseDataType);
                                }
                                else
                                {

                                    switch (entry.wayType)
                                    {
                                        case Way.WayType.Waterway:
                                            writer.Write(chWaterWay);
                                            break;
                                        case Way.WayType.highway_motorway:
                                            writer.Write(chArteryPrimary);
                                            break;
                                        case Way.WayType.trunk:
                                            writer.Write(chArterySecondary);
                                            break;
                                        case Way.WayType.trunk_link:
                                            writer.Write(chArteryTertiary);
                                            break;
                                        default:
                                            switch (entry.typeHighway.Trim().ToLower())
                                            {
                                                case "residential":
                                                    writer.Write(chResidential);
                                                    break;
                                                //case "unclassified":
                                                case "track":
                                                    //case "path":
                                                    writer.Write(chFootway);
                                                    break;
                                                default:
                                                    //writer.Write((char)0x0b);
                                                    writer.Write(chResidential);
                                                    break;
                                            }
                                            break;
                                    }
                                }
                            }

                            //short lenLabel = (short)entry.name.Length;
                            //short lenLabelUtf8 = (short)Encoding.UTF8.GetByteCount(entry.name);

                            //writer.Write(lenLabelUtf8);

                            String name = entry.name;
                            if(entry.wayType!=Way.WayType.Waterway && entry.wayType != Way.WayType.highway_motorway)
                            {
                                name = "";
                            }
                            if(String.IsNullOrEmpty(name.Trim()))
                            {
                                name = "not defined";
                            }

                            short lenLabel = (short)name.Length;
                            short lenLabelUtf8 = (short)Encoding.UTF8.GetByteCount(name);
                            writer.Write(lenLabelUtf8);

                            byte[] btName = Encoding.UTF8.GetBytes(name);

                            writer.Write(btName);

                            //break;

                            writer.Write((char)0x00);
                            writer.Write((char)0x00);
                            writer.Write((char)0x00);

                            short lenNodes = (short)entry.nodes.Count;
                            writer.Write(lenNodes);

                            foreach (Node node in entry.nodes)
                            {
                                double lonOff = Haversine(node.lat, m_dLon0, node.lat, node.lon);
                                lonOff = lonOffset0 - lonOff;

                                uint uiLon = (uint)Math.Round(lonOff);

                                byte[] bytes1 = BitConverter.GetBytes(Convert.ToSingle(node.lon));
                                Array.Reverse(bytes1);

                                writer.Write(bytes1);

                                //writer.Write(uiLon);

                                double latOff = Haversine(m_dLat0, node.lon, node.lat, node.lon);
                                latOff += latOffset0;

                                uint uiLat = (uint)Math.Round(latOff);

                                byte[] bytes2 = BitConverter.GetBytes(Convert.ToSingle(node.lat));
                                Array.Reverse(bytes2);

                                writer.Write(bytes2);

                                //writer.Write(uiLat);
                            }
                        }

                        //break;
                    }
                }
            }
        }

        private Way InsectWayWithTile(RectXY rect, Way way)
        {
            List<Node> lstNodesInBox = new List<Node>();
            Way wayOut = new Way(way);

            Boolean inBox = false;
            Boolean outBox = false;
            foreach (Node node in way.nodes)
            {
                if(rect.contains(Convert.ToSingle(node.lon), Convert.ToSingle(node.lat)))
                {
                    lstNodesInBox.Add(node);
                    //We have been in, out and in again?
                    if(outBox)
                    {
                        int stop = 1;
                        int debug = stop;
                    }
                    inBox = true;
                }
                else if(inBox)
                {
                    outBox = true;
                }
            }

            wayOut.nodes=lstNodesInBox;

            return wayOut;
        }

        public void WriteResultToDisk(String basePath, String fileName, double lonOffset0, double latOffset0, List<int> lstTiles)
        {
            //Create dictionary to map tiles to ways
            Dictionary<int, List<Way>> dictTileWay = new Dictionary<int, List<Way>>();
            foreach (int tile in lstTiles)
            {
                dictTileWay.Add(tile, new List<Way>());
            }

            using (var stream = File.Open(fileName, FileMode.Create))
            {
                using (var writer = new BinaryWriter2(stream, Encoding.UTF8, false))
                {
                    short lenTot = (short)ways.Count;
                    writer.Write(lenTot);

                    foreach (KeyValuePair<String, Way> entry in ways)
                    {
                        //!We are procesing ways with 2 nodes
                        if (entry.Value.nodes.Count >= 2)
                        {
                            //Check file from bounding box
                            RectXY boundBoxWay = entry.Value.getBoundingBox();

                            //Console.WriteLine(boundBoxWay.toString());

                            foreach (int tile in lstTiles)
                            {
                                GeoRegion geoRegion = GeoTile.getGeoRegionFromTileIndex(tile);
                                //Console.WriteLine(geoRegion.mBoundingBox.toString());

                                if (geoRegion.mBoundingBox.contains(boundBoxWay))
                                {
                                    dictTileWay[tile].Add(entry.Value);
                                }
                                else if(geoRegion.mBoundingBox.intersects(boundBoxWay))
                                {
                                    //Console.WriteLine("Way intersects bound box of tile");
                                    Way wayOut = InsectWayWithTile(geoRegion.mBoundingBox, entry.Value);
                                    if(wayOut.nodes.Count>=2)
                                    {
                                        dictTileWay[tile].Add(wayOut);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(String.Format("{0} way not contained in geo region", entry.Key));
                                }
                            }

                            //------------------------------------------------------------------------------------------------------

                            switch(entry.Value.wayType)
                            {
                                case Way.WayType.Waterway:
                                    writer.Write(chWaterWay);
                                    break;
                                default:
                                    writer.Write((char)0x0b);
                                    break;
                            }

                            short lenLabel = (short)entry.Value.name.Length;
                            short lenLabelUtf8 = (short)Encoding.UTF8.GetByteCount(entry.Value.name);

                            writer.Write(lenLabelUtf8);

                            String name = entry.Value.name;

                            byte[] btName = Encoding.UTF8.GetBytes(name);

                            writer.Write(btName);

                            //break;

                            writer.Write((char)0x00);
                            writer.Write((char)0x00);
                            writer.Write((char)0x00);

                            short lenNodes = (short)entry.Value.nodes.Count;
                            writer.Write(lenNodes);

                            foreach (Node node in entry.Value.nodes)
                            {
                                double lonOff = Haversine(node.lat, m_dLon0, node.lat, node.lon);
                                //lonOff = lonOffset0 - lonOff;
                                lonOff += lonOffset0;

                                uint uiLon = (uint)Math.Round(lonOff);

                                byte[] bytes1 = BitConverter.GetBytes(Convert.ToSingle(node.lon));
                                Array.Reverse(bytes1);

                                //writer.Write(uiLon);
                                writer.Write(bytes1);

                                double latOff = Haversine(m_dLat0, node.lon, node.lat, node.lon);
                                latOff += latOffset0;

                                uint uiLat = (uint)Math.Round(latOff);

                                byte[] bytes2 = BitConverter.GetBytes(Convert.ToSingle(node.lat));
                                Array.Reverse(bytes2);

                                //writer.Write(uiLat);
                                writer.Write(bytes2);
                            }
                        }

                        //break;
                    }
                }
            }

            foreach (int tile in lstTiles)
            {
                WriteWaysToDisk(basePath + @"\" + tile.ToString(), lonOffset0, latOffset0,
                    dictTileWay[tile]);
            }

            foreach (int tile in lstTiles)
            {
                var bytes = File.ReadAllBytes(basePath + @"\" + tile.ToString());
                if (File.Exists(basePath + @"\" + tile.ToString() + ".rgz"))
                {
                    File.Delete(basePath + @"\" + tile.ToString() + ".rgz");
                }
                using (FileStream fs = new FileStream(basePath + @"\" + tile.ToString() + ".rgz", FileMode.CreateNew))
                using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
                {
                    zipStream.Write(bytes, 0, bytes.Length);
                }
            }

            fileName = basePath + @"\" + "DownloadedOSMTiles-Base-filelisting.txt";
            /*
            using (var stream = File.Open(fileName, FileMode.Create))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.WriteLine("<list>");
                    writer.WriteLine("<s>ls /storage/sdcard0/ReconApps/GeodataService/DownloadedOSMTiles/Base</s>");
                    writer.WriteLine("<list>");
                    foreach (int tile in lstTiles)
                    {
                        writer.WriteLine(string.Format("<s>{0}.rgz</s>", tile));
                    }
                    writer.WriteLine("<s>version.txt</s>");
                    writer.WriteLine("</list>");
                    writer.WriteLine("</list>");
                }
            }
            */
        }

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

        public enum BaseDataTypes
        {
            AREA_LAND,
            AREA_OCEAN,
            AREA_CITYTOWN,
            AREA_WOODS,
            AREA_WATER,
            AREA_FIELD,
            AREA_PARK,
            HIGHWAY_MOTORWAY,
            HIGHWAY_PRIMARY,
            HIGHWAY_SECONDARY,
            HIGHWAY_TERTIARY,
            HIGHWAY_RESIDENTIAL,
            POI_RESTAURANT,
            POI_STORE,
            POI_HOSPITAL,
            POI_WASHROOM,
            POI_DRINKINGWATER,
            LINE_NATIONAL_BORDER,
            POI_PARKING,
            POI_INFORMATION,
            LINE_SKIRUN_GREEN,
            LINE_SKIRUN_BLUE,
            LINE_SKIRUN_BLACK,
            LINE_SKIRUN_DBLACK,
            LINE_SKIRUN_RED,
            LINE_CHAIRLIFT,
            LINE_ROADWAY,
            LINE_WALKWAY,
            AREA_SKIRESORT,
            WATERWAY
        }
    }
}
