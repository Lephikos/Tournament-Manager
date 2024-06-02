using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Heap
{

    /// <summary>
    /// A heap whose elements can be addressed using handles.<para/>
    /// 
    /// An insert operation returns a <see cref="IHandle{Ke, Va}"/> which can later
    /// be used in order to manipulate the element, such as decreasing its key, or
    /// deleting it. Storing the handle externally is the responsibility of the user.
    /// </summary>
    /// <typeparam name="K">the type of keys maintained by this heap</typeparam>
    /// <typeparam name="V">the type of values maintained by this heap</typeparam>
    internal interface IAddressableHeap<K, V>
    {

        /// <summary>
        /// Returns the comparator used to order the keys in this AddressableHeap, or
        /// <c>null</c> if this heap uses the Comparable natural ordering of its keys.
        /// </summary>
        /// <returns>the comparator used to order the keys in this heap, or
        ///          <c>null</c> if this addressable heap uses the natural ordering
        ///          of its keys</returns>
        internal Comparer<K>? Comparer();

        /// <summary>
        /// Insert a new element into the heap.
        /// </summary>
        /// <param name="key">the element's key</param>
        /// <param name="value">the element's value</param>
        /// <returns>a handle for the newly added element</returns>
        internal IHandle<K, V> Insert(K key, V value);

        /// <summary>
        /// Insert a new element into the heap with a null value.
        /// </summary>
        /// <param name="key">the element's key</param>
        /// <returns>a handle for the newly added element</returns>
        internal IHandle<K, V> Insert(K key);

        /// <summary>
        /// Find an element with the minimum key.
        /// </summary>
        /// <returns>a handle to an element with minimum key</returns>
        internal IHandle<K, V> FindMin();

        /// <summary>
        /// Delete and return an element with the minimum key. If multiple such
        /// elements exists, only one of them will be deleted. After the element is
        /// deleted the handle is invalidated and only method <see cref="IHandle{Ke, Va}.GetKey"/>
        /// and <see cref="IHandle{Ke, Va}.GetValue"/> can be used.
        /// </summary>
        /// <returns>a handle to the deleted element with minimum key</returns>
        internal IHandle<K, V> DeleteMin();

        /// <summary>
        /// Returns <c>true</c> if this heap is empty.
        /// </summary>
        /// <returns><c>true</c> if this heap is empty, <c>false</c> otherwise</returns>
        internal bool IsEmpty();

        /// <summary>
        /// Returns the number of elements in the heap.
        /// </summary>
        /// <returns>the number of elements in the heap</returns>
        internal long Count();

        /// <summary>
        /// Clear all the elements of the heap. After calling this method all handles
        /// should be considered invalidated and the behavior of methods
        /// <see cref="IHandle{Ke, Va}.DecreaseKey(Ke)"/> and <see cref="IHandle{Ke, Va}.Delete"/> is
        /// undefined.
        /// </summary>
        internal void Clear();

    }
}
