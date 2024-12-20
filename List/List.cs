﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BPlusTree;

namespace FooProject.Collection
{
    public class RangeKey : IComparer<RangeKey>, IComparable<RangeKey>, IEquatable<RangeKey>
    {
        int _start;
        public int start
        {
            get { return _start; }
            set { _start = value; this.end = this.start + this.length - 1; }
        }
        int _length;
        public int length
        {
            get => _length;
            set
            {
                _length = value;
                if (_length > 0)
                    this.end = this.start + this.length - 1;
                else
                    this.end = this.start;
            }
        }

        public int end
        {
            get;
            private set;
        }
        public RangeKey()
        {
        }

        public RangeKey(int start, int length)
        {
            this.start = start;
            this.length = length;
        }

        public int Compare(RangeKey? x, RangeKey? y)
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
                string.Format("未知のパターン x({0}) y({1})",x.ToString(),y.ToString())
                );
        }

        public new string ToString()
        {
            return String.Format("start:{0} end:{1}", this.start, this.end);
        }

        public int CompareTo(RangeKey? other)
        {
            return this.Compare(this, other);
        }

        public bool Equals(RangeKey? other)
        {
            if (other == null)
                throw new ArgumentNullException("other must not be null");
            return this.start == other.start && this.length == other.length;
        }
    }

    public class RangeItem<T>
    {
        public System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>(List<T>.MaxCapacity);
    }

    public class List<T> : IList<T>
    {
        BPTree<RangeKey, RangeItem<T>> collection;

        // LOH入りになってしまうとガベージコレクションの時に回収されなくなるので、LOH入りを回避できる値にする
        // 今のところは、x64(Intが8バイト)で動かすことが多く、このくらいの値であればおおむね回避できると思われる
        // https://learn.microsoft.com/ja-jp/dotnet/standard/garbage-collection/large-object-heap
        internal static int MaxCapacity = 10240;

        public List()
        {
            this.collection = new BPTree<RangeKey, RangeItem<T>>(null, MaxCapacity, MaxCapacity);
        }

        public List(int eachItemCapacity) : this()
        {
            MaxCapacity = eachItemCapacity;
            this.collection = new BPTree<RangeKey, RangeItem<T>>(null, eachItemCapacity, eachItemCapacity);
        }

        public T this[int index] {
            get {
                T outItem;
                var result = this.TryGet(index, out outItem);
                if (result == false)
                    throw new ArgumentOutOfRangeException();
                return outItem;
            }
            set {
                RangeKey range;
                RangeItem<T> item;
                this.TryGetItem(index, out range, out item);
                item.list[index - range.start] = value;
            }
        }

        const int DirtyFlag = -1;
        int _Count = -1;
        public int Count
        {
            get
            {
                if (_Count == DirtyFlag)
                {
                    _Count = 0;
                    if(this.collection.Count != 0)
                    {
                        var lastKey = this.collection.Last.Item1;
                        _Count += lastKey.start + lastKey.length;
                    }
                }
                return _Count;
            }
        }

        public bool IsReadOnly => false;

        public bool TryGetItem(int index,out RangeKey outRange,out  RangeItem<T> outItem)
        {
            outRange = null;
            outItem = null;
            var result = this.collection.TryGet(new RangeKey() { start = index, length = 1 },out outRange, out outItem);
            if (result == false)
            {
                return false;                
            }
            return true;
        }

        public bool TryGet(int index,out T outItem)
        {
            RangeKey range;
            RangeItem<T> item;
            outItem = default(T);
            var result = this.TryGetItem(index, out range, out item);
            if (result)
            {
                int relativeIndex = index - range.start;
                outItem = item.list[relativeIndex];
                return true;
            }
            return false;
        }

        void SetDirtyFlag()
        {
            _Count = DirtyFlag;
        }

        public void Add(T item)
        {
            this.AddRange(new T[] { item });
        }

        public void AddRange(IEnumerable<T> items)
        {
            if(this.collection.Count == 0) //ノードが存在するかどうか
            {
                this.InsertRange(0, items);
            }
            else
            {
                var (lastkey,lastvalue) = this.collection.Last;
                this.InsertRange(lastkey.start + lastkey.length, items);
            }
        }

        public void Clear()
        {
            this.collection.Clear();
            _Count = 0;
        }

        public bool Contains(T item)
        {
            IEnumerable<(RangeKey, RangeItem<T>)> list = this.collection.AsPairEnumerable(new RangeKey() { start = 0, length = 1 });
            foreach (var kv in list)
            {
                if(kv.Item2.list.Contains(item))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int targetArrayIndex = arrayIndex;
            IEnumerable<(RangeKey, RangeItem<T>)> list = this.collection.AsPairEnumerable(new RangeKey() { start = 0, length = 1 });
            foreach (var kv in list)
            {
                foreach (var item in kv.Item2.list)
                {
                    array[targetArrayIndex++] = item;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable<(RangeKey, RangeItem<T>)> list = this.collection.AsPairEnumerable(new RangeKey() { start = 0, length = 1 });
            foreach (var kv in list)
            {
                foreach (var item in kv.Item2.list)
                {
                    yield return item;
                }
            }
        }

        public int IndexOf(T item)
        {
            IEnumerable<(RangeKey, RangeItem<T>)> list = this.collection.AsPairEnumerable(new RangeKey() { start = 0, length = 1 });
            foreach (var kv in list)
            {
                var key = kv.Item1;
                var value = kv.Item2;
                int relativeIndex = value.list.IndexOf(item);
                if (relativeIndex != -1)
                {
                    return relativeIndex + key.start;
                }
            }
            return -1;
        }

        private void Validate()
        {
#if DEBUG
            int nextIndex = 0;
            foreach(var item in this.collection.AsPairEnumerable())
            {
                var key = item.Item1;
                int headIndex = key.start;
                if (headIndex != nextIndex)
                {
                    throw new InvalidDataException("Invaild Range");
                }
                nextIndex = headIndex + key.length;
            }
#endif
        }

        public void Insert(int index, T item)
        {
            this.InsertRange(index, new T[1] { item} );
        }

        public void InsertRange(int index,IEnumerable<T> items)
        {
            int insertIndex = index;
            int insertCount = 0;
            RangeKey range;
            RangeItem<T> target;
            System.Collections.Generic.List<T> overflowItems = new System.Collections.Generic.List<T>();
            int itemsCount = items.Count();

            this.UpdateIndex(insertIndex, itemsCount, false);

            var result = this.TryGetItem(index, out range, out target);
            if(result == false)
            {
                result = this.TryGetItem(index -1, out range, out target );
                if (result == false && this.collection.Count >0)
                    throw new KeyNotFoundException("近くのキーがありません");
            }
            if(result == true)
            {
                int relativeIndex = index - range.start;
                if(target.list.Count+ itemsCount < List<T>.MaxCapacity)
                {
                    target.list.InsertRange(relativeIndex, items);
                    range.length += itemsCount;
                    insertIndex = index + itemsCount;
                    insertCount = itemsCount;
                }
                else
                {
                    int relativeCount = target.list.Count - relativeIndex;
                    overflowItems.AddRange(target.list.GetRange(relativeIndex,relativeCount));
                    target.list.RemoveRange(relativeIndex, relativeCount);
                    range.length -= overflowItems.Count;
                    if (target.list.Count + itemsCount < List<T>.MaxCapacity)
                    {
                        target.list.AddRange(items);
                        range.length += itemsCount;
                        insertIndex = index + itemsCount;
                        insertCount = itemsCount;
                    }
                    if (target.list.Count == 0)
                    {
                        RangeItem<T> deletedItems;
                        this.collection.Remove(range, out deletedItems);
                    }
                }
            }

            foreach (var sliced_items in items.Concat(overflowItems).Skip(insertCount).Chunk(List<T>.MaxCapacity))
            {
                var newItemCount = sliced_items.Length;
                var newKey = new RangeKey(insertIndex, newItemCount);
                var newitem = new RangeItem<T>() { list = new System.Collections.Generic.List<T>(sliced_items) };
                this.collection.Add(newKey, newitem);
                insertIndex += newItemCount;
                insertCount += newItemCount;
            }

            this.Validate();
        }

        public bool Remove(T item)
        {
            IEnumerable<(RangeKey, RangeItem<T>)> list = this.collection.AsPairEnumerable(new RangeKey() { start = 0, length = 1 });
            foreach (var kv in list)
            {
                var key = kv.Item1;
                var value = kv.Item2;
                int relativeIndex = value.list.IndexOf(item);
                if (relativeIndex != -1)
                {
                    value.list.RemoveAt(relativeIndex);
                    return true;
                }
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            this.RemoveRange(index, 1);
        }

        public void RemoveRange(int index,int count)
        {
            if (count < 1)
                return;

            int removeIndex = index;
            int removeEnd = removeIndex + count - 1;
            int previousNodeCountDelta = 0;
            while(true)
            {
                if (removeIndex > removeEnd)
                    break;

                RangeKey range;
                RangeItem<T> target;
                var result = this.TryGetItem(removeIndex, out range, out target);
                if (result == false)
                {
                    throw new InvalidOperationException();
                }

                int relativeIndex;
                int relativeCount;
                if(removeIndex >= range.start && removeEnd < range.end)
                {
                    relativeIndex = removeIndex - range.start;
                    relativeCount = removeEnd - removeIndex + 1;
                    target.list.RemoveRange(relativeIndex, relativeCount);
                    range.length -= relativeCount;
                }else if(removeEnd >= range.end){
                    relativeIndex = removeIndex - range.start;
                    relativeCount = target.list.Count - relativeIndex;
                    target.list.RemoveRange(relativeIndex, relativeCount);
                    range.length -= relativeCount;
                }
                else
                {
                    relativeIndex = range.start;
                    relativeCount = count - relativeIndex;
                    target.list.RemoveRange(relativeIndex, relativeCount);
                    range.length -= relativeCount;
                }
                if (previousNodeCountDelta > 0)
                    range.start -= previousNodeCountDelta;

                if (target.list.Count == 0)
                {
                    RangeItem<T> removedItem;
                    this.collection.Remove(range, out removedItem);
                }

                removeIndex += relativeCount;
                previousNodeCountDelta += relativeCount;
            }

            //残りのインデックスを調整する
            UpdateIndex(removeIndex, -count, false);

            this.Validate();
        }

        void UpdateIndex(int startIndex,int updateCount,bool includeStartNode = true)
        {

            IEnumerable<(RangeKey, RangeItem<T>)> list = this.collection.AsPairEnumerable(new RangeKey() { start = startIndex, length = 1 });
            if (includeStartNode)
            {
                foreach (var item in list)
                {
                    var key = item.Item1;
                    key.start += updateCount;
                }
            }
            else
            {
                foreach (var item in list)
                {
                    var key = item.Item1;
                    if (key.start > startIndex)
                        key.start += updateCount;
                }
            }
            SetDirtyFlag();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach(var item in  this)
                yield return item;
        }
    }
}
