using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif.Extensions.ApplicationExtensions
{
    /// <summary>
    /// Represents a gif application extension used for specifying how many times the animation in it should be looped.
    /// </summary>
    public sealed class LoopingGifApplicationExtension : GifApplicationExtension
    {
        public const string AnimExtsAuthCode = "1.0";
        public const string AnimExtsIdentifier = "ANIMEXTS";

        public const string NetscapeAuthCode = "2.0";
        public const string NetscapeIdentifier = "NETSCAPE";

        private const string animExtsIdentifierAndAuth = AnimExtsIdentifier + AnimExtsAuthCode;
        private const string netscapeIdentififierAndAuth = NetscapeIdentifier + NetscapeAuthCode;
        private const byte subBlockId = 1;

        /// <summary>
        /// Gets or sets the number of times that the gif file should be looped.
        /// 0 indicates an infinite loop.
        /// </summary>
        public ushort Loops { get; set; }

        static LoopingGifApplicationExtension()
        {
            registerSpecificApplicationExtension(netscapeIdentififierAndAuth, fromBytes);
            registerSpecificApplicationExtension(animExtsIdentifierAndAuth, fromBytes);
        }

        protected override byte[] getApplicationData()
        {
            return new[]
            {
                subBlockId,
                (byte)(Loops & byte.MaxValue),
                (byte)(Loops >> 8)
            };
        }

        private static GifApplicationExtension fromBytes(byte[] data)
        {
            if (data[0] != subBlockId)
                throw new ArgumentException("Data has to start with the sub-block Id " + subBlockId + "!", "data");

            return new LoopingGifApplicationExtension
            {
                Loops = (ushort)(data[1] | (data[2] << 8))
            };
        }
    }
}