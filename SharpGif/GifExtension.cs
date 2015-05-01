using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents a generic gif extension.
    /// </summary>
    public class GifExtension
    {
        /// <summary>
        /// Hex: 21; Char: '!'
        /// </summary>
        private const byte header = 0x21;

        private static Dictionary<byte, Func<byte[], GifExtension>> getSpecificExtension = new Dictionary<byte, Func<byte[], GifExtension>>();

        /// <summary>
        /// Gets the Data for the extension.
        /// </summary>
        public byte[] Data { get; protected set; }

        /// <summary>
        /// Gets the Function Code of the extension.
        /// </summary>
        public byte FunctionCode { get; private set; }

        internal static GifExtension FromStream(Stream stream)
        {
            if (stream.ReadByte() != header)
                throw new FormatException("Stream has to have an Extension coming up!");

            var functionCode = (byte)stream.ReadByte();
            var data = GifDataStream.FromStream(stream).ToArray();

            if (getSpecificExtension.ContainsKey(functionCode))
            {
                var extension = getSpecificExtension[functionCode](data);
                extension.FunctionCode = functionCode;
                return extension;
            }

            return new GifExtension
            {
                FunctionCode = functionCode,
                Data = data
            };
        }

        /// <summary>
        /// Checks whether the upcoming <see cref="byte"/> in the stream indicates a <see cref="GifExtension"/> and resets the position.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to check.</param>
        /// <returns>Whether the upcoming <see cref="byte"/> in the <see cref="Stream"/> indicates a <see cref="GifExtension"/>.</returns>
        internal bool IsUpcoming(Stream stream)
        {
            var readByte = stream.ReadByte();
            --stream.Position;

            return readByte == header;
        }

        /// <summary>
        /// Registers the function to get an instance of the specific <see cref="GifExtension"/>-derivate for the given function code.
        /// </summary>
        /// <param name="functionCode">The function code of the extension.</param>
        /// <param name="get">A method that takes the data of the extension and uses it to return an instance of the specific <see cref="GifExtension"/>-derivate.</param>
        protected static void registerSpecificExtension(byte functionCode, Func<byte[], GifExtension> get)
        {
            getSpecificExtension.Add(functionCode, get);
        }
    }
}