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
        /// Gets the <see cref="GifExtension"/> preceding this frame.
        /// </summary>
        public GifExtension Extension { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="GifFrame"/> class from the byte representation
        /// starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="GifFrame"/>.</param>
        /// <param name="globalColorTable">The global color table of the Gif.</param>
        internal GifFrame(Stream stream, GifColorTable globalColorTable)
        {
            if (GifExtension.IsUpcoming(stream))
                Extension = GifExtension.FromStream(stream);

            Descriptor = new GifFrameDescriptor(stream);
            ColorTable = Descriptor.HasLocalColorTable ? new GifColorTable(stream, Descriptor.SizeOfColorTable) : globalColorTable;
            ColorData = GifLZW.Decode(stream);
        }

        /// <summary>
        /// Writes the byte representation of this <see cref="GifFrame"/> to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        internal void WriteToStream(Stream stream)
        {
            if (Extension != null)
                Extension.WriteToStream(stream);

            Descriptor.WriteToStream(stream);

            if (Descriptor.HasLocalColorTable)
                ColorTable.WriteToStream(stream);

            GifLZW.Encode(stream, ColorData, GifColorTable.GetNumberOfEntries(ColorTable.ScreenDescriptorSize));
        }
    }
}