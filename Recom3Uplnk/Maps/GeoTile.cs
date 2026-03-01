using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk.Maps
{
    class GeoTile
    {
        private const float EQUITORIALCIRCUMFRENCE = 4.0075016E7f;
        private const float MERIDIONALCIRCUMFRENCE = 4.000786E7f;
        private const int NUMBER_TILES_PER_HEMISPHERE = 3600;
        private const double TILE_HEIGHT_IN_DEGREES = 0.025d;
        private const double TILE_HEIGHT_IN_METERS = 2778.3d;
        private const double TILE_WIDTH_IN_DEGREES_AT_EQUATOR = 0.024958105d;
        private const double TILE_WIDTH_IN_METERS = 2778.3d;
        public static List<int> getTileListForGeoRegion(GeoRegion geoRegion, List<int> ignoreTheseTiles)
        {
            double bottomOfTileLatitude;
            List<int> tileList = new List<int>();
            double startLat = geoRegion.mBoundingBox.top;
            double startLong = geoRegion.mBoundingBox.left;
            double endLat = startLat;
            double endLong = geoRegion.mBoundingBox.right;
            //https://stackoverflow.com/questions/477954/java-treemap-equivalent-in-c
            //TreeMap<int, int> doNotInclude = new TreeMap<>();
            SortedDictionary<int, int> doNotInclude = new SortedDictionary<int, int>();
            /*
            if (ignoreTheseTiles != null)
            {
                Iterator i$ = ignoreTheseTiles.iterator();
                while (i$.hasNext()) {
                    int tileIndex = i$.next();
                    doNotInclude.put(tileIndex, tileIndex);
                }
            }
            */
            SortedDictionary<float, int> tileDistance = new SortedDictionary<float, int>();
            //RedBlack tileDistance = new RedBlack();

            int centerTileIndex = getTileIndex(geoRegion.mCenterPoint.x, geoRegion.mCenterPoint.y);
            GeoRegion centerTileGeoRegion = getGeoRegionFromTileIndex(centerTileIndex);
            double dlongAtCenter = TILE_WIDTH_IN_DEGREES_AT_EQUATOR / Math.Cos(ToRadians(geoRegion.mCenterPoint.y));
            do
            {
                int startPointTileIndex = getTileIndex(startLong, startLat);
                GeoRegion startTileGeoRegion = getGeoRegionFromTileIndex(startPointTileIndex);
                int endPointTileIndex = getTileIndex(endLong, endLat);
                bottomOfTileLatitude = Math.Floor(startLat / TILE_HEIGHT_IN_DEGREES) * TILE_HEIGHT_IN_DEGREES;
                double curLong = startTileGeoRegion.mBoundingBox.left;
                double dlong = TILE_WIDTH_IN_DEGREES_AT_EQUATOR / Math.Cos(ToRadians(startTileGeoRegion.mBoundingBox.bottom));
                if (endPointTileIndex >= startPointTileIndex)
                {
                    for (int index = startPointTileIndex; index <= endPointTileIndex; index = index + 1)
                    {
                        if (doNotInclude.ContainsKey(index) == false)
                        {
                            tileDistance.Add(distanceBetweenPoints(curLong, startTileGeoRegion.mBoundingBox.bottom, centerTileGeoRegion.mBoundingBox.left, centerTileGeoRegion.mBoundingBox.bottom, dlongAtCenter), index);
                        }
                        curLong += dlong;
                    }
                }
                else
                {
                    int maxTileIndex = combineTileSubIndices((int)(Math.Floor(startLat / TILE_HEIGHT_IN_DEGREES) + NUMBER_TILES_PER_HEMISPHERE), (int)(Math.Floor((4.0075016E7d * Math.Cos(ToRadians(startLat))) / 2778.3d)));
                    for (int index2 = startPointTileIndex; index2 <= maxTileIndex; index2 = index2 + 1)
                    {
                        if (doNotInclude.ContainsKey(index2) == false)
                        {
                            tileDistance.Add(distanceBetweenPoints(curLong, startTileGeoRegion.mBoundingBox.bottom, centerTileGeoRegion.mBoundingBox.left, centerTileGeoRegion.mBoundingBox.bottom, dlongAtCenter), index2);
                        }
                        curLong += dlong;
                    }
                    double curLong2 = 0.0d;
                    int sIndex = (endPointTileIndex / 100000) * 100000;
                    for (int index3 = sIndex; index3 <= endPointTileIndex; index3 = index3 + 1)
                    {
                        if (doNotInclude.ContainsKey(index3) == false)
                        {
                            tileDistance.Add(distanceBetweenPoints(curLong2, startTileGeoRegion.mBoundingBox.bottom, centerTileGeoRegion.mBoundingBox.left, centerTileGeoRegion.mBoundingBox.bottom, dlongAtCenter), index3);
                        }
                        curLong2 += dlong;
                    }
                }
                startLat -= TILE_HEIGHT_IN_DEGREES;
                endLat -= TILE_HEIGHT_IN_DEGREES;
            } while (bottomOfTileLatitude > geoRegion.mBoundingBox.bottom);

            foreach (KeyValuePair<float, int> entry in tileDistance)
            //RedBlackEnumerator t = tileDistance.Values();
            //while (t.MoveNext())
            {
                //int iData = (int)t.Value;
                tileList.Add(entry.Value);
            }
            return tileList;
        }

        public static GeoRegion getGeoRegionFromTileIndex(long tileIndex)
        {
            int yIndex = (int)(tileIndex / 100000);
            double bottom = (yIndex - 3600) * TILE_HEIGHT_IN_DEGREES;
            double top = bottom + TILE_HEIGHT_IN_DEGREES;
            int xIndex = (int)(tileIndex % 100000);
            double dlong = TILE_WIDTH_IN_DEGREES_AT_EQUATOR / Math.Cos(ToRadians(bottom));
            double left = xIndex * dlong;
            if (left > 180.0d)
            {
                left -= 360.0d;
            }
            double right = (xIndex + 1) * dlong;
            if (right > 180.0d)
            {
                right -= 360.0d;
            }
            return new GeoRegion().makeUsingBoundingBox((float)left, (float)top, (float)right, (float)bottom);
        }

        public static float distanceBetweenPoints(double longObs, double latObs, double longRef, double latRef, double dlongAtRefLat)
        {
            double latdiff = (latObs - latRef) / TILE_HEIGHT_IN_DEGREES;
            double longdiff = (longObs - longRef) / dlongAtRefLat;
            double dist = Math.Sqrt((longdiff * longdiff) + (latdiff * latdiff));
            //return float.valueOf((float)dist);
            return Convert.ToSingle(dist);
        }

        public static int getTileIndex(double longitude, double latitude)
        {
            double latRatio = latitude / TILE_HEIGHT_IN_DEGREES;
            //int baseLatIndex = floorWithJavaFix(latRatio);
            int baseLatIndex = (int)Math.Floor(latRatio);

            double dlong = TILE_WIDTH_IN_DEGREES_AT_EQUATOR / Math.Cos(ToRadians(baseLatIndex * TILE_HEIGHT_IN_DEGREES));
            int latIndex = baseLatIndex + NUMBER_TILES_PER_HEMISPHERE;
            double positiveLongitude = (360.0d + longitude) % 360.0d;

            //int longIndex = floorWithJavaFix(positiveLongitude / dlong);
            int longIndex = (int)Math.Floor(positiveLongitude / dlong);
            int result = combineTileSubIndices(latIndex, longIndex);
            return result;
        }

        public static int combineTileSubIndices(int latIndex, int longIndex)
        {
            return (100000 * latIndex) + longIndex;
        }

        public static double ToRadians(double angleIn10thofaDegree)
        {
            // Angle in 10th of a degree
            return (angleIn10thofaDegree * Math.PI) / 180;
        }

        public static int floorWithJavaFix(double value)
        {
            return (int)Math.Floor(1.0E-10d + value);
        }
    }
}
