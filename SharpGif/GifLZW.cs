using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Contains methods for decoding and encoding of Gif-LZW compressed image data.
    /// </summary>
    public static class GifLZW
    {
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

            var gifDataStream = GifDataStream.FromStream(stream);
            var codeStream = new BitStream.BitStream(new MemoryStream(gifDataStream.ToArray()));

            var indexStream = new List<byte>();
            var codeTable = new List<CodeTableEntry>();

            var codeSize = startCodeSize + 1u;
            var indexBuffer = new List<byte>();
            CodeTableEntry.CodeType codeType;
            ushort prevCode = getColorCodeCount(startCodeSize);
            do
            {
                var codeBytes = new byte[(int)Math.Ceiling(codeSize / 8f)];
                codeStream.ReadBits(codeBytes, 0, codeSize);
                var code = BitConverter.ToUInt16(codeBytes, 0);

                if (code < codeTable.Count)
                {
                    codeType = codeTable[code].Type;

                    switch (codeType)
                    {
                        case CodeTableEntry.CodeType.Color:
                            indexStream.AddRange(codeTable[code].Colors);

                            indexBuffer.Clear();
                            indexBuffer.AddRange(codeTable[prevCode].Colors);
                            indexBuffer.Add(codeTable[code].Colors[0]);

                            codeTable.Add(new CodeTableEntry(indexBuffer.ToArray()));
                            break;

                        case CodeTableEntry.CodeType.Clear:
                            indexBuffer.Clear();
                            buildCodeTable(codeTable, startCodeSize);
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

                if (codeSize <= Math.Floor(Math.Log(code + 1, 2)))
                    ++codeSize;

                prevCode = code;
            }
            while (codeType != CodeTableEntry.CodeType.EndOfInformation);

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
            return (byte)(Math.Pow(2, startCodeSize) - 1);
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