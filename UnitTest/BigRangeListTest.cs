﻿using FooProject.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UnitTest
{
    [TestClass]
    public class BigRangeListTest
    {
        private void AssertAreRangeEqual(int[] expected, BigRangeList<MyRange> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var n = list.GetIndexIntoRange(i);
                Assert.AreEqual(expected[i], n.Index);
            }
        }

        [TestMethod]
        public void AddTest()
        {
            BigRangeList<MyRange> list = new BigRangeList<MyRange>();
            for (int i = 0; i < 8; i++)
            {
                list.Add(new MyRange(0,3));
            }
            list.Insert(0, new MyRange(0, 3));
            list.Insert(list.Count, new MyRange(0, 3));

            var expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);
        }

        [TestMethod]
        public void AddRangeTest()
        {
            var list = new BigRangeList<MyRange>();
            var rangeList = new List<MyRange>();
            for (int i = 0; i < 8; i++)
            {
                rangeList.Add(new MyRange(0,3));
            }
            list.AddRange(rangeList);
            list.Insert(0, new MyRange(0,3));
            list.Insert(list.Count, new MyRange(0,3));

            var expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);
        }

        [TestMethod]
        public void InsertTest()
        {
            BigRangeList<MyRange> list = new BigRangeList<MyRange>();
            list.Add(new MyRange(0, 3));
            list.Add(new MyRange(0, 3));
            for (int i = 0; i < 8; i++)
            {
                list.Insert(1,new MyRange(0, 3));
            }
            var expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);

            list = new BigRangeList<MyRange>();
            list.Add(new MyRange(0, 3));
            list.Add(new MyRange(0, 3));
            for (int i = 0; i < 8; i++)
            {
                list.Insert(0, new MyRange(0, 3));
            }
            expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);

            list = new BigRangeList<MyRange>();
            list.Add(new MyRange(0, 3));
            list.Add(new MyRange(0, 3));
            for (int i = 0; i < 8; i++)
            {
                list.Insert(list.Count, new MyRange(0, 3));
            }
            expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);
        }

        [TestMethod]
        public void InsertRangeTest()
        {
            var rangeList = new List<MyRange>();
            for (int i = 0; i < 8; i++)
            {
                rangeList.Add(new MyRange(0, 3));
            }

            var list = new BigRangeList<MyRange>();
            list.Add(new MyRange(0, 3));
            list.Add(new MyRange(0, 3));
            list.InsertRange(1,rangeList);

            var expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);

            list = new BigRangeList<MyRange>();
            list.Add(new MyRange(0, 3));
            list.Add(new MyRange(0, 3));
            list.InsertRange(0, rangeList);

            expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);

            list = new BigRangeList<MyRange>();
            list.Add(new MyRange(0, 3));
            list.Add(new MyRange(0, 3));
            list.InsertRange(list.Count, rangeList);

            expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            AssertAreRangeEqual(expected, list);
        }

        [TestMethod]
        public void RemoveTest()
        {
            var rangeList = new List<MyRange>();
            for (int i = 0; i < 10; i++)
            {
                rangeList.Add(new MyRange(0, 3));
            }

            var list = new BigRangeList<MyRange>();
            list.AddRange(rangeList);
            list.RemoveAt(0);
            list.RemoveAt(0);
            list.RemoveAt(2);
            list.RemoveAt(3);
            list.RemoveAt(5);

            var expected = new int[] { 0, 3, 6, 9, 12, 15,   };
            AssertAreRangeEqual(expected, list);
        }

        [TestMethod]
        public void RemoveRangeTest()
        {
            var rangeList = new List<MyRange>();
            for (int i = 0; i < 10; i++)
            {
                rangeList.Add(new MyRange(0, 3));
            }

            var list = new BigRangeList<MyRange>();
            list.AddRange(rangeList);
            list.RemoveRange(0, 2);
            list.RemoveRange(2, 2);
            list.RemoveRange(5, 1);
            var expected = new int[] { 0, 3, 6, 9, 12, 15, };
            AssertAreRangeEqual(expected, list);
        }

        [TestMethod]
        public void GetIndexFromIndexIntoRangeTest()
        {
            var rangeList = new List<MyRange>();
            for (int i = 0; i < 10; i++)
            {
                rangeList.Add(new MyRange(0, 3));
            }

            var list = new BigRangeList<MyRange>();
            list.AddRange(rangeList);

            var expected = new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, };
            foreach(var item in expected)
            {
                var index = list.GetIndexFromIndexIntoRange(item);
                var range = list.GetIndexIntoRange(index);
                Assert.AreEqual(item, range.Index);
            }
        }
    }

    class MyRange : IRange
    {
        public int Index { get; set; }
        public int Length { get; set; }

        public MyRange(int index, int length)
        {
            Index = index; 
            Length = length;
        }
        public MyRange()
        {
        }
    }
}
