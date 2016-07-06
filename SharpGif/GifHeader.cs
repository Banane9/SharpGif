using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpGif
{
    /// <summary>
    /// Represents the header of a gif image/file.
    /// </summary>
    public abstract class GifHeader
    {
        /// <summary>
        /// Gets the Header Information for Gif 89a.
        /// </summary>
        public static readonly GifHeader Gif87a = new Version87a();

        /// <summary>
        /// Gets the Header Information for Gif 89a.
        /// </summary>
        public static readonly GifHeader Gif89a = new Version89a();

        private const int length = 6;

        /// <summary>
        /// Gets the Signature of the Gif Header.
        /// </summary>
        public string Signature
        {
            get { return "GIF"; }
        }

        /// <summary>
        /// Gets the Version of the Gif Header.
        /// </summary>
        public abstract string Version { get; }

        private GifHeader()
        { }

        public override string ToString()
        {
            return Signature + Version;
        }

        /// <summary>
        /// Gets the <see cref="GifHeader"/> from the byte representation starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="GifHeader"/>.</param>
        /// <returns>A <see cref="GifHeader"/> corresponding to the byte representation.</returns>
        internal static GifHeader GetFromStream(Stream stream)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);

            var header = Encoding.UTF8.GetString(buffer, 0, length);

            if (header == Gif89a.ToString())
                return Gif89a;

            if (header == Gif87a.ToString())
                return Gif87a;

            throw new ArgumentOutOfRangeException("stream", "Header not recognized!");
        }

        /// <summary>
        /// Writes the byte representation of this <see cref="GifHeader"/> to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        internal void WriteToStream(Stream stream)
        {
            stream.Write(Encoding.UTF8.GetBytes(ToString()), 0, length);
        }

        private sealed class Version87a : GifHeader
        {
            public override string Version
            {
                get { return "87a"; }
            }
        }

        private sealed class Version89a : GifHeader
        {
            public override string Version
            {
                get { return "89a"; }
            }
        }
    }
}