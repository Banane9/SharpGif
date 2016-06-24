using BitStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Contains methods for decoding and encoding of Gif-LZW compressed image data.
    /// </summary>
    // http://matthewflickinger.com/lab/whatsinagif/lzw_image_data.asp
    public static class GifLZW
    {
        private const byte maxCodeSize = 12;

        // The largest 12 bit value
        private const ushort maxCodeValue = 4095;

        private const byte maxStartCodeSize = 8;
        private const byte minStartCodeSize = 2;

        #region Decode

        /// <summary>
        /// Gets the decompressed  Color Data starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the compressed Color Data.</param>
        /// <returns>The decompressed Color Data.</returns>
        public static byte[] Decode(Stream stream)
        {
            var startCodeSize = (byte)stream.ReadByte();

            if (startCodeSize < minStartCodeSize || startCodeSize > maxStartCodeSize)
                throw new ArgumentOutOfRangeException("startCodeSize", "The encoded start code size has to be between 2 and 8!");

            var indexStream = new List<byte>();
            var codeData = GifDataStream.FromStream(stream);
            using (var codeStream = new BitStream.BitStream(new MemoryStream(codeData)))
            {
                var codeTable = new List<CodeTableEntry>();
                buildCodeTable(codeTable, startCodeSize);

                var codeSize = startCodeSize + 1u;
                var indexBuffer = new List<byte>();
                CodeTableEntry.CodeType codeType;
                ushort prevCode = getColorCodeCount(startCodeSize);
                bool first = true;
                do
                {
                    var codeBytes = new byte[2];
                    codeStream.ReadBits(codeBytes, 0, codeSize);
                    var code = BitConverter.ToUInt16(codeBytes, 0);

                    if (code < codeTable.Count)
                    {
                        codeType = codeTable[code].Type;

                        switch (codeType)
                        {
                            case CodeTableEntry.CodeType.Color:
                                indexStream.AddRange(codeTable[code].Colors);

                                if (!first)
                                {
                                    indexBuffer.Clear();
                                    indexBuffer.AddRange(codeTable[prevCode].Colors);
                                    indexBuffer.Add(codeTable[code].Colors[0]);

                                    codeTable.Add(new CodeTableEntry(indexBuffer.ToArray()));
                                }

                                first = false;
                                break;

                            case CodeTableEntry.CodeType.Clear:
                                indexBuffer.Clear();
                                buildCodeTable(codeTable, startCodeSize);
                                first = true;
                                break;
                        }
                    }
                    else if (code == codeTable.Count)
                    {
                        codeType = CodeTableEntry.CodeType.Color;

                        indexBuffer.Clear();
                        indexBuffer.AddRange(codeTable[prevCode].Colors);
                        indexBuffer.Add(codeTable[prevCode].Colors[0]);

                        codeTable.Add(new CodeTableEntry(indexBuffer.ToArray()));

                        indexStream.AddRange(codeTable[code].Colors);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("code", "Encoded code wasn't too high. Expected: " + codeTable.Count + " or less; got: " + code);
                    }

                    if (Math.Pow(2, codeSize) <= codeTable.Count)
                        ++codeSize;

                    prevCode = code;
                }
                while (codeType != CodeTableEntry.CodeType.EndOfInformation);
            }

            return indexStream.ToArray();
        }

        private static void buildCodeTable(List<CodeTableEntry> codeTable, byte startCodeSize)
        {
            codeTable.Clear();

            // startCodeSize has a max of 8, so it will always fit into a byte
            var colorCodes = getColorCodeCount(startCodeSize);
            for (byte code = 0; code < colorCodes; ++code)
                codeTable.Add(new CodeTableEntry(code));

            codeTable.Add(new CodeTableEntry(CodeTableEntry.CodeType.Clear));
            codeTable.Add(new CodeTableEntry(CodeTableEntry.CodeType.EndOfInformation));
        }

        private static byte getColorCodeCount(byte startCodeSize)
        {
            return (byte)(Math.Pow(2, startCodeSize));
        }

        #endregion Decode

        #region Encode

        /// <summary>
        /// Writes the compressed version of the given Color Data to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="colorData">The Color Data to compress.</param>
        /// <param name="colorTableSize">Number of entries in the used color table.</param>
        public static void Encode(Stream stream, IEnumerable<byte> colorData, ushort colorTableSize)
        {
            var startCodeSize = (byte)Math.Max(minStartCodeSize, Math.Ceiling(Math.Log(colorTableSize, 2)));

            if (startCodeSize > maxStartCodeSize)
                throw new ArgumentOutOfRangeException("colorTableSize", "Color Table can have a maximum of 256 entries!");

            stream.WriteByte(startCodeSize);

            using (var codeStream = new BitStream.BitStream(new MemoryStream()))
            {
                var codeTable = new Dictionary<CodeTableEntry, ushort>();

                byte codeSize = maxCodeSize + 1;
                var indexBuffer = new List<byte>();
                foreach (var color in colorData)
                {
                    if (codeSize > maxCodeSize)
                    {
                        codeStream.WriteBits(BitConverter.GetBytes(codeTable[new CodeTableEntry(CodeTableEntry.CodeType.Clear)]), 0, maxCodeSize);
                        buildCodeTable(codeTable, startCodeSize);
                        codeSize = (byte)(startCodeSize + 1);
                    }

                    var prevIndexBuffer = indexBuffer.ToArray();

                    indexBuffer.Add(color);
                    var tableEntry = new CodeTableEntry(indexBuffer.ToArray());

                    if (codeTable.ContainsKey(tableEntry))
                        continue;

                    codeTable.Add(tableEntry, (ushort)codeTable.Count);

                    codeStream.WriteBits(BitConverter.GetBytes(codeTable[new CodeTableEntry(prevIndexBuffer)]), 0, codeSize);

                    indexBuffer.Clear();
                    indexBuffer.Add(color);

                    if (Math.Pow(2, codeSize) <= codeTable.Count)
                        ++codeSize;
                }

                codeStream.WriteBits(BitConverter.GetBytes(codeTable[new CodeTableEntry(indexBuffer.ToArray())]), 0, codeSize);
                codeStream.WriteBits(BitConverter.GetBytes(codeTable[new CodeTableEntry(CodeTableEntry.CodeType.EndOfInformation)]), 0, codeSize);
                codeStream.WriteBits(0, (BitNum)(BitNum.MaxValue - codeStream.BitPosition));

                var codes = ((MemoryStream)codeStream.UnderlayingStream).ToArray();
                GifDataStream.ToStream(stream, codes);
            }
        }

        private static void buildCodeTable(Dictionary<CodeTableEntry, ushort> codeTable, byte startCodeSize)
        {
            codeTable.Clear();

            // startCodeSize has a max of 8, so it will always fit into a byte
            var colorCodes = getColorCodeCount(startCodeSize);
            for (byte code = 0; code < colorCodes; ++code)
                codeTable.Add(new CodeTableEntry(code), code);

            codeTable.Add(new CodeTableEntry(CodeTableEntry.CodeType.Clear), (ushort)codeTable.Count);
            codeTable.Add(new CodeTableEntry(CodeTableEntry.CodeType.EndOfInformation), (ushort)codeTable.Count);
        }

        #endregion Encode

        private sealed class CodeTableEntry
        {
            public readonly byte[] Colors;

            public readonly CodeType Type;

            public CodeTableEntry(CodeType type)
            {
                Type = type;
                Colors = new byte[0];
            }

            public CodeTableEntry(params byte[] colors)
            {
                Type = CodeType.Color;
                Colors = colors;
            }

            public static bool operator !=(CodeTableEntry left, CodeTableEntry right)
            {
                return !(left == right);
            }

            public static bool operator ==(CodeTableEntry left, CodeTableEntry right)
            {
                if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
                    return true;

                if ((object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null))
                    || (!object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)))
                    return false;

                if (left.Type != right.Type
                    || left.Colors.Length != right.Colors.Length)
                    return false;

                for (var i = 0; i < left.Colors.Length; ++i)
                    if (left.Colors[i] != right.Colors[i])
                        return false;

                return true;
            }

            public override bool Equals(object obj)
            {
                var codeTableEntry = obj as CodeTableEntry;

                return this == codeTableEntry;
            }

            public override int GetHashCode()
            {
                return unchecked(Colors.GetHashCode() + (int)Type);
            }

            public override string ToString()
            {
                return Type + ": { " + string.Join(", ", Colors) + " }";
            }

            public enum CodeType
            {
                Color,
                Clear,
                EndOfInformation
            }
        }
    }
}