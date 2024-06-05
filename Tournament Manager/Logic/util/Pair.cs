using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Data;

namespace Tournament_Manager.Logic.util
{

    /// <summary>
    /// Generic pair.
    /// </summary>
    /// <typeparam name="A">the first element type</typeparam>
    /// <typeparam name="B">the second element type</typeparam>
    internal class Pair<A, B>
    {

        /// <summary>
        /// The first pair element
        /// </summary>
        protected A first;

        /// <summary>
        /// The second pair element
        /// </summary>
        protected B second;


        /// <summary>
        /// Create a new pair
        /// </summary>
        /// <param name="first">the first element</param>
        /// <param name="second">the second element</param>
        public Pair(A first, B second)
        {
            this.first = first;
            this.second = second;
        }


        public A GetFirst() { return first; }

        public B GetSecond() { return second; }


        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }
            else if (!(obj is Pair<A, B>))
            {
                return false;
            }

            Pair < A, B > other = (Pair<A, B>)obj;

            return Object.Equals(first, other.first) && Object.Equals(second, other.second);
        }

        public override int GetHashCode()
        {
            int hashCode = 137;

            hashCode = (hashCode * 397) ^ (first != null ? first.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (second != null ? second.GetHashCode() : 0);

            return hashCode;
        }

        public override string ToString()
        {
            return "(" + first + ", " + second + ")";
        }

    }
}
