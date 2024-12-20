using System;
using FooProject.Collection;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace UnitTest
{
    [TestClass]
    public sealed class ListTest
    {
        [TestMethod]
        public void TryGetItemTest()
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
        public void CountTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            Assert.AreEqual(0, buf.Count);
            buf.AddRange("0123456789");
            Assert.AreEqual(10, buf.Count);
        }

        [TestMethod]
        public void TryGetTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            char result;
            buf.TryGet(0, out result);
            Assert.AreEqual('0', result);

            buf.TryGet(4, out result);
            Assert.AreEqual('4', result);

            buf.TryGet(5, out result);
            Assert.AreEqual('5', result);
        }

        [TestMethod]
        public void AddTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.Add('0');
            buf.Add('1');
            buf.Add('2');
            buf.Add('3');
            buf.Add('4');
            buf.Add('5');
            var output = String.Concat<char>(buf);
            Assert.AreEqual("012345", output);
        }

        [TestMethod]
        public void AddRangeTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            var output = String.Concat<char>(buf);
            Assert.AreEqual("0123456789", output);
        }

        [TestMethod]
        public void ContainsTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            var result = buf.Contains('0');
            Assert.AreEqual(true, result);
            result = buf.Contains('a');
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void CopyToTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            char[] result = new char[6];
            buf.AddRange("012345");
            buf.CopyTo(result, 0);
            Assert.AreEqual("012345", String.Concat<char>(result));
        }

        [TestMethod]
        public void IndexTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            var result = buf.IndexOf('0');
            Assert.AreEqual(0, result);
            result = buf.IndexOf('a');
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void InsertTest()
        {
            var buf = new FooProject.Collection.List<char>(5);
            buf.AddRange("0123456789");
            buf.Insert(4, 'a');
            var output = String.Concat<char>(buf);
            Assert.AreEqual("0123a456789", output);
        }

        [TestMethod]
        public void InserRangetTest()
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

        [TestMethod]
        public void RemoveAndInsertTest()
        {
            var buf = new FooProject.Collection.List<char>(13);
            buf.AddRange("this is ");
            buf.AddRange("a pen\n");
            buf.AddRange("this is ");
            buf.AddRange("a pen\n");
            buf.RemoveRange(10, 3);
            buf.InsertRange(10, "ratking");
            var output = String.Concat<char>(buf);
            Assert.AreEqual("this is a ratking\nthis is a pen\n", output);
            buf.RemoveRange(5, 4);
            buf.InsertRange(5, "is to be");
            output = String.Concat<char>(buf);
            Assert.AreEqual("this is to be ratking\nthis is a pen\n", output);
        }
    }
}
