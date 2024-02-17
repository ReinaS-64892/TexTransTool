using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;

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


    internal static class ParserUtility
    {
        public static bool Signature(ref SubSpanStream stream, byte[] signature)
        {
            bool hitSignature = false;
            while (!hitSignature)
            {
                byte readValue = stream.ReadByte();
                if (stream.Position == stream.Length - 3) { return false; }
                if (readValue == signature[0]) { hitSignature = true; stream.Position -= 1; }
            }

            return stream.ReadSubStream(signature.Length).Span.SequenceEqual(signature);

        }

        public static string ReadPascalString(ref SubSpanStream stream)
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








        public static void Fill<T>(this NativeArray<T> values, T val) where T : struct
        {
            for (var i = 0; values.Length > i; i += 1)
            {
                values[i] = val;
            }
        }
        public static void Fill<T>(this NativeSlice<T> values, T val) where T : struct
        {
            for (var i = 0; values.Length > i; i += 1)
            {
                values[i] = val;
            }
        }

        public static void CopyTo<T>(this NativeSlice<T> from, NativeSlice<T> to) where T : struct
        {
            to.CopyFrom(from);
        }
        public static void CopyTo<T>(this NativeArray<T> from, NativeSlice<T> to) where T : struct
        {
            to.CopyFrom(from);
        }

        public static void CopyFrom<T>(this NativeArray<T> to, Span<T> from) where T : struct
        {
            for (var i = 0; to.Length > i; i += 1)
            {
                to[i] = from[i];
            }
        }
    }
}