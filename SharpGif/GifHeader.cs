using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets the Signature of the Gif Header.
        /// </summary>
        public string Signature
        {
            get { return "GIF"; }
        }

        /// <summary>
        /// Gets the Version fo the Gif Header.
        /// </summary>
        public abstract string Version { get; }

        internal GifHeader()
        { }

        public override string ToString()
        {
            return Signature + Version;
        }

        /// <summary>
        /// Gets the <see cref="GifHeader"/> corresponding to the byte representation.
        /// </summary>
        /// <param name="bytes">The byte representation of a <see cref="GifHeader"/>.</param>
        /// <returns>A <see cref="GifHeader"/> corresponding to the byte representation.</returns>
        internal GifHeader FromBytes(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("headerBytes", "Header Bytes can't be null!");

            if (bytes == Gif87a.GetBytes())
                return Gif87a;
            else if (bytes == Gif89a.GetBytes())
                return Gif89a;
            else
                throw new ArgumentOutOfRangeException("headerBytes", "Header not recognized!");
        }

        /// <summary>
        /// Gets the byte representation of the <see cref="GifHeader"/>.
        /// </summary>
        /// <returns>Byte array containing the byte representation of the <see cref="GifHeader"/>.</returns>
        internal byte[] GetBytes()
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