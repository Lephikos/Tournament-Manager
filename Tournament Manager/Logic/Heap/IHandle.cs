using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Heap
{

    /// <summary>
    /// A heap element handle. Allows someone to address an element already in a
    /// heap and perform additional operations.
    /// </summary>
    /// <typeparam name="K">the type of keys maintained by this heap</typeparam>
    /// <typeparam name="V">the type of values maintained by this heap</typeparam>
    internal interface IHandle<K, V>
    {

        /// <summary>
        /// Return the key of the element.
        /// </summary>
        /// <returns>the key of the element</returns>
        internal K GetKey();

        /// <summary>
        /// Return the value of the element.
        /// </summary>
        /// <returns>the value of the element</returns>
        internal V? GetValue();

        /// <summary>
        /// Set the value of the element.
        /// </summary>
        /// <param name="value">the new value</param>
        internal void SetValue(V value);

        /// <summary>
        /// Decrease the key of the element. Throws an ArgumentException()
        /// if the new key is larger than the old key according to the comparator used when constructing 
        /// the heap or the natural ordering of the elements if no comparator was used
        /// </summary>
        /// <param name="newKey">the new key</param>
        /// 
        internal void DecreaseKey(K newKey);

        /// <summary>
        /// Delete the element from the heap that it belongs. Throws an ArgumentException()
        /// in case this function is called twice on the same element 
        /// or the element has already been deleted using <see cref="DeleteMin"/>.
        /// </summary>
        internal void Delete();

    }
}
