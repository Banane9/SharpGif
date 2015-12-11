using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif.Extensions
{
    public sealed class GraphicControlGifExtension : GifExtension
    {
        private const byte functionCode = 0xf9;

        /// <summary>
        /// Gets or sets how many cs (100ths of a second) to wait before displaying the frame.
        /// </summary>
        public ushort DelayTime { get; set; }

        /// <summary>
        /// Gets or sets the disposal method for the frame.
        /// </summary>
        public DisposalMethod FrameDisposalMethod { get; set; }

        /// <summary>
        /// Gets or sets whether the frame has a color index that is to be treated as transparent.
        /// </summary>
        public bool HasTransparentColor { get; set; }

        /// <summary>
        /// Gets or sets whether user input is required to continue the animation.
        /// </summary>
        public bool RequireUserInput { get; set; }

        /// <summary>
        /// The color index that is to be treated as transparent if <see cref="HasTransparentColor"/> is true.
        /// </summary>
        public byte TransparentColorIndex { get; set; }

        static GraphicControlGifExtension()
        {
            registerSpecificExtension(functionCode, fromBytes);
        }

        internal static void registerSelfAsGifExtension()
        {
            registerSpecificExtension(functionCode, fromBytes);
        }

        protected override byte[] getData()
        {
            var data = new byte[4];

            data[0] = (byte)(((int)FrameDisposalMethod) << 2 | (RequireUserInput ? 1 : 0) << 1 | (HasTransparentColor ? 1 : 0));
            data[1] = (byte)(DelayTime & byte.MaxValue);
            data[2] = (byte)(DelayTime >> 8);
            data[3] = TransparentColorIndex;

            return data;
        }

        private static GifExtension fromBytes(byte[] data)
        {
            var extension = new GraphicControlGifExtension();
            var packedByte = data[0];

            // First is a packed byte
            // First three bits are reserved for future use
            // Next three bits denote the Disposal Method so shift right twice and AND with 0000 0111 to get the lowest three bits
            extension.FrameDisposalMethod = (DisposalMethod)((packedByte >> 2) & 7);
            // Next is a bit denoting whether the displaying program should wait for user input
            extension.RequireUserInput = ((packedByte >> 1) & 1) == 1;
            // Last is a bit denoting whether there is a transparent color
            extension.HasTransparentColor = (packedByte & 1) == 1;

            // Next comes to bytes in LSB-first order storing the delay time before this image is displayed
            extension.DelayTime = (ushort)((data[2] << 8) | data[1]);

            // Last a byte storing the index of the transparent color
            extension.TransparentColorIndex = data[3];

            return extension;
        }

        public enum DisposalMethod : byte
        {
            Unspecified,

            /// <summary>
            /// Previous canvas content is to be kept and the frame is to be overlayed.
            /// </summary>
            DoNothing,

            /// <summary>
            /// Canvas content is to be restored to the background color specified by the <see cref="GifLogicalScreenDescriptor"/>.
            /// </summary>
            RestoreBackground,

            /// <summary>
            /// Canvas content is to be restored to whatever it was before the previous frame was drawn.
            /// Probably not widely supported.
            /// </summary>
            RestorePrevious
        }
    }
}