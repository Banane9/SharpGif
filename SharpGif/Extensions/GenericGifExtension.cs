using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif.Extensions
{
    /// <summary>
    /// Represents a generic gif extension for which no specific type was found.
    /// </summary>
    public sealed class GenericGifExtension : GifExtension
    {
        /// <summary>
        /// Gets the Data for the generic extension.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericGifExtension"/> class with the given data.
        /// </summary>
        /// <param name="data">The data of the generic extension.</param>
        public GenericGifExtension(byte[] data)
        {
            Data = data;
        }

        protected override byte[] getData()
        {
            return Data;
        }
    }
}