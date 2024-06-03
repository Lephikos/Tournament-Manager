using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Heap;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// This class is a data structure for Kolmogorov's Blossom V algorithm.<para/>
    /// 
    /// Is used to maintain an auxiliary graph whose nodes correspond to alternating trees in the Blossom
    /// V algorithm. Let's denote the current tree $T$ and some other tree $T'$. Every tree edge contains
    /// three heaps:
    /// <list type="bullet">
    /// <item><description>
    /// a heap of (+, +) cross-tree edges. This heap contains all edges between two "+" nodes where
    /// one node belongs to tree $T$ and another to $T'$. The (+, +) cross-tree edges are used to augment
    /// the matching.
    /// </description></item>
    /// <item><description>
    /// a heap of (+, -) cross-tree edges
    /// </description></item>
    /// <item><description>
    /// a heap of (-, +) cross-tree edges
    /// </description></item>
    /// </list>
    /// 
    /// <b>Note:</b> from the tree edge perspective there is no difference between a heap of (+, -) and
    /// (-, +) cross-tree edges. That's why we distinguish these heaps by the direction of the edge. Here
    /// the direction is considered with respect to the trees $T$ and $T'$ based upon the notation
    /// introduced above.<para/>
    /// 
    /// Every tree edge is directed from one tree to another and every tree edge belongs to the two
    /// doubly linked lists of tree edges. The presence of a tree edge in these lists in maintained by
    /// the two-element arrays <see cref="prev"/> and <see cref="next"/>. For one
    /// tree the edge is an outgoing tree edge; for the other, an incoming. In the first case it belongs
    /// to the <c>tree.first[0]</c> linked list; in the second, to the <c>tree.first[1]</c> linked
    /// list.<para/>
    /// 
    /// Let <c>tree</c> be a tail of the edge, and <c>oppositeTree</c> a head of the edge. Then
    /// <c>edge.head[0] == oppositeTree</c> and <c>edge.head[1] == tree</c>.
    /// </summary>
    internal class BlossomVTreeEdge
    {

        #region member

        /// <summary>
        /// Two-element array of trees this edge is incident to.
        /// </summary>
        internal BlossomVTree?[] head;

        /// <summary>
        /// A two-element array of references to the previous elements in the circular doubly linked
        /// lists of tree edges. The lists are circular with one exception: the lastElement.next[dir] ==
        /// null. Each list belongs to one of the endpoints of this edge.
        /// </summary>
        internal BlossomVTreeEdge[] prev;

        /// <summary>
        /// A two-element array of references to the next elements in the circular doubly linked lists of
        /// tree edges. The lists are circular with one exception: the lastElementInTheList.next[dir] ==
        /// null. Each list belongs to one of the endpoints of this edge.
        /// </summary>
        internal BlossomVTreeEdge[] next;

        /// <summary>
        /// A heap of (+, +) cross-tree edges
        /// </summary>
        internal readonly IMergeableAddressableHeap<Double, BlossomVEdge> plusPlusEdges;

        /// <summary>
        /// A heap of (-, +) cross-tree edges
        /// </summary>
        internal readonly IMergeableAddressableHeap<Double, BlossomVEdge> plusMinusEdges0;

        /// <summary>
        /// A heap of (+, -) cross-tree edges
        /// </summary>
        internal readonly IMergeableAddressableHeap<Double, BlossomVEdge> plusMinusEdges1;

        #endregion member

        #region constructor

        public BlossomVTreeEdge()
        {
            this.head = new BlossomVTree[2];
            this.prev = new BlossomVTreeEdge[2];
            this.next = new BlossomVTreeEdge[2];
            this.plusPlusEdges = new PairingHeap<Double, BlossomVEdge>();
            this.plusMinusEdges0 = new PairingHeap<Double, BlossomVEdge>();
            this.plusMinusEdges1 = new PairingHeap<Double, BlossomVEdge>();
        }

        #endregion constructor

        #region public methods

        /// <summary>
        /// Removes this edge from both doubly linked lists of tree edges.
        /// </summary>
        public void RemoveFromTreeEdgeList()
        {
            for (int dir = 0; dir < 2; dir++)
            {
                if (prev[dir] != null)
                {
                    prev[dir].next[dir] = next[dir];
                } else
                {
                    //this is the first edge in this direction
                    head[1 - dir]!.first[dir] = next[dir];
                }

                if (next[dir] != null)
                {
                    next[dir].prev[dir] = prev[dir];
                }
            }

            head[0] = head[1] = null;
        }

        public override string ToString()
        {
            return "BlossomVTreeEdge (" + head[0]!.id + ":" + head[1]!.id + ")";
        }

        /// <summary>
        /// Adds <c>edge</c> to the heap of (-, +) cross-tree edges. As explained in the class
        /// description, this method chooses <see cref="plusMinusEdges0"/> or
        /// <see cref="plusMinusEdges1"/> based upon the <c>direction</c>. The key is
        /// edge.slack
        /// </summary>
        /// <param name="edge">an edge to add to the current heap of (-, +) cross-tree edges.</param>
        /// <param name="direction">direction of this tree edge wrt. current tree and opposite tree</param>
        public void AddToCurrentMinusPlusHeap(BlossomVEdge edge, int direction)
        {
            edge.handle = GetCurrentMinusPlusHeap(direction).Insert(edge.slack, edge);
        }

        /// <summary>
        /// Adds <c>edge</c> to the heap of (+, -) cross-tree edges. As explained in the class
        /// description, this method chooses <see cref="plusMinusEdges0"/> or
        /// <see cref="plusMinusEdges1"/> based upon the <c>direction</c>. The key is
        /// edge.slack
        /// </summary>
        /// <param name="edge">an edge to add to the current heap of (-, +) cross-tree edges.</param>
        /// <param name="direction">direction of this tree edge wrt. current tree and opposite tree</param>
        public void AddToCurrentPlusMinusHeap(BlossomVEdge edge, int direction)
        {
            edge.handle = GetCurrentPlusMinusHeap(direction).Insert(edge.slack, edge);
        }

        /// <summary>
        ///  Adds <c>edge</c> to the heap of (+, +) cross-tree edges. The key is edge.slack
        /// </summary>
        /// <param name="edge">an edge to add to the heap of (+, +) cross-tree edges</param>
        public void AddPlusPlusEdge(BlossomVEdge edge)
        {
            edge.handle = plusPlusEdges.Insert(edge.slack, edge);
        }

        /// <summary>
        /// Removes <c>edge</c> from the current heap of (-, +) cross-tree edges. As explained in the
        /// class description, this method chooses <see cref="plusMinusEdges0"/> or
        /// <see cref="plusMinusEdges1"/> based upon the <c>direction</c>.
        /// </summary>
        /// <param name="edge">an edge to remove</param>
        public static void RemoveFromCurrentMinusPlusHeap(BlossomVEdge edge)
        {
            edge.handle!.Delete();
            edge.handle = null;
        }

        /// <summary>
        /// Removes <c>edge</c> from the current heap of (+, -) cross-tree edges. As explained in the
        /// class description, this method chooses <see cref="plusMinusEdges0"/> or
        /// <see cref="plusMinusEdges1"/> based upon the <c>direction</c>.
        /// </summary>
        /// <param name="edge">an edge to remove</param>
        public static void RemoveFromCurrentPlusMinusHeap(BlossomVEdge edge)
        {
            edge.handle!.Delete();
            edge.handle = null;
        }

        /// <summary>
        /// Removes <c>edge</c> from the heap of (+, +) cross-tree edges.
        /// </summary>
        /// <param name="edge">an edge to remove</param>
        public static void RemoveFromPlusPlusHeap(BlossomVEdge edge)
        {
            edge.handle!.Delete();
            edge.handle = null;
        }

        /// <summary>
        /// Returns the current heap of (-, +) cross-tree edges. Always returns a heap different from
        /// <c>getCurrentPlusMinusHeap(currentDir)</c>
        /// </summary>
        /// <param name="currentDir">the current direction of this edge</param>
        /// <returns>current heap of (-, +) cross-tree edges</returns>
        public IMergeableAddressableHeap<Double, BlossomVEdge> GetCurrentMinusPlusHeap(int currentDir)
        {
            return currentDir == 0 ? plusMinusEdges0 : plusMinusEdges1;
        }

        /// <summary>
        /// Returns the current heap of (+, -) cross-tree edges. Always returns a heap different from
        /// <c>getCurrentMinusPlusHeap(currentDir)}</c>
        /// </summary>
        /// <param name="currentDir">the current direction of this edge</param>
        /// <returns>current heap of (+, -) cross-tree edges</returns>
        public IMergeableAddressableHeap<Double, BlossomVEdge> GetCurrentPlusMinusHeap(int currentDir)
        {
            return currentDir == 0 ? plusMinusEdges1 : plusMinusEdges0;
        }

        #endregion public methods

    }
}
