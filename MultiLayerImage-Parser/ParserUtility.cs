using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;

namespace net.rs64.MultiLayerImage.Parser
{
    internal ref struct SubSpanStream
    {
        public Span<byte> Span;
        private int _position;
        private long _firstToPosition;
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

        public long FirstToPosition => _firstToPosition;

        public SubSpanStream(Span<byte> bytes, long firstToPosition = 0)
        {
            Span = bytes;
            _position = 0;
            _firstToPosition = firstToPosition;
        }
        public SubSpanStream ReadSubStream(int length)
        {
            var ftOffset = _firstToPosition + _position;
            var subSpan = Span.Slice(Position, length);
            Position += length;
            return new SubSpanStream(subSpan, ftOffset);
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
        public ushort ReadUInt16(bool isBigEndianSource = true)
        {
            var beforePos = Position;
            Position += 2;
            if (isBigEndianSource) { return BinaryPrimitives.ReadUInt16BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadUInt16LittleEndian(Span.Slice(beforePos)); }
        }
        public short ReadInt16(bool isBigEndianSource = true)
        {
            var beforePos = Position;
            Position += 2;
            if (isBigEndianSource) { return BinaryPrimitives.ReadInt16BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadInt16LittleEndian(Span.Slice(beforePos)); }
        }
        public uint ReadUInt32(bool isBigEndianSource = true)
        {
            var beforePos = Position;
            Position += 4;
            if (isBigEndianSource) { return BinaryPrimitives.ReadUInt32BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadUInt32LittleEndian(Span.Slice(beforePos)); }
        }
        public int ReadInt32(bool isBigEndianSource = true)
        {
            var beforePos = Position;
            Position += 4;
            if (isBigEndianSource) { return BinaryPrimitives.ReadInt32BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadInt32LittleEndian(Span.Slice(beforePos)); }
        }
        public ulong ReadUInt64(bool isBigEndianSource = true)
        {
            var beforePos = Position;
            Position += 8;
            if (isBigEndianSource) { return BinaryPrimitives.ReadUInt64BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadUInt64LittleEndian(Span.Slice(beforePos)); }
        }
        public long ReadInt64(bool isBigEndianSource = true)
        {
            var beforePos = Position;
            Position += 8;
            if (isBigEndianSource) { return BinaryPrimitives.ReadInt64BigEndian(Span.Slice(beforePos)); }
            else { return BinaryPrimitives.ReadInt64LittleEndian(Span.Slice(beforePos)); }
        }
        public double ReadDouble(bool isBigEndianSource = true)
        {
            var beforePos = Position;
            Position += 8;
            if (isBigEndianSource) { return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(Span.Slice(beforePos))); }
            else { return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(Span.Slice(beforePos))); }
        }
    }


    internal static class ParserUtility
    {
        public static bool Signature(ref SubSpanStream stream, byte[] signature)
        {
            return stream.ReadSubStream(signature.Length).Span.SequenceEqual(signature);
        }

        public static string ReadPascalStringForPadding4Byte(ref SubSpanStream stream)
        {
            var stringLength = stream.ReadByte();
            if (stringLength != 0)
            {
                var str = Encoding.GetEncoding("shift-jis").GetString(stream.ReadSubStream(stringLength).Span);
                var readLength = stringLength + 1;
                if ((readLength % 4) != 0)
                {
                    var paddingLength = 4 - (readLength % 4);
                    for (int i = 0; paddingLength > i; i += 1)
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
        public static string ParseASCII(this Span<byte> bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
        public static string ParseUTF8(this Span<byte> bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        public static string ParseUTF16(this Span<byte> bytes)
        {
            return Encoding.Unicode.GetString(bytes);
        }
        public static string ParseBigUTF16(this Span<byte> bytes)
        {
            return Encoding.BigEndianUnicode.GetString(bytes);
        }
        public static string ReadUnicodeString(Stream stream)
        {
            throw new NotImplementedException();
        }








        public static void Fill<T>(this Span<T> values, T val) where T : struct
        {
            for (var i = 0; values.Length > i; i += 1)
            {
                values[i] = val;
            }
        }

        public static void CopyTo<T>(this Span<T> from, Span<T> to) where T : struct
        {
            to.CopyFrom(from);
        }

        public static void CopyFrom<T>(this Span<T> to, Span<T> from) where T : struct
        {
            for (var i = 0; to.Length > i; i += 1)
            {
                to[i] = from[i];
            }
        }
    }
}
