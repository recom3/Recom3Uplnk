using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk.Maps
{
    class RectXY
    {
        public float bottom;
        public float left;
        private PointXY mCenterPoint;
        public float right;
        public float top;

        public RectXY(float _left, float _top, float _right, float _bottom)
        {
            this.left = _left;
            this.top = _top;
            this.right = _right;
            this.bottom = _bottom;
            this.mCenterPoint = new PointXY((_right + _left) / 2.0f, (_top + _bottom) / 2.0f);
        }

        public bool contains(float _x, float _y)
        {
            return _x >= this.left && _x <= this.right && _y >= this.bottom && _y <= this.top;
        }

        public bool contains(RectXY _rect)
        {
            return _rect.left >= this.left && _rect.right <= this.right && _rect.top <= this.top && _rect.bottom >= this.bottom;
        }

        public bool intersects(RectXY _rect)
        {
            return _rect.left < this.right && this.left < _rect.right && this.bottom < _rect.top && _rect.bottom < this.top;
        }

        public PointXY getCenterPoint()
        {
            return this.mCenterPoint;
        }        public string toString()
        {
            return string.Format("{0} {1} {2} {3}", left, top, right, bottom);
        }
    }
}
