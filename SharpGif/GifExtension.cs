using SharpGif.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents a generic gif extension.
    /// </summary>
    public abstract class GifExtension
    {
        /// <summary>
        /// Hex: 21; Char: '!'
        /// </summary>
        private const byte header = 0x21;

        private static Dictionary<byte, Func<byte[], GifExtension>> getSpecificExtension = new Dictionary<byte, Func<byte[], GifExtension>>();

        /// <summary>
        /// Gets the Function Code of the extension.
        /// </summary>
        public byte FunctionCode { get; set; }

        /// <summary>
        /// Gets the most specific <see cref="GifExtension"/>-derivate from the byte representation starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="GifExtension"/>.</param>
        /// <returns>A most specific <see cref="GifExtension"/>-derivate corresponding to the byte representation.</returns>
        internal static GifExtension FromStream(Stream stream)
        {
            if (stream.ReadByte() != header)
                throw new FormatException("Stream has to have an Extension coming up!");

            var functionCode = (byte)stream.ReadByte();
            var data = GifDataStream.Decode(stream);

            var extension = getSpecificExtension.ContainsKey(functionCode) ?
                getSpecificExtension[functionCode](data) : new GenericGifExtension(data);

            extension.FunctionCode = functionCode;

            return extension;
        }

        /// <summary>
        /// Checks whether the upcoming <see cref="byte"/> in the stream indicates a <see cref="GifExtension"/> and resets the position.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to check.</param>
        /// <returns>Whether the upcoming <see cref="byte"/> in the <see cref="Stream"/> indicates a <see cref="GifExtension"/>.</returns>
        internal static bool IsUpcoming(Stream stream)
        {
            return stream.PeakByte() == header;
        }

        /// <summary>
        /// Writes the byte representation of this <see cref="GifExtension"/> to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        internal void WriteToStream(Stream stream)
        {
            stream.WriteByte(header);
            stream.WriteByte(FunctionCode);
            GifDataStream.Encode(stream, getData());
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

        /// <summary>
        /// Gets the byte representation of the data of this <see cref="GifExtension"/>.
        /// </summary>
        /// <returns>The byte representation of the data of this <see cref="GifExtension"/>.</returns>
        protected abstract byte[] getData();
    }
}