using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif.Extensions.ApplicationExtensions
{
    /// <summary>
    /// Represents a generic gif application extension for which no specific type was found.
    /// </summary>
    public sealed class GenericGifApplicationExtension : GifApplicationExtension
    {
        /// <summary>
        /// Gets the Data for the generic application extension.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericGifApplicationExtension"/> class with the given data.
        /// </summary>
        /// <param name="data">The data of the generic application extension.</param>
        public GenericGifApplicationExtension(byte[] data)
        {
            Data = data;
        }

        protected override byte[] getApplicationData()
        {
            return Data;
        }
    }
}