using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk.Maps
{
    class GeoRegion
    {
        private const double equitorialCircumference = 4.0075017E7d;
        private const double meridonalCircumference = 4.000786E7d;
        private String TAG;
        public RectXY mBoundingBox;
        protected bool mCenterOnUser;
        public PointXY mCenterPoint;
        protected PointXY mSize;

        public GeoRegion makeUsingBoundingBox(float _left, float _top, float _right, float _bottom)
        {
            this.mCenterOnUser = false;
            this.mCenterPoint = new PointXY((_right + _left) / 2.0f, (_top + _bottom) / 2.0f);
            this.mBoundingBox = new RectXY(_left, _top, _right, _bottom);
            this.mSize = new PointXY(_right - _left, _top - _bottom);
            return this;
        }
    }
}
