using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Heap
{

    /// <summary>
    /// Pairing heaps. The heap is sorted according to the Comparable
    /// natural ordering of its keys, or by a <see cref="Comparer{T}"/> provided at heap
    /// creation time, depending on which constructor is used.<para/>
    /// 
    /// This implementation provides amortized O(log(n)) time cost for the
    /// <c>Insert</c>, <c>DeleteMin</c>, and <c>DecreaseKey</c> operations.
    /// Operation <c>FindMin</c>, is a worst-case O(1) operation. The algorithms are
    /// based on the <a href="http://dx.doi.org/10.1007/BF01840439">pairing heap
    /// paper</a>. Pairing heaps are very efficient in practice, especially in
    /// applications requiring the <c>DecreaseKey</c> operation. The operation
    /// <c>meld</c> is amortized O(log(n)).<para/>
    /// 
    /// All the above bounds, however, assume that the user does not perform
    /// cascading melds on heaps such as:<para/>
    /// 
    /// d.meld(e) -> c.meld(d) -> b.meld(c) -> a.meld(b)<para/>
    /// 
    /// The above scenario, although efficiently supported by using union-find with
    /// path compression, invalidates the claimed bounds.<para/>
    /// 
    /// Note that the ordering maintained by a pairing heap, like any heap, and
    /// whether or not an explicit comparator is provided, must be <em>consistent
    /// with <c>equals</c></em> if this heap is to correctly implement the
    /// <c>AdressableHeap</c> interface. This is so because the  <c>AdressableHeap</c> interface is
    /// defined in terms of the <c>equals</c> operation, but a pairing heap performs
    /// all key comparisons using its <c>Compare</c> method,
    /// so two keys that are deemed equal by this method are, from the standpoint of
    /// this heap, equal. The behavior of a heap <em>is</em> well-defined even if its
    /// ordering is inconsistent with <c>equals</c>; it just fails to obey the
    /// general contract of the <c>AdressableHeap</c> interface.<para/>
    /// 
    /// <strong>Note that this implementation is not synchronized.</strong> If
    /// multiple threads access a heap concurrently, and at least one of the threads
    /// modifies the heap structurally, it <em>must</em> be synchronized externally.
    /// (A structural modification is any operation that adds or deletes one or more
    /// elements or changing the key of some element.) This is typically accomplished
    /// by synchronizing on some object that naturally encapsulates the heap.
    /// </summary>
    /// <typeparam name="K">the type of keys maintained by this heap</typeparam>
    /// <typeparam name="V">the type of values maintained by this heap</typeparam>
    internal class PairingHeap<K, V> : IMergeableAddressableHeap<K, V>
    {

        #region member

        /// <summary>
        /// The comparator used to maintain order in this heap, or null if it uses
        /// the natural ordering of its keys.
        /// </summary>
        private readonly Comparer<K>? comparer;

        /// <summary>
        /// The root of the pairing heap
        /// </summary>
        private Node<K, V>? root;

        /// <summary>
        /// Size of the pairing heap
        /// </summary>
        private long count;

        /// <summary>
        /// Used to reference the current heap or some other pairing heap in case of
        /// elding, so that handles remain valid even after a meld, without having
        /// to iterate over them.
        /// 
        /// In order to avoid maintaining a full-fledged union-find data structure,
        /// we disallow a heap to be used in melding more than once. We use however,
        /// path-compression in case of cascading melds, that is, a handle moves from
        /// one heap to another and then another.
        /// </summary>
        internal PairingHeap<K, V> other;

        #endregion member

        #region constructor

        public PairingHeap() : this(null) { }

        public PairingHeap(Comparer<K>? comparer)
        {
            this.root = null;
            this.comparer = comparer;
            this.count = 0;
            this.other = this;
        }

        #endregion constructor

        #region public methods

        public IHandle<K, V> Insert(K key, V? value)
        {
            if (other != this)
            {
                throw new InvalidOperationException("A heap cannot be used after a meld");
            }

            if (key == null)
            {
                throw new NullReferenceException("Null keys not permitted");
            }

            Node<K, V> n = new (this, key, value);
            if (comparer == null)
            {
                root = Link(root, n);
            } else
            {
                root = LinkWithComparer(root, n);
            }

            count++;
            return n;
        }

        public IHandle<K, V> Insert(K key)
        {
            return Insert(key, default);
        }

        public IHandle<K, V> FindMin()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("No such element");
            }

            return root!;
        }

        public IHandle<K, V> DeleteMin()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("No such element");
            }

            IHandle<K, V> oldRoot = root!;

            root = Combine(CutChildren(root!));

            count--;
            return oldRoot;
        }

        public bool IsEmpty()
        {
            return count == 0;
        }

        public long Count()
        {
            return count;
        }

        public Comparer<K>? Comparer()
        {
            return comparer;
        }

        public void Clear()
        {
            root = null;
            count = 0;
        }

        public void Meld(IMergeableAddressableHeap<K, V> other)
        {
            PairingHeap<K, V> h = (PairingHeap<K, V>) other;

            //check same comparer
            if (comparer != null)
            {
                if (h.comparer == null || !h.comparer.Equals(comparer))
                {
                    throw new ArgumentException("Cannot meld heaps using different comparers!");
                }
            } else if (h.comparer != null)
            {
                throw new ArgumentException("Cannot meld heaps using different comparers!");
            }

            if (h.other != h)
            {
                throw new InvalidOperationException("A heap cannot be used after a meld");
            }

            //perform the meld
            count += h.count;
            root = comparer == null ? Link(root, h.root) : LinkWithComparer(root, h.root);

            //clear other
            h.count = 0;
            h.root = null;

            //take ownership
            h.other = this;
        }

        #endregion public methods

        #region private methods

        private static Node<K, V>? Link(Node<K, V>? node1, Node<K, V>? node2)
        {
            if (node2 == null)
            {
                return node1;
            }
            if (node1 == null)
            {
                return node2;
            }

            if (((IComparable<K>) node1.key!).CompareTo(node2.key) <= 0)
            {
                node2.youngerSibling = node1.oldestChild;
                node2.olderSiblingOrParent = node1;

                if (node1.oldestChild != null)
                {
                    node1.oldestChild.olderSiblingOrParent = node2;
                }
                node1.oldestChild = node2;

                return node1;
            } else
            {
                return Link(node2, node1);
            }
        }

        private Node<K, V>? LinkWithComparer(Node<K, V>? node1, Node<K, V>? node2)
        {
            if (node2 == null)
            {
                return node1;
            }
            if (node1 == null)
            {
                return node2;
            }

            if (comparer!.Compare(node1.key, node2.key) <= 0)
            {
                node2.youngerSibling = node1.oldestChild;
                node2.olderSiblingOrParent = node1;

                if (node1.oldestChild != null)
                {
                    node1.oldestChild.olderSiblingOrParent = node2;
                }
                node1.oldestChild = node2;

                return node1;
            } else
            {
                return LinkWithComparer(node2, node1);
            }
        }

        /// <summary>
        /// Cut the children of a node and return the list.
        /// </summary>
        /// <param name="n">the node</param>
        /// <returns>the first node in the children list</returns>
        private static Node<K, V>? CutChildren(Node<K, V> n)
        {
            Node<K, V>? child = n.oldestChild;

            n.oldestChild = null;

            if (child != null)
            {
                child.olderSiblingOrParent = null;
            }

            return child;
        }

        /// <summary>
        /// Two pass pair and compute root.
        /// </summary>
        /// <param name="l">the node</param>
        /// <returns>the root</returns>
        private Node<K, V>? Combine(Node<K, V>? l)
        {
            if (l == null)
            {
                return null;
            }

            //left-right pass
            Node<K, V>? pairs = null;
            Node<K, V>? it = l, p_it, n_it;

            while (it != null)
            {
                p_it = it;
                it = it.youngerSibling;

                if (it == null)
                {
                    //append last node to pair list
                    p_it.youngerSibling = pairs;
                    p_it.olderSiblingOrParent = null;
                    pairs = p_it;
                } else
                {
                    n_it = it.youngerSibling;

                    //disconnect both
                    p_it.youngerSibling = null;
                    p_it.olderSiblingOrParent = null;
                    it.youngerSibling = null;
                    it.olderSiblingOrParent = null;

                    //link trees
                    p_it = comparer == null ? Link(p_it, it) : LinkWithComparer(p_it, it);

                    //append to pair list
                    p_it!.youngerSibling = pairs;
                    pairs = p_it;

                    //advance
                    it = n_it;
                }
            }

            // second pass (reverse order - due to add first)
            it = pairs;
            Node<K, V>? node = null;

            while (it != null)
            {
                n_it = it.youngerSibling;
                it.youngerSibling = null;

                node = comparer == null ? Link(node, it) : LinkWithComparer(node, it);
                it = n_it;
            }

            return node;
        }

        #endregion private methods

        /// <summary>
        /// Decrease the key of a node.
        /// </summary>
        /// <param name="n">the node</param>
        /// <param name="newKey">the new key</param>
        internal void DecreaseKey(Node<K, V> n, K newKey)
        {
            //Differenz berechnen
            int c;
            if (comparer == null)
            {
                c = ((IComparable<K>) newKey!).CompareTo(n.key);
            } else
            {
                c = comparer.Compare(newKey, n.key);
            }

            //Wenn größer -> Exception, wenn gleich oder Wurzel -> Keine weitere Aktion nötig
            if (c > 0)
            {
                throw new ArgumentException("Keys can only be decreased");
            }
            n.key = newKey;
            if (c == 0 || root == n)
            {
                return;
            }

            if (n.olderSiblingOrParent == null)
            {
                throw new ArgumentException("Invalid handle");
            }

            // unlink from parent
            if (n.youngerSibling != null)
            {
                n.youngerSibling.olderSiblingOrParent = n.olderSiblingOrParent;
            }
            if (n.olderSiblingOrParent.oldestChild == n) // I am the oldest :(
            {
                n.olderSiblingOrParent.oldestChild = n.youngerSibling;
            }
            else // I have an older sibling!
            {
                n.olderSiblingOrParent.youngerSibling = n.youngerSibling;
            }
            n.youngerSibling = null;
            n.olderSiblingOrParent = null;

            // merge with root
            root = comparer == null ? Link(root, n) : LinkWithComparer(root, n);
        }

        /// <summary>
        /// Delete a node
        /// </summary>
        /// <param name="n">node which gets deleted</param>
        internal void Delete(Node<K, V> n)
        {
            if (root == n)
            {
                DeleteMin();
                n.oldestChild = null;
                n.youngerSibling = null;
                n.olderSiblingOrParent = null;
                return;
            }

            if (n.olderSiblingOrParent == null)
            {
                throw new ArgumentException("Invalid handle");
            }

            // unlink from parent
            if (n.youngerSibling != null)
            {
                n.youngerSibling.olderSiblingOrParent = n.olderSiblingOrParent;
            }
            if (n.olderSiblingOrParent.oldestChild == n) // I am the oldest :(
            {
                n.olderSiblingOrParent.oldestChild = n.youngerSibling;
            }
            else // I have an older sibling!
            {
                n.olderSiblingOrParent.youngerSibling = n.youngerSibling;
            }
            n.youngerSibling = null;
            n.olderSiblingOrParent = null;

            //perform delete min at tree rooted at this
            Node<K, V>? t = Combine(CutChildren(n));

            // and merge with other cut tree
            root = comparer == null ? Link(root, t) : LinkWithComparer(root, t);

            count--;
        }

    }
}
