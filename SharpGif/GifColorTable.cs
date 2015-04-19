using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents a color table in a gif file/image.
    /// </summary>
    public sealed class GifColorTable : IList<GifColorTable.Color>
    {
        /// <summary>
        /// Maximum number of entries in a color table.
        /// </summary>
        public const ushort MaxSize = byte.MaxValue + 1;

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
            get { return GetSceenDescriptorSize((ushort)colors.Count); }
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

        public IEnumerator<GifColorTable.Color> GetEnumerator()
        {
            return colors.GetEnumerator();
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IList

        /// <summary>
        /// Gets the <see cref="GifColorTable"/> from the byte representation starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="GifColorTable"/>.</param>
        /// <param name="length">The number of entries in the color table.</param>
        /// <returns>A <see cref="GifColorTable"/> corresponding to the byte representation.</returns>
        internal static GifColorTable FromStream(Stream stream, ushort length)
        {
            if (length > MaxSize)
                throw new ArgumentOutOfRangeException("length", "Color Table can only have a maximum of " + MaxSize + " entries!");

            var colorTable = new GifColorTable();

            for (var i = 0; i < length; ++i)
                colorTable.Add(Color.FromStream(stream));

            return colorTable;
        }

        /// <summary>
        /// Calculates the number of entries that the color table will have from the given screen descriptor size.
        /// </summary>
        /// <param name="screenDescriptorSize">The size given in the screen descriptor.</param>
        /// <returns>The number of entries in the color table.</returns>
        internal static ushort GetNumberOfEntries(byte screenDescriptorSize)
        {
            return (ushort)Math.Pow(2, screenDescriptorSize + 1);
        }

        /// <summary>
        /// Calculates the screen descriptor size from the number of entries in the color table.
        /// </summary>
        /// <param name="numberOfEntries">The number of entries in the color table.</param>
        /// <returns>The screen descriptor size.</returns>
        internal static byte GetSceenDescriptorSize(ushort numberOfEntries)
        {
            return (byte)(Math.Ceiling(Math.Log(numberOfEntries, 2)) - 1);
        }

        /// <summary>
        /// Writes the byte representation of this <see cref="GifColorTable"/> to the <see cref="Stream"/>.
        /// </summary>
        internal void ToStream(Stream stream)
        {
            // 3 bytes per entry.
            var bytes = new byte[(int)Math.Pow(2, ScreenDescriptorSize + 1) * 3];

            foreach (var color in colors)
                color.ToStream(stream);
        }

        /// <summary>
        /// Represents an entry in the color table.
        /// </summary>
        public struct Color
        {
            /// <summary>
            /// How strong the blue component is.
            /// </summary>
            public readonly byte B;

            /// <summary>
            /// How strong the green component is.
            /// </summary>
            public readonly byte G;

            /// <summary>
            /// How strong the red component is.
            /// </summary>
            public readonly byte R;

            /// <summary>
            /// Creates a new instance of the <see cref="Color"/> struct with the given strengths for the components.
            /// </summary>
            /// <param name="r">How strong the red component is.</param>
            /// <param name="g">How strong the green component is.</param>
            /// <param name="b">How strong the blue component is.</param>
            public Color(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }

            /// <summary>
            /// Gets the <see cref="Color"/> from the byte representation starting from the current position in the <see cref="Stream"/>.
            /// </summary>
            /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="Color"/>.</param>
            /// <returns>A <see cref="Color"/> corresponding to the byte representation.</returns>
            internal static Color FromStream(Stream stream)
            {
                return new Color(
                    r: (byte)stream.ReadByte(),
                    g: (byte)stream.ReadByte(),
                    b: (byte)stream.ReadByte());
            }

            /// <summary>
            /// Writes the byte representation of this <see cref="Color"/> to the <see cref="Stream"/>.
            /// </summary>
            internal void ToStream(Stream stream)
            {
                stream.WriteByte(R);
                stream.WriteByte(G);
                stream.WriteByte(B);
            }
        }
    }
}