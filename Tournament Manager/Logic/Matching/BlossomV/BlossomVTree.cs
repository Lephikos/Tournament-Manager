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
    /// Represents an alternating tree of <em>tight</em> edges which is used to find an augmenting path
    /// of tight edges in order to perform an augmentation and increase the cardinality of the matching.
    /// The nodes on odd layers are necessarily connected to their children via matched edges. Thus,
    /// these nodes have always exactly one child. The nodes on even layers can have arbitrarily many
    /// children.<para/>
    /// 
    /// The tree structure information is contained in <see cref="BlossomVNode"/>, this class only contains the
    /// reference to the root of the tree. It also contains three heaps:
    /// 
    /// <list type="bullet">
    /// <item><description>
    /// A heap of (+, inf) edges. These edges are also called infinity edges. If there exists a tight
    /// infinity edge, then it can be grown. Thus, this heap is used to determine an infinity edge of
    /// inimum slack.
    /// </description></item>
    /// <item><description> 
    /// A heap of (+, +) in-tree edges. These are edges between "+" nodes from the same tree. If a
    /// (+, +) in-tree edges is tight,it can be used to perform the shrink operation and introduce a new
    /// blossom. Thus, this heap is used to determine a (+, +) in-tree edge of minimum slack in a given
    /// tree.
    /// </description></item>
    /// <item><description>
    /// A heap of "-" blossoms. If there exists a blossom with zero actual dual variable, it can be
    /// expanded. Thus, this heap is used to determine a "-" blossom with minimum dual variable
    /// </description></item>
    /// </list>
    /// 
    /// Each tree contains a variable which accumulates dual changes applied to it. The dual changes
    /// aren't spread until a tree is destroyed by an augmentation. For every node in the tree its true
    /// dual variable is equal to <c>node.dual + node.tree.eps</c> if it is a "+" node; otherwise it
    /// equals <c>node.dual - node.tree.eps</c>. This applies only to the surface nodes that belong to
    /// some tree.<para/>
    /// 
    /// This class also contains implementations of two iterators: {@link TreeEdgeIterator} and
    /// {@link TreeNodeIterator}. They are used to conveniently traverse the tree edges incident to a
    /// particular tree, and to traverse the nodes of a tree in a depth-first order.
    /// </summary>
    internal class BlossomVTree
    {

        #region member

        /// <summary>
        /// Variable for debug purposes
        /// </summary>
        private static int currentId = 1;

        /// <summary>
        /// Variable for debug purposes
        /// </summary>
        internal int id;

        /// <summary>
        /// Two-element array of the first elements in the circular doubly linked lists of incident tree
        /// edges in each direction.
        /// </summary>
        internal BlossomVTreeEdge[] first;

        /// <summary>
        /// This variable is used to quickly determine the edge between two trees during primal
        /// operations.<para/>
        /// 
        /// Let $T$ be a tree that is being processed in the main loop. For every tree $T'$ that is
        /// adjacent to $T$ this variable is set to the <c>BlossomVTreeEdge</c> that connects both
        /// trees. This variable also helps to indicate whether a pair of trees is adjacent or not. This
        /// variable is set to <c>null</c> when no primal operation can be applied to the tree $T$.
        /// </summary>
        internal BlossomVTreeEdge? currentEdge;

        /// <summary>
        /// Direction of the tree edge connecting this tree and the currently processed tree
        /// </summary>
        internal int currentDirection;

        /// <summary>
        /// Dual change that hasn't been spread among the nodes in this tree. This technique is called
        /// lazy delta spreading
        /// </summary>
        internal double eps;

        /// <summary>
        /// Accumulated dual change. Is used during dual updates
        /// </summary>
        internal double accumulatedEps;

        /// <summary>
        /// The root of this tree
        /// </summary>
        internal BlossomVNode root;

        /// <summary>
        /// Next tree in the connected component, is used during updating the duals via connected
        /// components
        /// </summary>
        internal BlossomVTree? nextTree;

        /// <summary>
        /// The heap of (+,+) edges of this tree
        /// </summary>
        internal IMergeableAddressableHeap<Double, BlossomVEdge> plusPlusEdges;

        /// <summary>
        /// The heap of (+, inf) edges of this tree
        /// </summary>
        internal IMergeableAddressableHeap<Double, BlossomVEdge> plusInfinityEdges;

        /// <summary>
        /// The heap of "-" blossoms of this tree
        /// </summary>
        internal IMergeableAddressableHeap<Double, BlossomVNode> minusBlossoms;

        #endregion member

        #region constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BlossomVTree()
        {
            this.root = new BlossomVNode(-1);
            first = new BlossomVTreeEdge[2];
            plusPlusEdges = new PairingHeap<Double, BlossomVEdge>();
            plusInfinityEdges = new PairingHeap<Double, BlossomVEdge>();
            minusBlossoms = new PairingHeap<Double, BlossomVNode>();
        }

        public BlossomVTree(BlossomVNode root)
        {
            this.root = root;
            root.tree = this;
            root.isTreeRoot = true;

            first = new BlossomVTreeEdge[2];
            plusPlusEdges = new PairingHeap<Double, BlossomVEdge>();
            plusInfinityEdges = new PairingHeap<Double, BlossomVEdge>();
            minusBlossoms = new PairingHeap<Double, BlossomVNode>();

            this.id = currentId++;
        }

        #endregion constructor 

        #region public methods

        /// <summary>
        /// Adds a new tree edge from <c>from</c> to <c>to</c>. Sets the to.currentEdge and
        /// to.currentDirection with respect to the tree {@code from}
        /// </summary>
        /// <param name="from">the tail of the directed tree edge</param>
        /// <param name="to">the head of the directed tree edge</param>
        /// <returns></returns>
        public static BlossomVTreeEdge AddTreeEdge(BlossomVTree from, BlossomVTree to)
        {
            BlossomVTreeEdge treeEdge = new ();

            treeEdge.head[0] = to;
            treeEdge.head[1] = from;

            if (from.first[0] != null)
            {
                from.first[0].prev[0] = treeEdge;
            }
            if (to.first[1] != null)
            {
                to.first[1].prev[1] = treeEdge;
            }

            treeEdge.next[0] = from.first[0];
            treeEdge.next[1] = to.first[1];

            from.first[0] = treeEdge;
            to.first[1] = treeEdge;

            to.currentEdge = treeEdge;
            to.currentDirection = 0;

            return treeEdge;
        }

        /// <summary>
        /// Sets the currentEdge and currentDirection variables for all trees adjacent to this tree
        /// </summary>
        public void SetCurrentEdges()
        {
            BlossomVTreeEdge treeEdge;

            for (TreeEdgeEnumerator enumerator = GetTreeEdgeEnumerator(); enumerator.MoveNext(); )
            {
                treeEdge = enumerator.Current;

                BlossomVTree opposite = treeEdge.head[enumerator.GetCurrentDirection()]!;
                opposite.currentEdge = treeEdge;
                opposite.currentDirection = enumerator.GetCurrentDirection();
            }
        }

        /// <summary>
        /// Clears the currentEdge variable of all adjacent to the <c>tree</c> trees
        /// </summary>
        public void ClearCurrentEdges()
        {
            currentEdge = null;

            for (TreeEdgeEnumerator enumerator = GetTreeEdgeEnumerator(); enumerator.MoveNext(); )
            {
                enumerator.Current.head[enumerator.GetCurrentDirection()]!.currentEdge = null;
            }
        }

        /// <summary>
        /// Prints all the nodes of this tree
        /// </summary>
        public void PrintTreeNodes()
        {
            Console.WriteLine("Printing tree nodes");
            for (TreeNodeEnumerator enumerator = GetTreeNodeEnumerator(); enumerator.MoveNext(); )
            {
                Console.WriteLine(enumerator.Current.ToString());
            }
        }

        public override string ToString()
        {
            return "BlossomVTree pos=" + id + ", eps = " + eps + ", root = " + root;
        }

        /// <summary>
        /// Ensures correct addition of an edge to the heap
        /// </summary>
        /// <param name="edge">a (+, +) edge</param>
        public void AddPlusPlusEdge(BlossomVEdge edge)
        {
            edge.handle = plusPlusEdges.Insert(edge.slack, edge);
        }

        /// <summary>
        /// Ensures correct addition of an edge to the heap
        /// </summary>
        /// <param name="edge">a (+, inf) edge</param>
        public void AddPlusInfinityEdge(BlossomVEdge edge)
        {
            edge.handle = plusInfinityEdges.Insert(edge.slack, edge);
        }

        /// <summary>
        /// Ensures correct addition of a blossom to the heap
        /// </summary>
        /// <param name="blossom">a "-" blossom</param>
        public void AddMinusBlossom(BlossomVNode blossom)
        {
            blossom.handle = minusBlossoms.Insert(blossom.dual, blossom);
        }

        /// <summary>
        /// Removes the <c>edge</c> from the heap of (+, +) edges
        /// </summary>
        /// <param name="edge">the edge to remove</param>
        public static void RemovePlusPlusEdge(BlossomVEdge edge)
        {
            edge.handle!.Delete();
        }

        /// <summary>
        /// Removes the <c>edge</c> from the heap of (+, inf) edges
        /// </summary>
        /// <param name="edge">the edge to remove</param>
        public static void RemovePlusInfinityEdge(BlossomVEdge edge)
        {
            edge.handle!.Delete();
        }

        /// <summary>
        /// Removes the <c>blossom</c> from the heap of "-" blossoms
        /// </summary>
        /// <param name="blossom">the blossom to remove</param>
        public static void RemoveMinusBlossom(BlossomVNode blossom)
        {
            blossom.handle!.Delete();
        }

        /// <summary>
        /// Returns a new instance of TreeNodeIterator for this tree
        /// </summary>
        /// <returns>new TreeNodeIterator for this tree</returns>
        public TreeNodeEnumerator GetTreeNodeEnumerator()
        {
            return new TreeNodeEnumerator(root);
        }

        /// <summary>
        /// Returns a new instance of TreeEdgeIterator for this tree
        /// </summary>
        /// <returns>new TreeEdgeIterators for this tree</returns>
        public TreeEdgeEnumerator GetTreeEdgeEnumerator()
        {
            return new TreeEdgeEnumerator(first);
        }

        #endregion public methods

        #region classes

        /// <summary>
        /// n iterator over tree nodes. This iterator traverses the nodes of the tree in a depth-first
        /// order. <b>Note:</b> this iterator can also be used to iterate the nodes of some subtree of a
        /// tree.
        /// </summary>
        public class TreeNodeEnumerator : IEnumerator<BlossomVNode>
        {

            private BlossomVNode? currentNode;

            /// <summary>
            /// A root of a subtree of a tree
            /// </summary>
            private readonly BlossomVNode treeRoot;

            private bool isInitialized = false;


            /// <summary>
            /// Constructs a new TreeNodeIterator for a <c>root</c>.<para/>
            /// 
            /// <b>Note:</b> <c>root</c> doesn't need to be a root of some tree; this iterator also
            /// works with subtrees.
            /// </summary>
            /// <param name="root"></param>
            public TreeNodeEnumerator(BlossomVNode root)
            {
                this.currentNode = root;
                this.treeRoot = root;
            }


            public bool MoveNext()
            {
                if (!isInitialized)
                {
                    isInitialized = true;
                    return true;
                }

                currentNode = Advance();

                return currentNode != null;
            }

            /// <summary>
            /// Advances the iterator to the next tree node
            /// </summary>
            private BlossomVNode? Advance()
            {
                if (currentNode == null)
                {
                    return currentNode;
                }

                if (currentNode.firstTreeChild != null)
                {
                    //advance deeper
                    currentNode = currentNode.firstTreeChild;
                    return currentNode;
                } else
                {
                    //advance to the next unvisited sibling of the current node or
                    //of some of its ancestors
                    while (currentNode != treeRoot && currentNode!.treeSiblingNext == null)
                    {
                        currentNode = currentNode.parentEdge!.GetOpposite(currentNode);
                    }
                    currentNode = currentNode.treeSiblingNext;

                    if (currentNode == treeRoot.treeSiblingNext)
                    {
                        currentNode = null;
                    }
                    return currentNode;
                }
            }


            public void Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose() { GC.SuppressFinalize(this); }

            public BlossomVNode Current
            {
                get
                {
                    return currentNode ?? new BlossomVNode(-1);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

        }

        /// <summary>
        /// An iterator over tree edges incident to this tree.
        /// </summary>
        public class TreeEdgeEnumerator : IEnumerator<BlossomVTreeEdge>
        {

            private readonly BlossomVTreeEdge[] first;

            private int currentDirection;

            private BlossomVTreeEdge? currentEdge;

            private bool isInitialized = false;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="first"></param>
            public TreeEdgeEnumerator(BlossomVTreeEdge[] first)
            {
                this.first = first;

                currentEdge = first[0];
                currentDirection = 0;

                if (currentEdge == null)
                {
                    currentEdge = first[1];
                    currentDirection = 1;
                }
            }


            public bool MoveNext()
            {
                if (!isInitialized && currentEdge != null)
                {
                    isInitialized = true;
                    return true;
                }

                Advance();

                return currentEdge != null;
            }

            /// <summary>
            /// Moves this iterator to the next tree edge. If the last outgoing edge has been traversed,
            /// changes the current direction to 1. If the the last incoming edge has been traversed,
            /// sets <c>currentEdge</c> to null.
            /// </summary>
            private void Advance()
            {
                if (currentEdge == null)
                {
                    return;
                }

                currentEdge = currentEdge.next[currentDirection];

                if (currentEdge == null && currentDirection == 0)
                {
                    currentEdge = first[1];
                    currentDirection = 1;
                }
            }

            /// <summary>
            /// Returns the direction of the current edge
            /// </summary>
            /// <returns>the direction of the current edge</returns>
            public int GetCurrentDirection()
            {
                return currentDirection;
            }


            public void Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose() { GC.SuppressFinalize(this); }

            public BlossomVTreeEdge Current
            {
                get
                {
                    return currentEdge ?? new BlossomVTreeEdge();
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

        }

        #endregion classes

    }
}
