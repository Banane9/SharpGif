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
            var codeData = GifDataStream.Decode(stream);
            using (var codeStream = new BitStream.BitStream(new MemoryStream(codeData)))
            {
                var codeTable = new List<CodeTableEntry>();
                buildCodeTable(codeTable, startCodeSize);

                var codeSize = startCodeSize + 1u;
                var indexBuffer = new List<byte>();
                CodeTableEntry codeEntry;
                ushort prevCode = getColorCodeCount(startCodeSize);
                var first = true;
                do
                {
                    var codeBytes = new byte[2];
                    codeStream.ReadBits(codeBytes, 0, codeSize);
                    var code = BitConverter.ToUInt16(codeBytes, 0);

                    if (code < codeTable.Count)
                    {
                        codeEntry = codeTable[code];

                        if (codeEntry == CodeTableEntry.Clear)
                        {
                            indexBuffer.Clear();
                            buildCodeTable(codeTable, startCodeSize);
                            first = true;
                        }
                        else if (codeEntry != CodeTableEntry.EndOfInformation)
                        {
                            indexStream.AddRange(codeTable[code].Colors);

                            if (!first)
                            {
                                indexBuffer.Clear();
                                indexBuffer.AddRange(codeTable[prevCode].Colors);
                                indexBuffer.Add(codeTable[code].Colors[0]);

                                codeTable.Add(CodeTableEntry.MakeColorEntry(indexBuffer.ToArray()));
                            }

                            first = false;
                        }
                    }
                    else if (code == codeTable.Count)
                    {
                        indexBuffer.Clear();
                        indexBuffer.AddRange(codeTable[prevCode].Colors);
                        indexBuffer.Add(codeTable[prevCode].Colors[0]);

                        codeEntry = CodeTableEntry.MakeColorEntry(indexBuffer.ToArray());
                        codeTable.Add(codeEntry);

                        indexStream.AddRange(codeTable[code].Colors);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("code", "Encoded code was too large. Expected: " + codeTable.Count + " or less; got: " + code);
                    }

                    if (Math.Pow(2, codeSize) <= codeTable.Count)
                        ++codeSize;

                    prevCode = code;
                }
                while (codeEntry != CodeTableEntry.EndOfInformation);
            }

            return indexStream.ToArray();
        }

        private static void buildCodeTable(List<CodeTableEntry> codeTable, byte startCodeSize)
        {
            codeTable.Clear();

            // startCodeSize has a max of 8, so it will always fit into a byte
            var colorCodes = getColorCodeCount(startCodeSize);
            for (byte code = 0; code < colorCodes; ++code)
                codeTable.Add(CodeTableEntry.MakeColorEntry(code));

            codeTable.Add(CodeTableEntry.Clear);
            codeTable.Add(CodeTableEntry.EndOfInformation);
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
                buildCodeTable(codeTable, startCodeSize);

                var codeSize = (byte)(startCodeSize + 1);
                codeStream.WriteBits(BitConverter.GetBytes(codeTable[CodeTableEntry.Clear]), 0, codeSize);

                var indexBuffer = new List<byte>(colorData.Take(1));
                CodeTableEntry tableEntry = null;
                foreach (var color in colorData.Skip(1))
                {
                    if (codeSize > maxCodeSize)
                    {
                        codeStream.WriteBits(BitConverter.GetBytes(codeTable[CodeTableEntry.Clear]), 0, maxCodeSize);
                        buildCodeTable(codeTable, startCodeSize);

                        codeSize = (byte)(startCodeSize + 1);
                    }

                    var prevIndexBuffer = indexBuffer.ToArray();

                    indexBuffer.Add(color);
                    tableEntry = CodeTableEntry.MakeColorEntry(indexBuffer.ToArray());

                    if (codeTable.ContainsKey(tableEntry))
                        continue;

                    codeTable.Add(tableEntry, (ushort)codeTable.Count);

                    var bytes = BitConverter.GetBytes(codeTable[CodeTableEntry.MakeColorEntry(prevIndexBuffer)]);
                    codeStream.WriteBits(bytes, 0, codeSize);

                    indexBuffer.Clear();
                    indexBuffer.Add(color);

                    if ((Math.Pow(2, codeSize) + 1) <= codeTable.Count)
                        ++codeSize;
                }

                if (codeTable.ContainsKey(tableEntry))
                    codeStream.WriteBits(BitConverter.GetBytes(codeTable[tableEntry]), 0, codeSize);
                else
                    codeStream.WriteBits(BitConverter.GetBytes(codeTable[CodeTableEntry.MakeColorEntry(indexBuffer.ToArray())]), 0, codeSize);

                codeStream.WriteBits(BitConverter.GetBytes(codeTable[CodeTableEntry.EndOfInformation]), 0, codeSize);
                codeStream.WriteBits(0, (BitNum)(BitNum.MaxValue - codeStream.BitPosition));

                var codes = ((MemoryStream)codeStream.UnderlayingStream).ToArray();
                GifDataStream.Encode(stream, codes);
            }
        }

        private static void buildCodeTable(Dictionary<CodeTableEntry, ushort> codeTable, byte startCodeSize)
        {
            codeTable.Clear();

            // startCodeSize has a max of 8, so it will always fit into a byte
            var colorCodes = getColorCodeCount(startCodeSize);
            for (byte code = 0; code < colorCodes; ++code)
                codeTable.Add(CodeTableEntry.MakeColorEntry(code), code);

            codeTable.Add(CodeTableEntry.Clear, (ushort)codeTable.Count);
            codeTable.Add(CodeTableEntry.EndOfInformation, (ushort)codeTable.Count);
        }

        #endregion Encode

        private abstract class CodeTableEntry
        {
            public static CodeTableEntry Clear { get; } = new ClearCodeTableEntry();
            public static CodeTableEntry EndOfInformation { get; } = new EndOfInformationCodeTableEntry();

            public abstract byte[] Colors { get; }

            private CodeTableEntry()
            { }

            public static CodeTableEntry MakeColorEntry(params byte[] colors)
            {
                return new ColorCodeTableEntry(colors);
            }

            private sealed class ClearCodeTableEntry : CodeTableEntry
            {
                public override byte[] Colors
                {
                    get { throw new NotSupportedException(); }
                }

                public override string ToString()
                {
                    return "Clear";
                }
            }

            private sealed class ColorCodeTableEntry : CodeTableEntry
            {
                public override byte[] Colors { get; }

                public ColorCodeTableEntry(params byte[] colors)
                {
                    Colors = colors;
                }

                public static bool operator !=(ColorCodeTableEntry left, ColorCodeTableEntry right)
                {
                    return !(left == right);
                }

                public static bool operator ==(ColorCodeTableEntry left, ColorCodeTableEntry right)
                {
                    if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
                        return true;

                    if ((object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null))
                        || (!object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)))
                        return false;

                    if (left.Colors.Length != right.Colors.Length)
                        return false;

                    for (var i = 0; i < left.Colors.Length; ++i)
                        if (left.Colors[i] != right.Colors[i])
                            return false;

                    return true;
                }

                public override bool Equals(object obj)
                {
                    var codeTableEntry = obj as ColorCodeTableEntry;

                    return this == codeTableEntry;
                }

                public override int GetHashCode()
                {
                    var hashcode = 0;
                    unchecked
                    {
                        foreach (var color in Colors)
                            hashcode = hashcode * 13 + color;
                    }

                    return hashcode;
                }

                public override string ToString()
                {
                    return $"Colors: {{ {string.Join(", ", Colors)} }}";
                }
            }

            private sealed class EndOfInformationCodeTableEntry : CodeTableEntry
            {
                public override byte[] Colors
                {
                    get { throw new NotSupportedException(); }
                }

                public override string ToString()
                {
                    return "EndOfInformation";
                }
            }
        }
    }
}