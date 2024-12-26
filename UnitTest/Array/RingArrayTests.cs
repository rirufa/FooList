using Microsoft.VisualStudio.TestTools.UnitTesting;
using BPlusTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BPlusTree.Tests
{
    [TestClass()]
    public class RingArrayTests
    {
        [TestMethod()]
        public void InsertTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Insert(1, 3);
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 2 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void RemoveAtTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.RemoveAt(0);
            arr.RemoveAt(1);
            CollectionAssert.AreEquivalent(new int[] { 2 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void RemoveTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Remove(1);
            CollectionAssert.AreEquivalent(new int[] { 2, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void AddTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void IndexOfTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            var r = arr.IndexOf(1);
            Assert.AreEqual(0, r);
            r = arr.IndexOf(4);
            Assert.AreEqual(-1, r);
        }

        [TestMethod()]
        public void ContainsTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            Assert.IsTrue(arr.Contains(1));
            Assert.IsFalse(arr.Contains(4));
        }

        [TestMethod()]
        public void PushFirstTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(2);
            arr.Add(3);
            arr.PushFirst(1);
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void PushLastTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.PushLast(3);
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void PopFirstTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            var r = arr.PopFirst();
            Assert.AreEqual(1, r);
            CollectionAssert.AreEquivalent(new int[] { 2, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void PopLastTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            var r = arr.PopLast();
            Assert.AreEqual(3, r);
            CollectionAssert.AreEquivalent(new int[] { 1, 2 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void InsertPopFirstTest()
        {
            var arr = RingArray<int>.NewFixedCapacityArray(4);
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Add(4);
            var r = arr.InsertPopFirst(1,5);
            Assert.AreEqual(1, r);
            CollectionAssert.AreEquivalent(new int[] { 5, 2, 3, 4 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void InsertPopLastTest()
        {
            var arr = RingArray<int>.NewFixedCapacityArray(4);
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Add(4);
            var r = arr.InsertPopLast(1, 5);
            Assert.AreEqual(4, r);
            CollectionAssert.AreEquivalent(new int[] { 1, 5, 2, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void BinarySearchTest()
        {
            var arr = RingArray<int>.NewFixedCapacityArray(4);
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Add(4);
            var r = arr.BinarySearch(2);
            Assert.AreEqual(1, r);
            r = arr.BinarySearch(10);
            Assert.AreEqual(-5, r);
        }

        [TestMethod()]
        public void InsertOrderedTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Add(5);
            arr.InsertOrdered(4);
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3, 4, 5 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void RemoveOrderedTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.RemoveOrdered(2);
            CollectionAssert.AreEquivalent(new int[] { 1, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void ReplaceTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Replace(1,5);
            CollectionAssert.AreEquivalent(new int[] { 1, 5, 3 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void ClearTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Clear();
            Assert.AreEqual(0,arr.Count);
        }

        [TestMethod()]
        public void CopyToTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            int[] newArray = new int[3];
            arr.CopyTo(newArray, 0);
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3 }.ToList(), newArray.ToList());
        }

        [TestMethod()]
        public void SplitRightTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Add(4);
            var newArr = arr.SplitRight();
            CollectionAssert.AreEquivalent(new int[] { 3, 4 }.ToList(), newArr.ToList());
            CollectionAssert.AreEquivalent(new int[] { 1, 2 }.ToList(), arr.ToList());
        }

        [TestMethod()]
        public void MergeLeftTest()
        {
            var arr = RingArray<int>.NewArray();
            arr.Add(1);
            arr.Add(2);
            arr.Add(3);
            arr.Add(4);
            var newArr = arr.SplitRight();
            arr.MergeLeft(newArr);
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3, 4 }.ToList(), arr.ToList());
        }
    }
}