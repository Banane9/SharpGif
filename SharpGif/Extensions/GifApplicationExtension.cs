using SharpGif.Extensions.ApplicationExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif.Extensions
{
    /// <summary>
    /// Represents a generic gif application extension (a kind of gif extension).
    /// </summary>
    public abstract class GifApplicationExtension : GifExtension
    {
        /// <summary>
        /// 3 bytes.
        /// </summary>
        private const byte authCodeLength = 3;

        private const byte functionCode = 255;

        /// <summary>
        /// 8 bytes.
        /// </summary>
        private const byte identifierLength = 8;

        private static readonly Dictionary<string, Func<byte[], GifApplicationExtension>> getSpecificApplicationExtension =
            new Dictionary<string, Func<byte[], GifApplicationExtension>>();

        /// <summary>
        /// Gets the Application Authentication Code for the application extension.
        /// </summary>
        public string AuthenticationCode { get; private set; }

        /// <summary>
        /// Gets the Application Identifier for the application extension.
        /// </summary>
        public string Identifier { get; private set; }

        internal static void registerSelfAsGifExtension()
        {
            registerSpecificExtension(functionCode, fromBytes);
        }

        /// <summary>
        /// Registers the function to get an instance of the specific <see cref="GifApplicationExtension"/>-derivate for the given identifier and authentication code.
        /// </summary>
        /// <param name="identifierAndAuth">The identifier and authentication code of the application extension. Has to be exactly 11 8-bit characters long.</param>
        /// <param name="get">A method that takes the data of the application extension and uses it to return an instance of the specific <see cref="GifApplicationExtension"/>-derivate.</param>
        protected static void registerSpecificApplicationExtension(string identifierAndAuth, Func<byte[], GifApplicationExtension> get)
        {
            getSpecificApplicationExtension.Add(identifierAndAuth, get);
        }

        /// <summary>
        /// Gets the byte representation of the data of this <see cref="GifApplicationExtension"/>.
        /// </summary>
        /// <returns>The byte representation of the data of this <see cref="GifApplicationExtension"/>.</returns>
        protected abstract byte[] getApplicationData();

        protected override byte[] getData()
        {
            var data = new List<byte>();
            data.AddRange(Identifier.Cast<byte>());
            data.AddRange(AuthenticationCode.Cast<byte>());
            data.AddRange(getApplicationData());

            return data.ToArray();
        }

        private static GifExtension fromBytes(byte[] bytes)
        {
            var identifierAndAuth = new string(bytes.Take(identifierLength + authCodeLength).Cast<char>().ToArray());
            var data = bytes.Skip(identifierLength + authCodeLength).ToArray();

            var identifier = identifierAndAuth.Substring(0, identifierLength);
            var authCode = identifierAndAuth.Substring(identifierLength);

            var extension = getSpecificApplicationExtension.ContainsKey(identifierAndAuth) ?
                getSpecificApplicationExtension[identifierAndAuth](data) : new GenericGifApplicationExtension(data);

            extension.Identifier = identifier;
            extension.AuthenticationCode = authCode;

            return extension;
        }
    }
}