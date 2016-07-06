using System;
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

        /// <summary>
        /// Creates a new instance of the <see cref="Gif"/> class from the byte representation
        /// starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="Gif"/>.</param>
        public Gif(Stream stream)
        {
            Header = GifHeader.GetFromStream(stream);

            LogicalScreenDescriptor = new GifLogicalScreenDescriptor(stream);

            if (LogicalScreenDescriptor.HasGlobalColorTable)
                GlobalColorTable = new GifColorTable(stream, LogicalScreenDescriptor.SizeOfColorTable);

            Frames = new List<GifFrame>();
            while (stream.ReadByte() != trailer)
            {
                --stream.Position;
                Frames.Add(GifFrame.FromStream(stream, GlobalColorTable));
            }
        }

        public void ToStream(Stream stream)
        {
            Header.WriteToStream(stream);
            LogicalScreenDescriptor.ToStream(stream);

            if (LogicalScreenDescriptor.HasGlobalColorTable)
                GlobalColorTable.ToStream(stream);

            foreach (var frame in Frames)
                frame.ToStream(stream);

            stream.WriteByte(trailer);
        }
    }
}