using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    internal static class BinaryHelper
    {
        public static int GetSize(this Type type)
        {
            if (type.BaseType == typeof(Enum))
            {
                type = Enum.GetUnderlyingType(type);
            }
            else if (type == typeof(bool))
            {                
                type = typeof(byte);                
            }
            else if (type == typeof(char))
            {
                type = typeof(short);
            }

            return Marshal.SizeOf(type);
        }

        public static byte[] Read(this Stream stream, Type dataType)
        {
            int size;

            if (dataType == typeof(byte[]) || dataType == typeof(string) || dataType == typeof(Stream))
            {
                size = BitConverter.ToInt32(stream.Read(typeof(int)), 0);
            }
            else
            {
                size = dataType.GetSize();
            }

            int offset = 0;

            byte[] buffer = new byte[size];

            do
            {
                int readBytes = stream.Read(buffer, offset, size - offset);

                offset += readBytes;
            }
            while (offset < size);

            return buffer;
        }
    }
}
