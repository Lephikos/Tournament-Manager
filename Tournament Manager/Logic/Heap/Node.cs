using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Heap
{

    /// <summary>
    /// Node for <see cref="PairingHeap{K, V}"/>
    /// </summary>
    /// <typeparam name="K">type of keys</typeparam>
    /// <typeparam name="V">type of values</typeparam>
    internal class Node<K, V> : IHandle<K, V>
    {

        internal PairingHeap<K, V> heap;

        internal K key;
        internal V? value;
        internal Node<K, V>? oldestChild;
        internal Node<K, V>? youngerSibling;
        internal Node<K, V>? olderSiblingOrParent;

        /// <summary>
        /// Constructor for new node
        /// </summary>
        /// <param name="heap"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal Node(PairingHeap<K, V> heap, K key, V? value)
        {
            this.heap = heap;
            this.key = key;
            this.value = value;
            this.oldestChild = null;
            this.youngerSibling = null;
            this.olderSiblingOrParent = null;
        }


        public K GetKey()
        {
            return key;
        }

        public V? GetValue()
        {
            return value;
        }

        public void SetValue(V value)
        {
            this.value = value;
        }

        public void DecreaseKey(K newKey)
        {
            GetOwner().DecreaseKey(this, newKey);
        }

        public void Delete()
        {
            GetOwner().Delete(this);
        }

        /// <summary>
        /// Get the owner heap of the handle. This is union-find with
        /// path-compression between heaps.
        /// </summary>
        /// <returns></returns>
        internal PairingHeap<K, V> GetOwner()
        {
            if (heap.other != heap)
            {
                //find root
                PairingHeap<K, V> root = heap;
                while (root != root.other)
                {
                    root = root.other;
                }

                // path - compression
                PairingHeap<K, V> cur = heap;
                while (cur.other != root)
                {
                    PairingHeap<K, V> next = cur.other;
                    cur.other = root;
                    cur = next;
                }
                heap = root;
            }
            return heap;
        }

    }
}
