using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk.Maps
{
    class PointXY
    {
        public float x;
        public float y;

        public PointXY(float _x, float _y)
        {
            this.x = _x;
            this.y = _y;
        }

        public double distanceToPoint(PointXY p)
        {
            return Math.Sqrt(((this.x - p.x) * (this.x - p.x)) + ((this.y - p.y) * (this.y - p.y)));
        }
    }
}
