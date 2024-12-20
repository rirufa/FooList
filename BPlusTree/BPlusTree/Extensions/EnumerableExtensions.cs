﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BPlusTree
{
    public static partial class EnumerableExtensions
    {
        //public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> source)
        //{
        //    return new ReadOnlyDictionary<TKey, TValue>(source);
        //}

        internal static IEnumerable<T> ToReadOnlyList<T>(this IList<T> source)
        {
            return new ReadOnlyCollection<T>(source);
        }

        internal static IList<T> ToReversingList<T>(this IList<T> source)
        {
            return new ReversingList<T>(source);
        }

        internal static IEnumerable<T> ToReversingReadOnlyList<T>(this IList<T> source)
        {
            return new ReversingReadOnlyList<T>(source);
        }

        public static BPTree<TKey, TValue> ToBPTree<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            return new BPTree<TKey, TValue>(source.Select(x => ValueTuple.Create(keySelector(x), x)), keyComparer);
        }

        public static SparseArray<TKey, TValue> ToSparseArray<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            return new SparseArray<TKey, TValue>(source.Select(x => ValueTuple.Create(keySelector(x), x)), keyComparer);
        }
    }
}
