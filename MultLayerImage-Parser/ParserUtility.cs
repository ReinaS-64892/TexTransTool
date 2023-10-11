using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace net.rs64.PSD.parser
{
    public static class ParserUtility
    {
        // public static uint ReadByteToUInt32(this Stream stream, bool IsLittleEndian = false) => BitConverter.ToUInt32(IsLittleEndian ? ReadBytes(stream, 4) : ConvertLittleEndian(ReadBytes(stream, 4)), 0);
        // public static int ReadByteToInt32(this Stream stream) => BitConverter.ToInt32(ConvertLittleEndian(ReadBytes(stream, 4)), 0);
        // public static ushort ReadByteToUInt16(this Stream stream) => BitConverter.ToUInt16(ConvertLittleEndian(ReadBytes(stream, 2)), 0);
        // public static short ReadByteToInt16(this Stream stream) => BitConverter.ToInt16(ConvertLittleEndian(ReadBytes(stream, 2)), 0);
        // public static ulong ReadByteToUInt64(this Stream stream) => BitConverter.ToUInt64(ConvertLittleEndian(ReadBytes(stream, 8)), 0);
        // public static long ReadByteToInt64(this Stream stream) => BitConverter.ToInt64(ConvertLittleEndian(ReadBytes(stream, 8)), 0);
        // public static byte ReadByteToByte(this Stream stream) => (byte)stream.ReadByte();
        // public static sbyte ReadByteTosByte(this Stream stream) => (sbyte)stream.ReadByte();
        // public static double ReadByteToDouble(this Stream stream) => BitConverter.ToDouble(ConvertLittleEndian(ReadBytes(stream, 8)), 0);
        // public static byte[] ReadBytes(this Stream stream, uint count)
        // {
        //     if (count == 0) { return null; }
        //     byte[] array = new byte[count];
        //     stream.Read(array, 0, (int)count);
        //     return array;
        // }
        // public static byte[] ConvertLittleEndian(byte[] array)
        // {
        //     return BitConverter.IsLittleEndian ? array.Reverse().ToArray() : array;
        // }
        public static bool Signature(SubSpanStream stream, byte[] signature)
        {
            bool HitSignature = false;
            while (!HitSignature)
            {
                byte readValue = stream.ReadByte();
                if (stream.Position == stream.Length - 3) { return false; }
                if (readValue == signature[0]) { HitSignature = true; stream.Position -= 1; }
            }

            return stream.ReadSubStream(signature.Length).Span.SequenceEqual(signature);

        }

        public static string ReadPascalString(SubSpanStream stream)
        {
            var stringLength = stream.ReadByte();
            var count = 1;
            if (stringLength != 0)
            {
                var str = Encoding.GetEncoding("shift-jis").GetString(stream.ReadSubStream(stringLength).Span.ToArray());
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
        public static string ParseUTF8(this Span<byte> bytes)
        {
            return Encoding.UTF8.GetString(bytes.ToArray());
        }
        public static string ParseUTF16(this Span<byte> bytes)
        {
            return Encoding.Unicode.GetString(bytes.ToArray());
        }
        public static string ReadUnicodeString(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public ref struct SubSpanStream
    {
        public Span<byte> Span;
        private int _position;
        public int Position
        {
            get => _position;
            set
            {
                if (Span.Length > value)
                {
                    _position = value;
                }
                else
                {
                    _position = Length;
                }
            }
        }
        public int Length => Span.Length;

        public SubSpanStream(Span<byte> bytes)
        {
            Span = bytes;
            _position = 0;
        }

        public SubSpanStream ReadSubStream(int length)
        {
            var subSpan = Span.Slice(Position, length);
            Position += length;
            return new SubSpanStream(subSpan);
        }

        public byte ReadByte()
        {
            var beforePos = Position;
            Position += 1;
            return Span[beforePos];
        }
        public sbyte ReadsByte()
        {
            var beforePos = Position;
            Position += 1;
            return (sbyte)Span[beforePos];
        }
        public ushort ReadUInt16(bool isBigEndianSouse = true)
        {
            var beforePos = Position;
            Position += 2;
            if (isBigEndianSouse) { return BinaryPrimitives.ReadUInt16BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadUInt16LittleEndian(Span.Slice(beforePos)); }
        }
        public short ReadInt16(bool isBigEndianSouse = true)
        {
            var beforePos = Position;
            Position += 2;
            if (isBigEndianSouse) { return BinaryPrimitives.ReadInt16BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadInt16LittleEndian(Span.Slice(beforePos)); }
        }
        public uint ReadUInt32(bool isBigEndianSouse = true)
        {
            var beforePos = Position;
            Position += 4;
            if (isBigEndianSouse) { return BinaryPrimitives.ReadUInt32BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadUInt32LittleEndian(Span.Slice(beforePos)); }
        }
        public int ReadInt32(bool isBigEndianSouse = true)
        {
            var beforePos = Position;
            Position += 4;
            if (isBigEndianSouse) { return BinaryPrimitives.ReadInt32BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadInt32LittleEndian(Span.Slice(beforePos)); }
        }
        public ulong ReadUInt64(bool isBigEndianSouse = true)
        {
            var beforePos = Position;
            Position += 8;
            if (isBigEndianSouse) { return BinaryPrimitives.ReadUInt64BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadUInt64LittleEndian(Span.Slice(beforePos)); }
        }
        public long ReadInt64(bool isBigEndianSouse = true)
        {
            var beforePos = Position;
            Position += 8;
            if (isBigEndianSouse) { return BinaryPrimitives.ReadInt64BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadInt64LittleEndian(Span.Slice(beforePos)); }
        }
        public double ReadDouble(bool isBigEndianSouse = true)
        {
            var beforePos = Position;
            Position += 8;
            if (isBigEndianSouse) { return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(Span.Slice(beforePos))); }
            else { return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(Span.Slice(beforePos))); }
        }
    }
}