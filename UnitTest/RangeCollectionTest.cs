/*
 * Copyright (C) 2013 FooProject
 * * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FooEditEngine;
using BPlusTree;
using FooProject.Collection;

namespace UnitTest
{
    [TestClass]
    public class RangeCollectionTest
    {
        class MyRangeItem : IRange, IComparer<MyRangeItem>, IComparable<MyRangeItem>, IEquatable<MyRangeItem>
        {
            public MyRangeItem()
            {

            }

            public MyRangeItem(int start,int length)
            {
                this.start = start;
                this.length = length;
            }

            public int start
            {
                get;
                set;
            }

            public int length
            {
                get;
                set;
            }
            public int end
            {
                get
                {
                    if (this.length > 0)
                        return this.length + this.start - 1;
                    else
                        return 0;
                }
            }
            public int Compare(MyRangeItem? x, MyRangeItem? y)
            {
                if (x == null && y == null)
                    throw new ArgumentNullException("x or y");

                if (x.start <= y.start && y.start <= x.end)
                {
                    // xxxxx
                    //     yy
                    if (x.end >= y.end)
                        return 0;
                    // xxxxx
                    //     yyyyy
                    if (x.end < y.end)
                        return 0;
                }
                // xxxxx
                //               yyyy
                if (x.end < y.start)
                    return -1;
                //           xxxx
                //  yyyy
                if (x.start > y.end)
                    return 1;
                if (x.start > y.start)
                {
                    //      xx
                    //   yyyy
                    if (x.end >= y.start && x.end <= y.end)
                        return 0;
                    //    xxxxx
                    // yyyy
                    if (x.end > y.end)
                        return 0;
                }
                // xxxx
                // yyyy
                if (x.start == y.start && x.end == y.end)
                    return 0;
                throw new InvalidDataException(
                    string.Format("未知のパターン x({0}) y({1})", x.ToString(), y.ToString())
                    );
            }

            public new string ToString()
            {
                return String.Format("start:{0} end:{1}", this.start, this.end);
            }

            public int CompareTo(MyRangeItem? other)
            {
                return this.Compare(this, other);
            }

            public bool Equals(MyRangeItem? other)
            {
                if (other == null)
                    throw new ArgumentNullException("other must not be null");
                return this.start == other.start && this.length == other.length;
            }

            public override bool Equals(object? obj)
            {
                MyRangeItem? other = obj as MyRangeItem;
                if (other == null)
                    return false;
                return this.Equals(other);
            }
        }

        [TestMethod]
        public void InsertOrderedTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.InsertOrdered(new MyRangeItem(10, 10));
            collection.InsertOrdered(new MyRangeItem(1, 10));
            Assert.IsTrue(collection[0].start == 1 && collection[1].start == 10);
        }

        [TestMethod]
        public void ReplaceTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.InsertOrdered(new MyRangeItem(10, 10));
            collection.InsertOrdered(new MyRangeItem(0, 10));
            var new_collection = new System.Collections.Generic.List<MyRangeItem>();
            new_collection.Add(new MyRangeItem(10, 10));
            new_collection.Add(new MyRangeItem(20, 10));
            collection.ReplaceRange(1, new_collection, 0, 20);
            Assert.IsTrue(
                collection.GetLineHeadIndex(0) == 0 &&
                collection.GetLineHeadIndex(1) == 10 &&
                collection.GetLineHeadIndex(2) == 20 &&
                collection.GetLineHeadIndex(3) == 30
                );
            new_collection = new System.Collections.Generic.List<MyRangeItem>();
            new_collection.Add(new MyRangeItem(10, 5));
            new_collection.Add(new MyRangeItem(15, 5));
            collection.ReplaceRange(1, new_collection, 2, -10);
            Assert.IsTrue(
                collection.GetLineHeadIndex(0) == 0 &&
                collection.GetLineHeadIndex(1) == 10 &&
                collection.GetLineHeadIndex(2) == 15 &&
                collection.GetLineHeadIndex(3) == 20
                );
        }

        [TestMethod]
        public void QueryRangeItemTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            var result = collection.Get(1).ToList();
            Assert.IsTrue(result[0].start == 1 && result[0].length == 10);

            result = collection.Get(0, 20).ToList();
            Assert.IsTrue(result[0].start == 1 && result[0].length == 10);

            collection.Add(new MyRangeItem(15, 10));
            result = collection.Get(0, 20).ToList();
            Assert.IsTrue(result[0].start == 1 && result[0].length == 10);
            Assert.IsTrue(result[1].start == 15 && result[0].length == 10);
        }

        [TestMethod]
        public void RemoveRangeItemTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            collection.Add(new MyRangeItem(20, 10));

            collection.Remove(0, 15);
            
            var result = collection.ToList();
            Assert.IsTrue(result[0].start == 20 && result[0].length == 10);

            collection.Remove(20,1);
            Assert.IsTrue(collection.Count == 0);
        }
        [TestMethod()]
        public void InsertTest()
        {
            LazyRangeCollection<MyRangeItem> arr = new LazyRangeCollection<MyRangeItem>();
            arr.Add(new MyRangeItem(1, 10));
            arr.Add(new MyRangeItem(20, 10));
            arr.Insert(1, new MyRangeItem(10,10));

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(1, 10));
            expected_list.Add(new MyRangeItem(10, 10));
            expected_list.Add(new MyRangeItem(20, 10));

            CollectionAssert.AreEqual(expected_list, arr.ToList());
        }

        [TestMethod()]
        public void RemoveAtTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.RemoveAt(0);
            collection.RemoveAt(1);
            Assert.IsTrue(collection[0].start == 20 &&  collection[0].length == 10);
        }

        [TestMethod()]
        public void RemoveTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.Remove(new MyRangeItem(1, 10));
            Assert.IsTrue(collection[0].start == 20 && collection[0].length == 10);
            Assert.IsTrue(collection[1].start == 30 && collection[1].length == 10);
        }

        [TestMethod()]
        public void IndexOfTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            var r = collection.IndexOf(new MyRangeItem(1, 10));
            Assert.AreEqual(0, r);
            r = collection.IndexOf(new MyRangeItem(40, 10));
            Assert.AreEqual(-3, r);
        }

        [TestMethod()]
        public void ContainsTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            var r = collection.Contains(new MyRangeItem(1, 10));
            Assert.AreEqual(true, r);
            r = collection.Contains(new MyRangeItem(40, 1));
            Assert.AreEqual(false, r);
        }

        [TestMethod()]
        public void PushFirstTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(1, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.PushFirst(new MyRangeItem(0, 1));

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(0, 1));
            expected_list.Add(new MyRangeItem(1, 10));
            expected_list.Add(new MyRangeItem(20, 10));
            expected_list.Add(new MyRangeItem(30, 10));

            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }

        [TestMethod()]
        public void PushLastTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.PushLast(new MyRangeItem(40, 10));

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(0, 10));
            expected_list.Add(new MyRangeItem(20, 10));
            expected_list.Add(new MyRangeItem(30, 10));
            expected_list.Add(new MyRangeItem(40, 10));

            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }

        [TestMethod()]
        public void PopFirstTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            var r = collection.PopFirst();

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(20, 10));
            expected_list.Add(new MyRangeItem(30, 10));

            Assert.AreEqual(0, r.start);
            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }

        [TestMethod()]
        public void PopLastTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            var r = collection.PopLast();

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(0, 10));
            expected_list.Add(new MyRangeItem(20, 10));

            Assert.AreEqual(30, r.start);
            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }

        [TestMethod()]
        public void InsertPopFirstTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            var r = collection.InsertPopFirst(0, new MyRangeItem(10,10));

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(10, 10));
            expected_list.Add(new MyRangeItem(20, 10));
            expected_list.Add(new MyRangeItem(30, 10));
            Assert.AreEqual(0, r.start);
            CollectionAssert.AreEqual(expected_list,collection.ToList());
        }

        [TestMethod()]
        public void InsertPopLastTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            var r = collection.InsertPopLast(1, new MyRangeItem(10, 10));

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(0, 10));
            expected_list.Add(new MyRangeItem(10, 10));
            expected_list.Add(new MyRangeItem(20, 10));
            Assert.AreEqual(30, r.start);
            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }

        [TestMethod()]
        public void RemoveOrderedTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.RemoveOrdered(new MyRangeItem(0, 10));

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(20, 10));
            expected_list.Add(new MyRangeItem(30, 10));

            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }

        [TestMethod()]
        public void ClearTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.Clear();
            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod()]
        public void CopyToTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            MyRangeItem[] newArray = new MyRangeItem[3];
            collection.CopyTo(newArray, 0);

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(0, 10));
            expected_list.Add(new MyRangeItem(20, 10));
            expected_list.Add(new MyRangeItem(30, 10));
            CollectionAssert.AreEqual(expected_list, newArray.ToList());
        }

        [TestMethod()]
        public void SplitRightTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.Add(new MyRangeItem(40, 10));

           var newCollection = collection.SplitRight();
            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(30, 10));
            expected_list.Add(new MyRangeItem(40, 10));
            CollectionAssert.AreEqual(expected_list, newCollection.ToList());

            expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(0, 10));
            expected_list.Add(new MyRangeItem(20, 10));
            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }

        [TestMethod()]
        public void MergeLeftTest()
        {
            LazyRangeCollection<MyRangeItem> collection = new LazyRangeCollection<MyRangeItem>();
            collection.Add(new MyRangeItem(0, 10));
            collection.Add(new MyRangeItem(20, 10));
            collection.Add(new MyRangeItem(30, 10));
            collection.Add(new MyRangeItem(40, 10));

            var newCollection = collection.SplitRight();

            collection.MergeLeft(newCollection);

            var expected_list = new System.Collections.Generic.List<MyRangeItem>();
            expected_list.Add(new MyRangeItem(0, 10));
            expected_list.Add(new MyRangeItem(20, 10));
            expected_list.Add(new MyRangeItem(30, 10));
            expected_list.Add(new MyRangeItem(40, 10));
            CollectionAssert.AreEqual(expected_list, collection.ToList());
        }
    }
}
