using System;
using FooProject.Collection;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace UnitTest
{
    [TestClass]
    public sealed class ListTest
    {
        [TestMethod]
        public void GetTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");

            RangeKey key;
            RangeItem<char> item;

            buf.TryGetItem(0, out key, out item);
            Assert.AreEqual(0, key.start);
            Assert.AreEqual("01234", string.Concat(item.list));

            buf.TryGetItem(2, out key, out item);
            Assert.AreEqual(0, key.start);
            Assert.AreEqual("01234", string.Concat(item.list));

            buf.TryGetItem(4, out key, out item);
            Assert.AreEqual(0, key.start);
            Assert.AreEqual("01234", string.Concat(item.list));

            buf.TryGetItem(5, out key, out item);
            Assert.AreEqual(5, key.start);
            Assert.AreEqual("56789", string.Concat(item.list));

            buf.TryGetItem(7, out key, out item);
            Assert.AreEqual(5, key.start);
            Assert.AreEqual("56789", string.Concat(item.list));

            buf.TryGetItem(9, out key, out item);
            Assert.AreEqual(5, key.start);
            Assert.AreEqual("56789", string.Concat(item.list));
        }

        [TestMethod]
        public void AddTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            var output = String.Concat<char>(buf);
            Assert.AreEqual("0123456789", output);
        }

        [TestMethod]
        public void InsertTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            buf.InsertRange(4, "abcdef");
            var output = String.Concat<char>(buf);
            Assert.AreEqual("0123abcdef456789", output);
        }

        [TestMethod]
        public void RemoveTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            buf.InsertRange(5, "abcdef");
            buf.RemoveRange(5, 7);
            var output = String.Concat<char>(buf);
            Assert.AreEqual("012346789", output);
        }
    }
}
