using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents a color table in a gif file/image.
    /// </summary>
    public sealed class GifColorTable : IEnumerable<GifColorTable.Color>, IList<GifColorTable.Color>
    {
        /// <summary>
        /// Maximum number of entries in a color table.
        /// </summary>
        public const byte MaxSize = byte.MaxValue;

        private readonly List<Color> colors = new List<Color>();

        /// <summary>
        /// Gets whether there's free space left in the color table.
        /// </summary>
        public bool HasSpace
        {
            get { return colors.Count < MaxSize; }
        }

        /// <summary>
        /// Gets the size number for the screen descriptor, used by the Encoder.
        /// <para/>
        /// Decoder uses 2^(N+1) (where N is this number) to calculate the number of entries in the color table.
        /// </summary>
        internal byte ScreenDescriptorSize
        {
            get { return (byte)Math.Floor(Math.Sqrt(colors.Count) - 1); }
        }

        #region IList

        public int Count
        {
            get { return colors.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public GifColorTable.Color this[int index]
        {
            get { return colors[index]; }
            set
            {
                if (index > MaxSize || index < 0)
                    throw new ArgumentOutOfRangeException("index", "Index must be between 0 and 255!");

                colors[index] = value;
            }
        }

        public void Add(GifColorTable.Color item)
        {
            if (colors.Count >= MaxSize)
                throw new Exception("Color Table is full!");

            colors.Add(item);
        }

        public void Clear()
        {
            colors.Clear();
        }

        public bool Contains(GifColorTable.Color item)
        {
            return colors.Contains(item);
        }

        public void CopyTo(GifColorTable.Color[] array, int arrayIndex)
        {
            colors.CopyTo(array, arrayIndex);
        }

        public int IndexOf(GifColorTable.Color item)
        {
            return colors.IndexOf(item);
        }

        public void Insert(int index, GifColorTable.Color item)
        {
            if (colors.Count >= MaxSize)
                throw new Exception("Color Table is full!");

            colors.Insert(index, item);
        }

        public bool Remove(GifColorTable.Color item)
        {
            return colors.Remove(item);
        }

        public void RemoveAt(int index)
        {
            colors.RemoveAt(index);
        }

        #endregion IList

        #region IEnumerable

        public IEnumerator<GifColorTable.Color> GetEnumerator()
        {
            return colors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable

        /// <summary>
        /// Gets the <see cref="GifColorTable"/> corresponding to the byte representation.
        /// </summary>
        /// <param name="bytes">The byte representation of a <see cref="GifColorTable"/>.</param>
        /// <returns>A <see cref="GifColorTable"/> corresponding to the byte representation.</returns>
        internal static GifColorTable FromBytes(byte[] bytes)
        {
            var colorTable = new GifColorTable();

            for (var i = 0; i < bytes.Length; i += 3)
                colorTable.Add(new Color(bytes[i], bytes[i + 1], bytes[i + 2]));

            return colorTable;
        }

        /// <summary>
        /// Gets the byte representation of the <see cref="GifColorTable"/>.
        /// </summary>
        /// <returns>Byte array containing the byte representation of the <see cref="GifColorTable"/>.</returns>
        internal byte[] GetBytes()
        {
            // 3 bytes per entry.
            var bytes = new byte[(int)Math.Pow(2, ScreenDescriptorSize + 1) * 3];

            var i = 0;
            foreach (var color in colors)
            {
                ++i;
                bytes[i] = color.R;
                bytes[i + 1] = color.G;
                bytes[i + 2] = color.B;
            }

            return bytes;
        }

        /// <summary>
        /// Represents an entry in the color table.
        /// </summary>
        public struct Color
        {
            public readonly byte B;
            public readonly byte G;
            public readonly byte R;

            public Color(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }
        }
    }
}