using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents a data block that is divided into sub-blocks in the actual gif image/file.
    /// </summary>
    internal static class GifDataStream
    {
        private const byte maxSubBlockLength = byte.MaxValue;

        /// <summary>
        /// Gets the byte data from the gif data stream starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the gif data stream formatted data.</param>
        /// <returns>The byte data.</returns>
        internal static byte[] FromStream(Stream stream)
        {
            var data = new List<byte>();

            var nextBlockLength = stream.ReadByte();
            while (nextBlockLength > 0)
            {
                var subBlock = new byte[nextBlockLength];
                stream.Read(subBlock, 0, nextBlockLength);

                data.AddRange(subBlock);

                nextBlockLength = stream.ReadByte();
            }

            return data.ToArray();
        }

        /// <summary>
        /// Writes the gif data stream formatted data to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="data">The byte data to format.</param>
        internal static void ToStream(Stream stream, byte[] data)
        {
            var fullSubBlocks = (int)Math.Floor(data.Length / (float)maxSubBlockLength);
            var trailingLength = (byte)(data.Length % maxSubBlockLength);

            var dataBytes = data.ToArray();
            for (var i = 0; i < fullSubBlocks; i += maxSubBlockLength)
            {
                stream.WriteByte(maxSubBlockLength);
                stream.Write(dataBytes, i, maxSubBlockLength);
            }

            if (trailingLength > 0)
            {
                stream.WriteByte(trailingLength);
                stream.Write(dataBytes, fullSubBlocks * maxSubBlockLength, trailingLength);
            }

            stream.WriteByte(0);
        }
    }
}