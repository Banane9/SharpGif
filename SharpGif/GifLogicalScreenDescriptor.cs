using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents the logical screen decriptor after the header of a gif image/file.
    /// </summary>
    public sealed class GifLogicalScreenDescriptor
    {
        /// <summary>
        /// 7 bytes.
        /// </summary>
        private const byte size = 7;

        /// <summary>
        /// Gets the index (in the global color table) of the color used for the background of the canvas.
        /// </summary>
        public byte BackgroundColorIndex { get; private set; }

        /// <summary>
        /// Gets the height of the canvas (in pixels) that the encoded imaged will be displayed on.
        /// </summary>
        public ushort CanvasHeight { get; private set; }

        /// <summary>
        /// Gets the width of the canvas (in pixels) that the encoded images will be displayed on.
        /// </summary>
        public ushort CanvasWidth { get; private set; }

        /// <summary>
        /// Gets how many bits per primary color minus one the original image had. Unused.
        /// </summary>
        public byte ColorResolution
        {
            // 111 in binary.
            get { return 7; }
        }

        /// <summary>
        /// Gets whether the gif image/file has a global color table.
        /// </summary>
        public bool HasGlobalColorTable { get; private set; }

        /// <summary>
        /// Gets whether the gif image's/file's global color table is sorted by frequency of usage (descending).
        /// <para/>
        /// Might help the decoder, but doesn't really matter.
        /// </summary>
        public bool IsColorTableSortedByFrequency { get; private set; }

        /// <summary>
        /// Gets the Pixel Aspect Ratio. Unused.
        /// <para/>
        /// The spec says that if there was a value specified in this byte, N, the actual ratio used would be (N + 15) / 64 for all N != 0.
        /// </summary>
        public byte PixelAspectRatio
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the size of the color table. Range: 0-7 (2-256 colors).
        /// <para/>
        /// Decoder uses 2^(N+1) (where N is this number) to calculate the number of entries in the color table.
        /// </summary>
        public byte SizeOfColorTable { get; private set; }

        internal GifLogicalScreenDescriptor()
        { }

        /// <summary>
        /// Gets the <see cref="GifLogicalScreenDescriptor"/> corresponding to the byte representation.
        /// </summary>
        /// <param name="bytes">The byte representation of a <see cref="GifLogicalScreenDescriptor"/>.</param>
        /// <returns>A <see cref="GifLogicalScreenDescriptor"/> corresponding to the byte representation.</returns>
        internal static GifLogicalScreenDescriptor FromBytes(byte[] bytes)
        {
            if (bytes.Length != size)
                throw new ArgumentOutOfRangeException("bytes", "Has to be " + size + " bytes long!");

            // Reverse the transformations of GetBytes()
            return new GifLogicalScreenDescriptor
            {
                CanvasWidth = (ushort)(bytes[0] | (bytes[1] << 8)),
                CanvasHeight = (ushort)(bytes[2] | (bytes[3] << 8)),
                HasGlobalColorTable = (bytes[4] & 0x80) > 0,
                // ColorResolution gets skipped
                IsColorTableSortedByFrequency = (bytes[4] & 0x8) > 0,
                SizeOfColorTable = (byte)(bytes[4] & 0x7),
                BackgroundColorIndex = bytes[5],
                // PixelAspectRatio gets skipped
            };
        }

        /// <summary>
        /// Gets the byte representation of the <see cref="GifLogicalScreenDescriptor"/>.
        /// </summary>
        /// <returns>Byte array containing the byte representation of the <see cref="GifLogicalScreenDescriptor"/>.</returns>
        internal byte[] GetBytes()
        {
            var bytes = new byte[size];

            // All byte representations of numbers are LSB (least significant bit) first
            // First the canvas width
            // For example 1920 in binary is 0000 0111  1000 0000 (MSB first), when AND-ed with 0000 0000  1111 1111 it will only leave 0000 0000  1000 0000
            bytes[0] = (byte)(CanvasWidth & byte.MaxValue);
            // Simply shift the more significant byte down
            bytes[1] = (byte)(CanvasWidth >> 8);

            // Then the same for the canvas height
            bytes[2] = (byte)(CanvasHeight & byte.MaxValue);
            bytes[3] = (byte)(CanvasHeight >> 8);

            // Now comes a packed byte
            // First bit is a flag for the global color table. 0x80 is 1000 0000
            bytes[4] |= (byte)(HasGlobalColorTable ? 0x80 : 0);
            // Three bits for the color resolution. Max value of 7. AND-ing with 0x7 sets the first 5 bits to 0, just in case.
            bytes[4] |= (byte)((ColorResolution & 0x7) << 4);
            // Flag bit for sorted color table. 0x8 is 0000 1000
            bytes[4] |= (byte)(IsColorTableSortedByFrequency ? 0x8 : 0);
            // Last three bits are for the size of the global color table. Max value of 7. AND-ing with 0x7 sets the first 5 bits to 0, just in case.
            bytes[4] |= (byte)(SizeOfColorTable & 0x7);

            // Then comes the index for the background color
            bytes[5] = BackgroundColorIndex;

            // Lastly, the pixel aspect ratio
            bytes[6] = PixelAspectRatio;

            return bytes;
        }
    }
}