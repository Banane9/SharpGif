namespace SharpGif
{
    public sealed class Gif
    {
        internal const char Trailer = ';';

        /// <summary>
        /// Gets the Global Color Table of the gif image, or null if it doesn't have one (rare).
        /// </summary>
        public GifColorTable GlobalColorTable { get; private set; }

        /// <summary>
        /// Gets the Header used for the Gif.
        /// </summary>
        public GifHeader Header { get; private set; }

        /// <summary>
        /// The logical screen descriptor of the gif image.
        /// </summary>
        public GifLogicalScreenDescriptor LogicalScreenDescriptor { get; private set; }
    }
}