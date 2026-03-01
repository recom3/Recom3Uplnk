using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recom3Uplnk.Maps
{
    class BinaryWriter2 : BinaryWriter
    {
        public BinaryWriter2(System.IO.Stream stream) : base(stream) { }
        public BinaryWriter2(System.IO.Stream stream, System.Text.Encoding encoding) : base(stream, encoding) { }
        public BinaryWriter2(System.IO.Stream stream, System.Text.Encoding encoding, bool leaveOpen) : base(stream, encoding, leaveOpen) { }

        public override void Write(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(BitConverter.ToInt16(bytes, 0));
        }

        public override void Write(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(BitConverter.ToUInt32(bytes, 0));
        }
    }
}
