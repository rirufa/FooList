﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BPlusTree
{
    [DebuggerTypeProxy(typeof(BPTree<,>.DebugView))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class BPTree<TKey, TValue>
    {
        #region Debug View

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly BPTree<TKey, TValue> _tree;

            private DebugView(BPTree<TKey, TValue> tree)
            {
                if (null == tree)
                {
                    throw new ArgumentNullException("tree"); 
                }

                _tree = tree;
            }
            
            public Node Root
            {
                get { return _tree.Root; }
            }

            public LeafNode[] LinkList
            {
                get
                {
                    return GetEnumerable().ToArray();
                }
            }

            private IEnumerable<LeafNode> GetEnumerable()
            {
                LeafNode leaf = _tree.LinkList;
                while (leaf != null)
                {
                    yield return leaf;
                    leaf = leaf.Next;
                }
            }

            public LeafNode[] ReverseLinkList
            {
                get
                {
                    return GetEnumerable2().ToArray();
                }
            }

            private IEnumerable<LeafNode> GetEnumerable2()
            {
                LeafNode leaf = _tree.ReverseLinkList;
                while (leaf != null)
                {
                    yield return leaf;
                    leaf = leaf.Previous;
                }
            }
        }

        #endregion

        [DebuggerDisplay("{IsLeaf ? \"Leaf\" : \"Internal\",nq} Node, Length = {Length,nq}")]
        private abstract partial class Node
        {
        }

        #region Internal Node Debug View

        [DebuggerTypeProxy(typeof(BPTree<,>.InternalNode.DebugView))]
        private sealed partial class InternalNode
        {
            [DebuggerNonUserCode]
            private sealed class DebugView
            {
                private readonly InternalNode _node;

                private DebugView(InternalNode node)
                {
                    if (null == node)
                    {
                        throw new ArgumentNullException("node");   
                    }

                    _node = node;
                }
                
                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public KeyNodeItemView[] ChildNodes
                {
                    get
                    {
                        return GetEnumerable3().ToArray();
                    }
                }

                private IEnumerable<KeyNodeItemView> GetEnumerable3()
                {
                    var left = _node.Left;
                    foreach (var x in _node.Items)
                    {
                        yield return new KeyNodeItemView(x.Key, left, x.Right);
                        left = x.Right;
                    }
                }
            }

            [DebuggerNonUserCode]
            [DebuggerDisplay("Key = {Key}")]
            private struct KeyNodeItemView
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public TKey Key { get; private set; }
                public Node Left { get; private set;}
                public Node Right { get; private set;}

                public KeyNodeItemView(TKey key, Node left, Node right) : this()
                {
                    Key = key; //The 'this' object cannot be used before all of its fields are assigned to
                    Left = left;
                    Right = right;
                }
            }
        }

        #endregion

        #region Leaf Node Debug View

        [DebuggerTypeProxy(typeof(BPTree<,>.LeafNode.DebugView))]
        private sealed partial class LeafNode
        {
            [DebuggerNonUserCode]
            private sealed class DebugView
            {
                private readonly LeafNode _node;

                private DebugView(LeafNode node)
                {
                    if (null == node)
                    {
                        throw new ArgumentNullException("node");   
                    }

                    _node = node;
                }

                public RingArray<KeyValueItem> Items
                {
                    get { return _node.Items; }
                }

                public LeafNode Next
                {
                    get { return _node.Next; }
                }

                public LeafNode Previous
                {
                    get { return _node.Previous; }
                }
            }
        }

        #endregion
        
        [DebuggerDisplay("Key = {Key}")]
        private partial struct KeyNodeItem
        {
        }

        [DebuggerDisplay("Key = {Key}, Value = {Value}")]
        private partial struct KeyValueItem
        {
        }
    }
}
