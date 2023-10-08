using System;
using System.IO;
using System.Linq;
using System.Text;

namespace net.rs64.PSD.parser
{
    public static class ParserUtility
    {
        public static uint ReadByteToUInt32(this Stream stream, bool IsLittleEndian = false) => BitConverter.ToUInt32(IsLittleEndian ? ReadBytes(stream, 4) : ConvertLittleEndian(ReadBytes(stream, 4)), 0);
        public static int ReadByteToInt32(this Stream stream) => BitConverter.ToInt32(ConvertLittleEndian(ReadBytes(stream, 4)), 0);
        public static ushort ReadByteToUInt16(this Stream stream) => BitConverter.ToUInt16(ConvertLittleEndian(ReadBytes(stream, 2)), 0);
        public static short ReadByteToInt16(this Stream stream) => BitConverter.ToInt16(ConvertLittleEndian(ReadBytes(stream, 2)), 0);
        public static ulong ReadByteToUInt64(this Stream stream) => BitConverter.ToUInt64(ConvertLittleEndian(ReadBytes(stream, 8)), 0);
        public static long ReadByteToInt64(this Stream stream) => BitConverter.ToInt64(ConvertLittleEndian(ReadBytes(stream, 8)), 0);
        public static byte ReadByteToByte(this Stream stream) => (byte)stream.ReadByte();
        public static sbyte ReadByteTosByte(this Stream stream) => (sbyte)stream.ReadByte();
        public static double ReadByteToDouble(this Stream stream) => BitConverter.ToDouble(ConvertLittleEndian(ReadBytes(stream, 8)), 0);
        public static byte[] ReadBytes(this Stream stream, uint count)
        {
            if (count == 0) { return null; }
            byte[] array = new byte[count];
            stream.Read(array, 0, (int)count);
            return array;
        }
        public static byte[] ConvertLittleEndian(byte[] array)
        {
            return BitConverter.IsLittleEndian ? array.Reverse().ToArray() : array;
        }
        public static bool Signature(Stream stream, byte[] signature)
        {
            bool HitSignature = false;
            while (!HitSignature)
            {
                byte readValue = stream.ReadByteToByte();
                if (stream.Position == stream.Length - 3) { return false; }
                if (readValue == signature[0]) { HitSignature = true; stream.Position -= 1; }
            }

            return ReadBytes(stream, (uint)signature.Length).SequenceEqual(signature);

        }

        public static string ReadPascalString(Stream stream)
        {
            var stringLength = stream.ReadByte();
            var count = 1;
            if (stringLength != 0)
            {
                var str = Encoding.GetEncoding("shift-jis").GetString(ReadBytes(stream, (uint)stringLength));
                count += stringLength;
                if ((count % 4) != 0)
                {
                    for (int i = 0; (4 - (count % 4)) > i; i += 1)
                    {
                        stream.ReadByte();
                    }
                }
                return str;
            }
            else
            {
                stream.ReadByte();
                return "";
            }
        }
        public static string ParseUTF8(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        public static string ParseUTF16(this byte[] bytes)
        {
            return Encoding.Unicode.GetString(bytes);
        }
        public static string ReadUnicodeString(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}