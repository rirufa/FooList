﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace BPlusTree
{
    // public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable
    // {
    //     int Count { get; }
    // }
    //
    // public interface IReadOnlyList<out T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    // {
    //     T this[int index] { get; }
    // }


    internal sealed class ReversingReadOnlyList<T> : IEnumerable<T>
    {
        private readonly IList<T> source;

        public ReversingReadOnlyList(IList<T> source)
        {
            this.source = source;
        }

        public T this[int index]
        {
            get { return source[source.Count - 1 - index]; }
        }

        public int Count
        {
            get { return source.Count; }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// struct enumerator for <see cref="ReversingReadOnlyList{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private ReversingReadOnlyList<T> source;
            private int position; // points to next position after current.
            private T current;

            /// <summary>
            /// initializes a new instance of <see cref="Enumerator"/>.
            /// </summary>
            public Enumerator(ReversingReadOnlyList<T> source)
            {
                this.source = source;
                position = 0;
                current = default(T);
            }

            /// <inheritdoc />
            public T Current
            {
                get
                {
                    if (source == null)
                        throw new InvalidOperationException(
                            "enumerator has no array. it's either disposed or initialized with null array.");
                    if (position == 0)
                        throw new InvalidOperationException("enumerator cursor is not moved yet.");
                    if (position == source.Count + 1)
                        throw new InvalidOperationException("enumerator cursor has reached to the end.");
                    return current;
                }
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (source == null) return false;

                if (position < source.Count)
                {
                    current = source[position++];
                    return true;
                }
                else
                {
                    position = source.Count + 1; // end marker
                    current = default(T);
                    return false;
                }
            }

            /// <inheritdoc />
            public void Reset()
            {
                position = 0;
                current = default(T);
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                source = null;
                Reset();
            }
        }
    }
}