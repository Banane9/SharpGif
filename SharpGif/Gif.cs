namespace SharpGif
{
    public sealed class Gif
    {
        /// <summary>
        /// Gets the height of the gif canvas that the encoded imaged will be displayed on.
        /// </summary>
        public ushort CanvasHeight { get; private set; }

        /// <summary>
        /// Gets the width of the gif canvas that the encoded images will be displayed on.
        /// </summary>
        public ushort CanvasWidth { get; private set; }

        /// <summary>
        /// Gets the Global Color Table of the gif image, or null if it doesn't have one (rare).
        /// </summary>
        public GifColorTable GlobalColorTable { get; private set; }

        /// <summary>
        /// Gets the Header used for the Gif.
        /// </summary>
        public GifHeader Header { get; private set; }
    }
}