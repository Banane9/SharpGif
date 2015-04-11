using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents the image/frame descriptor at the beginning of every frame in a gif image/file.
    /// </summary>
    public sealed class GifFrameDescriptor
    {
        /// <summary>
        /// Hex: 2C; Char: ','
        /// </summary>
        private const char separator = ',';

        /// <summary>
        /// 10 bytes.
        /// </summary>
        private const byte size = 10;

        /// <summary>
        /// Gets whether the frame has a local color table or not.
        /// </summary>
        public bool HasLocalColorTable { get; private set; }

        /// <summary>
        /// Gets the height of the frame (in pixels).
        /// </summary>
        public ushort Height { get; private set; }

        /// <summary>
        /// Gets whether the gif image's/file's global color table is sorted by frequency of usage (descending).
        /// <para/>
        /// Might help the decoder, but doesn't really matter.
        /// </summary>
        public bool IsColorTableSortedByFrequency { get; private set; }

        /// <summary>
        /// Gets whether the frame's rows are in interlaced order.
        /// <para/>
        /// Interlaced order is:
        /// * Every 8th row starting from 0
        /// * Every 8th row starting from 4
        /// * Every 4th row starting from 2
        /// * Every 2nd row starting from 1
        /// </summary>
        public bool IsInterlaced { get; private set; }

        /// <summary>
        /// Gets the offset from the left border at which the frame starts.
        /// </summary>
        public ushort LeftOffset { get; private set; }

        /// <summary>
        /// Gets the size of the color table. Range: 0-7 (2-256 colors).
        /// <para/>
        /// Decoder uses 2^(N+1) (where N is this number) to calculate the number of entries in the color table.
        /// </summary>
        public byte SizeOfColorTable { get; private set; }

        /// <summary>
        /// Gets the offset from the top border at which the frame starts.
        /// </summary>
        public ushort TopOffset { get; private set; }

        /// <summary>
        /// Gets the width of the frame (in pixels).
        /// </summary>
        public ushort Width { get; private set; }

        internal GifFrameDescriptor()
        { }

        /// <summary>
        /// Gets the <see cref="GifFrameDescriptor"/> corresponding to the byte representation.
        /// </summary>
        /// <param name="bytes">The byte representation of a <see cref="GifFrameDescriptor"/>.</param>
        /// <returns>A <see cref="GifFrameDescriptor"/> corresponding to the byte representation.</returns>
        internal static GifFrameDescriptor FromBytes(byte[] bytes)
        {
            if (bytes.Length != size)
                throw new ArgumentOutOfRangeException("bytes", "Has to be " + size + " bytes long!");

            if (bytes[0] != (byte)separator)
                throw new FormatException("Bytes have to start with the image separator char/byte value.");

            // Reverse the transformations of GetBytes()
            return new GifFrameDescriptor
            {
                LeftOffset = (ushort)(bytes[1] | (bytes[2] << 8)),
                TopOffset = (ushort)(bytes[3] | (bytes[4] << 8)),
                Width = (ushort)(bytes[5] | (bytes[6] << 8)),
                Height = (ushort)(bytes[7] | (bytes[8] << 8)),
                HasLocalColorTable = (bytes[9] & 0x80) > 0,
                IsInterlaced = (bytes[9] & 0x40) > 0,
                IsColorTableSortedByFrequency = (bytes[9] & 0x20) > 0,
                SizeOfColorTable = (byte)(bytes[9] & 0x7)
            };
        }

        /// <summary>
        /// Gets the byte representation of the <see cref="GifFrameDescriptor"/>.
        /// </summary>
        /// <returns>Byte array containing the byte representation of the <see cref="GifFrameDescriptor"/>.</returns>
        internal byte[] GetBytes()
        {
            var bytes = new byte[size];

            // First the image separator
            bytes[0] = (byte)separator;

            // Then the left offset - for more detailed explanation of the bit working, see same method in GifLogicalScreenDescriptor
            bytes[1] = (byte)(LeftOffset & byte.MaxValue);
            bytes[2] = (byte)(LeftOffset >> 8);

            // Then the top offset
            bytes[3] = (byte)(TopOffset & byte.MaxValue);
            bytes[4] = (byte)(TopOffset >> 8);

            // Then the image width
            bytes[5] = (byte)(Width & byte.MaxValue);
            bytes[6] = (byte)(Width >> 8);

            // Then the image height
            bytes[7] = (byte)(Height & byte.MaxValue);
            bytes[8] = (byte)(Height >> 8);

            // Now comes a packed byte
            // First bit is a flag for the local color table
            bytes[9] |= (byte)(HasLocalColorTable ? 0x80 : 0);
            // Next bit is a flag for interlacing
            bytes[9] |= (byte)(IsInterlaced ? 0x40 : 0);
            // Next bit is a flag for sorted color table
            bytes[9] |= (byte)(IsColorTableSortedByFrequency ? 0x20 : 0);
            // Next two bits are reserved for future use
            // Last three bits are for the size of the global color table. Max value of 7.
            bytes[9] |= (byte)(SizeOfColorTable & 0x7);

            return bytes;
        }
    }
}