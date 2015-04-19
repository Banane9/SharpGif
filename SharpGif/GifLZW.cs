using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Contains methods for decoding and encoding of Gif-LZW compressed image data.
    /// </summary>
    public static class GifLZW
    {
        /// <summary>
        /// Gets the decompressed  Color Data starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the compressed Color Data.</param>
        /// <returns>The decompressed Color Data.</returns>
        public static byte[] Decode(Stream stream)
        {
        }

        /// <summary>
        /// Writes the compressed version of the given Color Data to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="colorData">The Color Data to compress.</param>
        /// <param name="colorTableSize">Number of entries in the used color table.</param>
        public static void Encode(Stream stream, byte[] colorData, ushort colorTableSize)
        {
        }
    }
}