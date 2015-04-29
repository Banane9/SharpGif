using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        internal static GifHeader FromStream(Stream stream)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);

            var gif89aBytes = Gif89a.getBytes();
            var isGif89a = true;
            for (var i = 0; i < buffer.Length; ++i)
                if (buffer[i] != gif89aBytes[i])
                {
                    isGif89a = false;
                    break;
                }

            if (isGif89a)
                return Gif89a;

            var gif87aBytes = Gif87a.getBytes();
            var isGif87a = true;
            for (var i = 0; i < buffer.Length; ++i)
                if (buffer[i] != gif87aBytes[length])
                {
                    isGif87a = false;
                    break;
                }

            if (isGif87a)
                return Gif87a;

            throw new ArgumentOutOfRangeException("stream", "Header not recognized!");
        }

        /// <summary>
        /// Writes the byte representation of this <see cref="GifHeader"/> to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        internal void ToStream(Stream stream)
        {
            stream.Write(getBytes(), 0, length);
        }

        private byte[] getBytes()
        {
            return ToString().Cast<byte>().ToArray();
        }

        private class Version87a : GifHeader
        {
            public override string Version
            {
                get { return "87a"; }
            }
        }

        private class Version89a : GifHeader
        {
            public override string Version
            {
                get { return "89a"; }
            }
        }
    }
}