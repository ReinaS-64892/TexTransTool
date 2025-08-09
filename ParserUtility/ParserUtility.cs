using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;

namespace net.rs64.ParserUtility
{
    public class BinarySectionStream
    {
        private byte[] _array;
        private long _start;
        private long _length;
        private long _position;

        public long Position
        {
            get => _position;
            set
            {
                if (_length >= value)
                {
                    _position = value;
                }
                else { throw new ArgumentOutOfRangeException(); }
            }
        }
        public long Length => _length;
        long ArrayPosition => _start + _position;

        public bool BigEndian = true;

        public BinarySectionStream(byte[] array)
        {
            _array = array;
            _start = 0;
            _position = 0;
            _length = array.LongLength;
        }
        private BinarySectionStream(byte[] array, long start, long length)
        {
            _array = array;
            _start = start;
            _length = length;
        }
        private bool CheckSectionRange(long absPos)//ダメな範囲だったら False を返すから注意ね
        {
            var relPos = absPos - _start;
            if (relPos < 0 || relPos > _length) { return true; }
            return false;
        }
        public BinarySectionStream ReadSubSection(long length)
        {
            var subSectionStart = ArrayPosition;
            if (CheckSectionRange(subSectionStart) || CheckSectionRange(subSectionStart + length)) { throw new ArgumentOutOfRangeException(); }
            Position += length;
            return new BinarySectionStream(_array, subSectionStart, length);
        }
        public BinaryAddress ReadToAddress(long length)
        {
            var ba = PeekToAddress(length);
            Position += length;
            return ba;
        }
        public BinaryAddress PeekToAddress(long length)
        {
            return new() { StartAddress = ArrayPosition, Length = length };
        }
        public byte ReadByte()
        {
            var beforePos = ArrayPosition;
            Position += 1;
            return _array[beforePos];
        }
        public sbyte ReadsByte()
        {
            var beforePos = ArrayPosition;
            Position += 1;
            return (sbyte)_array[beforePos];
        }
        public void ReadToSpan(Span<byte> write)
        {
            _array.LongCopyTo(ArrayPosition, write);
            Position += write.Length;
        }
        public ushort ReadUInt16()
        {
            Span<byte> span = stackalloc byte[2]; ReadToSpan(span);
            if (BigEndian) { return BinaryPrimitives.ReadUInt16BigEndian(span); }
            else { return BinaryPrimitives.ReadUInt16LittleEndian(span); }
        }
        public short ReadInt16()
        {
            Span<byte> span = stackalloc byte[2]; ReadToSpan(span);
            if (BigEndian) { return BinaryPrimitives.ReadInt16BigEndian(span); }
            else { return BinaryPrimitives.ReadInt16LittleEndian(span); }
        }
        public uint ReadUInt32()
        {
            Span<byte> span = stackalloc byte[4]; ReadToSpan(span);
            if (BigEndian) { return BinaryPrimitives.ReadUInt32BigEndian(span); }
            else { return BinaryPrimitives.ReadUInt32LittleEndian(span); }
        }
        public int ReadInt32()
        {
            Span<byte> span = stackalloc byte[4]; ReadToSpan(span);
            if (BigEndian) { return BinaryPrimitives.ReadInt32BigEndian(span); }
            else { return BinaryPrimitives.ReadInt32LittleEndian(span); }
        }
        public ulong ReadUInt64()
        {
            Span<byte> span = stackalloc byte[8]; ReadToSpan(span);
            if (BigEndian) { return BinaryPrimitives.ReadUInt64BigEndian(span); }
            else { return BinaryPrimitives.ReadUInt64LittleEndian(span); }
        }
        public long ReadInt64()
        {
            Span<byte> span = stackalloc byte[8]; ReadToSpan(span);
            if (BigEndian) { return BinaryPrimitives.ReadInt64BigEndian(span); }
            else { return BinaryPrimitives.ReadInt64LittleEndian(span); }
        }
        public double ReadDouble()
        {
            Span<byte> span = stackalloc byte[8]; ReadToSpan(span);
            if (BigEndian) { return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(span)); }
            else { return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(span)); }
        }
    }
    [Serializable]
    public struct BinaryAddress
    {
        public long StartAddress;
        public long Length;
    }
    public static class ParserUtil
    {
        public static bool Signature(this BinarySectionStream stream, byte[] signature)
        {
            Span<byte> data = stackalloc byte[signature.Length];
            stream.ReadToSpan(data);
            return data.SequenceEqual(signature);
        }

        public static string ReadPascalStringForPadding4Byte(BinarySectionStream stream)
        {
            var stringLength = stream.ReadByte();
            if (stringLength != 0)
            {
                Span<byte> strBuf = stackalloc byte[stringLength];
                stream.ReadToSpan(strBuf);
                var str = Encoding.GetEncoding("shift-jis").GetString(strBuf);
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
                // 4byte padding
                stream.ReadByte();
                stream.ReadByte();
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

        public static void CopyFrom<T>(this Span<T> to, Span<T> from) where T : struct
        {
            for (var i = 0; to.Length > i; i += 1)
            {
                to[i] = from[i];
            }
        }
        public static void LongCopyTo<T>(this T[] from, long start, Span<T> to) where T : struct
        {
            for (var i = 0; to.Length > i; i += 1)
            {
                to[i] = from[start + i];
            }
        }

    }
}
