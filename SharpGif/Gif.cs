using System.Collections.Generic;
using System.IO;

namespace SharpGif
{
    public sealed class Gif
    {
        /// <summary>
        /// Hex: 3B; Char: ';'
        /// </summary>
        private const byte trailer = 0x3b;

        public List<GifFrame> Frames { get; private set; }

        /// <summary>
        /// Gets the global color table of the gif image, or null if it doesn't have one (rare).
        /// </summary>
        public GifColorTable GlobalColorTable { get; private set; }

        /// <summary>
        /// Gets the Header used for the Gif.
        /// </summary>
        public GifHeader Header { get; private set; }

        /// <summary>
        /// Gets the logical screen descriptor of the gif image.
        /// </summary>
        public GifLogicalScreenDescriptor LogicalScreenDescriptor { get; private set; }

        public Gif(Stream stream)
        {
            Header = GifHeader.FromStream(stream);

            LogicalScreenDescriptor = GifLogicalScreenDescriptor.FromStream(stream);

            if (LogicalScreenDescriptor.HasGlobalColorTable)
                GlobalColorTable = GifColorTable.FromStream(stream, LogicalScreenDescriptor.SizeOfColorTable);

            Frames = new List<GifFrame>();
            while (stream.ReadByte() != trailer)
            {
                --stream.Position;
                Frames.Add(GifFrame.FromStream(stream, GlobalColorTable));
            }
        }
    }
}