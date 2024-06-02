using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Heap
{

    /// <summary>
    /// An addressable heap that allows melding with another addressable heap.<para/>
    /// 
    /// The second heap becomes empty and unusable after the meld operation, meaning
    /// that further insertions are not possible and will throw an
    /// <see cref="InvalidOperationException"/>
    /// 
    /// A <see cref="InvalidCastException"/> will be thrown if the two heaps are not of the
    /// same type. Moreover, the two heaps need to use the same comparators. If only
    /// one of them uses a custom comparator or both use custom comparators but are
    /// not the same by <em>equals</em>, an <see cref="ArgumentException"/> is
    /// thrown.<para/>
    /// 
    /// Note that all running time bounds on mergeable heaps are valid assuming that
    /// the user does not perform cascading melds on heaps such as:<para/>
    /// 
    /// d.meld(e) -> c.meld(d) -> b.meld(c) -> a.meld(b)<para/>
    /// 
    /// The above scenario, although efficiently supported by using union-find with
    /// path compression, invalidates the claimed bounds.
    /// </summary>
    /// <typeparam name="K">the type of keys maintained by this heap</typeparam>
    /// <typeparam name="V">the type of values maintained by this heap</typeparam>
    internal interface IMergeableAddressableHeap<K, V> : IAddressableHeap<K, V>
    {

        /// <summary>
        /// Meld a heap into the current heap.<para/>
        /// 
        /// After the operation the <c>other</c> heap will be empty and will not
        /// permit further insertions.
        /// </summary>
        /// <param name="other">a merge-able heap</param>
        /// <exception cref="InvalidCastException"/>
        /// <exception cref="InvalidOperationException"/>
        internal void Meld(IMergeableAddressableHeap<K, V> other);

    }
}
