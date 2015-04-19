using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGif
{
    /// <summary>
    /// Represents a data block that is divided into sub-blocks in the actual gif image/file.
    /// </summary>
    public sealed class GifDataStream : IList<byte>
    {
        private const byte maxSubBlockLength = byte.MaxValue;

        private readonly List<byte> data = new List<byte>();

        /// <summary>
        /// Gets the <see cref="GifDataStream"/> from the byte representation starting from the current position in the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the byte representation of a <see cref="GifDataStream"/>.</param>
        /// <returns>A <see cref="GifDataStream"/> corresponding to the byte representation.</returns>
        internal static GifDataStream FromStream(Stream stream)
        {
            var result = new GifDataStream();

            var nextBlockLength = stream.ReadByte();
            while (nextBlockLength > 0)
            {
                var subBlock = new byte[nextBlockLength];
                stream.Read(subBlock, 0, nextBlockLength);

                result.data.AddRange(subBlock);

                nextBlockLength = stream.ReadByte();
            }

            return result;
        }

        /// <summary>
        /// Writes the byte representation of this <see cref="GifDataStream"/> to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        internal void ToStream(Stream stream)
        {
            var fullSubBlocks = (int)Math.Ceiling(data.Count / (float)maxSubBlockLength);
            var trailingLength = (byte)(data.Count % maxSubBlockLength);

            var dataBytes = data.ToArray();
            for (var i = 0; i < fullSubBlocks; i += maxSubBlockLength)
            {
                stream.WriteByte(maxSubBlockLength);
                stream.Write(dataBytes, i, maxSubBlockLength);
            }

            if (trailingLength > 0)
            {
                stream.WriteByte(trailingLength);
                stream.Write(dataBytes, fullSubBlocks * maxSubBlockLength, trailingLength);
            }

            stream.WriteByte(0);
        }

        #region IList

        public int Count
        {
            get { return data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public byte this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public void Add(byte item)
        {
            data.Add(item);
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(byte item)
        {
            return data.Contains(item);
        }

        public void CopyTo(byte[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public int IndexOf(byte item)
        {
            return data.IndexOf(item);
        }

        public void Insert(int index, byte item)
        {
            data.Insert(index, item);
        }

        public bool Remove(byte item)
        {
            return data.Remove(item);
        }

        public void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IList
    }
}