using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents a frame in the gif image/file.
    /// </summary>
    public sealed class GifFrame
    {
        /// <summary>
        /// Gets the indexes into the color table for each pixel.
        /// <para/>
        /// Index for each pixel can be found with: (offsetTop * width) + offsetLeft
        /// </summary>
        public byte[] ColorData { get; private set; }

        /// <summary>
        /// Gets the color table for this frame, or null if it doesn't have one.
        /// </summary>
        public GifColorTable ColorTable { get; private set; }

        /// <summary>
        /// Gets the descriptor for this frame.
        /// </summary>
        public GifFrameDescriptor Descriptor { get; private set; }

        /// <summary>
        /// Gets the <see cref="GifFrame"/> from the byte representation starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="GifFrame"/>.</param>
        /// <returns>A <see cref="GifFrame"/> corresponding to the byte representation.</returns>
        internal static GifFrame FromStream(Stream stream, GifColorTable globalColorTable)
        {
            var gifFrame = new GifFrame();

            // Add Graphic Control Extension
            gifFrame.Descriptor = GifFrameDescriptor.FromStream(stream);
            gifFrame.ColorTable = gifFrame.Descriptor.HasLocalColorTable ? GifColorTable.FromStream(stream, gifFrame.Descriptor.SizeOfColorTable) : globalColorTable;
            gifFrame.ColorData = GifLZW.Decode(stream);

            return gifFrame;
        }

        /// <summary>
        /// Writes the byte representation of this <see cref="GifFrame"/> to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        internal void ToStream(Stream stream)
        {
            // Add Graphic Control Extension
            Descriptor.ToStream(stream);

            if (Descriptor.HasLocalColorTable)
                ColorTable.ToStream(stream);

            GifLZW.Encode(stream, ColorData, GifColorTable.GetNumberOfEntries(ColorTable.ScreenDescriptorSize));
        }
    }
}