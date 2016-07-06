﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace SharpGif.Tests
{
    [TestClass]
    public class Decoding
    {
        private static byte[] file = new byte[]
        {
            0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x0A, 0x00, 0x0A, 0x00, 0x91, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x0A, 0x00, 0x00, 0x02, 0x16, 0x8C, 0x2D, 0x99, 0x87, 0x2A, 0x1C, 0xDC, 0x33, 0xA0, 0x02, 0x75, 0xEC, 0x95, 0xFA, 0xA8, 0xDE, 0x60, 0x8C, 0x04, 0x91, 0x4C, 0x01, 0x00, 0x3B
        };

        [TestMethod]
        public void DecodesFile()
        {
            var ms = new MemoryStream(file);
            var gif = new Gif(ms);
        }

        [TestMethod]
        public void EncodesFile()
        {
            var ms = new MemoryStream(file);
            var gif = new Gif(ms);

            var ms2 = new MemoryStream();
            gif.ToStream(ms2);

            var bytes = ms2.ToArray();
            Assert.AreEqual(file.Length, bytes.Length);

            for (var i = 0; i < ms.Length; ++i)
                Assert.AreEqual(file[i], bytes[i]);

            ms2.Position = 0;
            var gif2 = new Gif(ms2);
        }
    }
}