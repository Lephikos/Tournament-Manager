using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using Tournament_Manager.Logic.Heap;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.LinkLabel;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// This class is a data structure for Kolmogorov's Blossom V algorithm.<para/>
    /// 
    /// It represents a vertex of graph, and contains three major blocks of data needed for the algorithm.
    /// 
    /// <list type="bullet">
    /// <item><description>
    /// Node's state information, i.e. <see cref="label"/>, <see cref="isTreeRoot"/>, etc. 
    ///  This information is maintained dynamically and is changed by <see cref="BlossomVPrimalUpdater"/>
    ///  </description></item>
    /// <item><description>
    /// Information needed to maintain alternating tree structure. 
    /// It is designed to be able to quickly plant subtrees, split and concatenate child lists, traverse the tree up and down
    /// </description></item>
    /// <item><description>
    /// information needed to maintain a "pyramid" of contracted nodes. The common use-cases are to traverse the nodes of a blossom,
    /// to move from some node up to the outer blossom(or penultimate blossom, if the outer one is being expanded)
    /// </description></item>
    /// </list>
    /// 
    /// Each node has a dual variable. This is the only information that can be changed by the <see cref="BlossomVDualUpdater"/>. 
    /// This variable is updated lazily due to performance reasons.<para/>
    /// 
    /// The edges incident to a node are stored in two linked lists. The first linked list is used for outgoing edges; the other, for incoming edges.
    /// The notions of outgoing and incoming edges are symmetric in the context of this algorithm since the initial graph is undirected. 
    /// The first element in the list of outgoing edges is <see cref="first"/>[0], 
    /// the first element in the list of incoming edges is <see cref="first"/>[1].<para/>
    /// 
    /// A node is called a <i>plus</i> node if it belongs to the even layer of some alternating tree (root has layer 0). 
    /// Then its label is <see cref="Label.PLUS"/>. 
    /// A node is called a <i>minus</i> node if it belongs to the odd layer of some alternating tree. 
    /// Then its label is <see cref="Label.MINUS"/>. 
    /// A node is called an <i>infinity</i> or <i>free</i> node if it doesn't belong to any alternating tree.
    /// A node is called <i>outer</i> if it belongs to the surface graph, i.e. it is not contracted. 
    /// A node is called a <i>blossom</i> or <i>pseudonode</i> if it emerged from contracting an odd circuit.
    /// This implies that this node doesn't belong to the original graph. 
    /// A node is called <i>matched</i>, if it is matched to some other node. 
    /// If a node is free, it means that it is matched. 
    /// If a node is not a free node, then it necessarily belongs to some tree.
    /// If a node isn't matched, it necessarily is a tree root.
    /// 
    /// </summary>
    internal class BlossomVNode
    {

        #region member

        /// <summary>
        /// Node from the heap this node is stored in
        /// </summary>
        internal IHandle<Double, BlossomVNode>? handle;

        /// <summary>
        /// True if this node is a tree root, implies that this node is outer and isn't matched.
        /// </summary>
        internal bool isTreeRoot;

        /// <summary>
        /// True if this node is a blossom node (also called a "pesudonode", the notions are equivalent)
        /// </summary>
        internal bool isBlossom;

        /// <summary>
        /// True if this node is outer, i.e. it isn't contracted in some blossom and belongs to the surface graph
        /// </summary>
        internal bool isOuter;

        /// <summary>
        /// Support variable to identify the nodes which have been "processed" in some sense by the algorithm.
        /// Is used in the shrink and expand operations.<para/>
        /// 
        /// For example, during the shrink operation we traverse the odd circuit and apply dual changes.
        /// All nodes from this odd circuit are marked, i.e. <see cref="isMarked"/>. When a node on this
        /// circuit is traversed, we set <see cref="isProcessed"/> to <see langword="true"/>. When a (+, +)
        /// inner edge is encountered, we can determine whether the opposite endpoint has been processed or
        /// not depending on the value of this variable. Without this variable inner (+, +) edges can be
        /// processed twice (which is wrong).
        /// </summary>
        internal bool isProcessed;

        /// <summary>
        /// Support variable. In particular, it is used in shrink and expand operation to quickly
        /// determine whether a node belongs to the current blossom or not. Is similar to the
        /// <see cref="isProcessed"/>.
        /// </summary>
        internal bool isMarked;

        /// <summary>
        /// Current label of this node. Is valid if this node is outer.
        /// </summary>
        internal Label label;

        /// <summary>
        /// Two-element array of references of the first elements in the linked lists of edges
        /// that are incident to this node. first[0] is the first outgoing edge, first[1] is
        /// the first incoming edge, see <see cref="BlossomVEdge"/>.
        /// </summary>
        internal BlossomVEdge?[] first;

        /// <summary>
        /// Current dual variable of this node. If the node belongs to a tree and is an outer
        /// node, then this value may not be valid.
        /// </summary>
        internal double dual;

        /// <summary>
        /// An edge which is incident to this node and currently belongs to the matching.
        /// </summary>
        internal BlossomVEdge? matched;

        /// <summary>
        /// A (+, inf) edge incident to this node. This variable is used during fractional
        /// matching initialization and is assigned only to the infinity nodes. In fact, it
        /// is used to determine for a particular infinity node the "cheapest" edge to connect
        /// it to the tree. The "cheapest" means the edge with the minimum slack. When the
        /// dual change is bounded by the dual constraints on the (+, inf) edges, we choose
        /// the "cheapest" best edge, increase the duals of the tree if needed, and grow
        /// this edge.
        /// </summary>
        internal BlossomVEdge? bestEdge;

        /// <summary>
        /// Reference to the tree this node belongs to.
        /// </summary>
        internal BlossomVTree? tree;

        /// <summary>
        /// An edge to the parent node in the tree structure.
        /// </summary>
        internal BlossomVEdge? parentEdge;

        /// <summary>
        /// The first child in the linked list of children of this node.
        /// </summary>
        internal BlossomVNode? firstTreeChild;

        /// <summary>
        /// Reference of the next tree sibling in the doubly linked list of children of
        /// the node parentEdge.GetOpposite(this). Is null if this node is the last
        /// child of the parent node.<para/>
        /// 
        /// If this node is a tree root, references the next tree root in the doubly
        /// linked list of tree roots or is null if this is the last tree root.
        /// </summary>
        internal BlossomVNode? treeSiblingNext;

        /// <summary>
        /// Reference of the previous tree sibling in the doubly linked list of children of
        /// the node parentEdge.GetOpposite(this). If this node is the first child of the
        /// parent node (i.e. parentEdge.GetOpposite(this).FirstTreeChild == this), references
        /// the last sibling.<para/>
        /// 
        /// If this node is a tree root, references the previous tree root in the doubly linked
        /// list of tree roots. The first element in the linked list of tree roots is a dummy
        /// node which is stored in <see cref="BlossomVState{V, E}.nodes"/>[nodeNum]. This is done to quickly determine the first actual
        /// tree root via <see cref="BlossomVState{V, E}.nodes"/>[nodeNum].treeSiblingNext.
        /// </summary>
        internal BlossomVNode? treeSiblingPrev;

        /// <summary>
        /// Reference of the blossom this node is contained in. The blossom parent is always one
        /// layer higher than this node.
        /// </summary>
        internal BlossomVNode? blossomParent;

        /// <summary>
        /// Reference of some blossom that is higher than this node. This variable is used for
        /// the path compression technique. It is used to quickly find the penultimate
        /// grandparent of this node, i.e. a grandparent whose blossomParent is an outer node.
        /// </summary>
        internal BlossomVNode? blossomGrandparent;

        /// <summary>
        /// Reference of the next node in the blossom structure in the circular singly linked
        /// list of blossom nodes. Is used to traverse the blossom nodes in a circular order.
        /// </summary>
        internal BlossomVEdge? blossomSibling;

        /// <summary>
        /// Position of this node in the array <see cref="BlossomVState.nodes"/>. This helps
        /// to determine generic counterpart of this node in constant time.
        /// </summary>
        internal int pos;

        #endregion member

        #region constructor

        /// <summary>
        /// Constructs a new "+" node with a <see cref="Label.PLUS"/> label.
        /// </summary>
        /// <param name="pos"></param>
        public BlossomVNode(int pos)
        {
            this.first = new BlossomVEdge[2];
            this.label = Label.PLUS;
            this.pos = pos;
        }

        #endregion constructor

        #region public methods

        /// <summary>
        /// Insert the <c>edge</c> into linked list of incident edges of this node in
        /// the specified direction <c>dir</c>.
        /// </summary>
        /// <param name="edge">edge to insert in the linked list of incident edges</param>
        /// <param name="dir">the direction of this edge with respect to this node</param>
        public void AddEdge(BlossomVEdge edge, int dir)
        {
            if (first[dir] == null)
            {
                first[dir] = edge.next[dir] = edge.prev[dir] = edge;
            } else
            {
                edge.prev[dir] = first[dir]!.prev[dir];
                edge.next[dir] = first[dir]!;
                first[dir]!.prev[dir].next[dir] = edge;
                first[dir]!.prev[dir] = edge;
            }

            // this constraint is used to maintain the following feature: if an edge has direction
            // dir with respect to this node, then edge.head[dir] is the opposite node
            edge.head[1 - dir] = this;
        }

        /// <summary>
        /// Removes the <c>edge</c> from the linked list of edges incident to this node. Updates the
        /// first[dir] reference if needed.
        /// </summary>
        /// <param name="edge">the edge to remove</param>
        /// <param name="dir">the directions of the <c>edge</c> with respect to this node</param>
        public void RemoveEdge(BlossomVEdge edge, int dir)
        {
            if (edge.prev[dir] == edge)
            {
                first[dir] = null;
            } else
            {
                edge.prev[dir].next[dir] = edge.next[dir];
                edge.next[dir].prev[dir] = edge.prev[dir];
                if (first[dir] == edge)
                {
                    first[dir] = edge.next[dir];
                }
            }
        }

        /// <summary>
        /// Helper method, returns the tree grandparent of this node or null if this node 
        /// has no grandparent
        /// </summary>
        /// <returns>the tree grandparent of this node or null if this node has no grandparent</returns>
        public BlossomVNode? GetTreeGrandparent()
        {
            BlossomVNode? t = parentEdge?.GetOpposite(this);
            return t?.parentEdge?.GetOpposite(t);
        }

        /// <summary>
        /// Helper method, returns the tree parent of this node or null if this node has no
        /// tree parent
        /// </summary>
        /// <returns>node's parent or null if this node has no tree parent</returns>
        public BlossomVNode? GetTreeParent()
        {
            return parentEdge?.GetOpposite(this);
        }

        /// <summary>
        /// Appends the <c>child</c> to the end of the linked list of children of this node. The
        /// <c>parentEdge</c> becomes the parent edge of the <c>child</c>.<para/>
        /// 
        /// Variable <c>grow</c> is used to determine whether the <c>child</c> was an infinity node
        /// and now is being added in tree structure. Then we have to set <c>child.firstTreeChild</c>
        /// to <see langword="null"/> so that all its tree structure variables are changed. This allows
        /// us to avoid overwriting the fields during tree destroying.
        /// </summary>
        /// <param name="child">the new child of this node</param>
        /// <param name="parentEdge">the edge between this node and <c>child</c></param>
        /// <param name="grow">true if <c>child</c> is being grown</param>
        public void AddChild(BlossomVNode child, BlossomVEdge parentEdge, Boolean grow)
        {
            child.parentEdge = parentEdge;
            child.tree = tree;
            child.treeSiblingNext = firstTreeChild;

            if (grow)
            {
                child.firstTreeChild = null; // with this check we are able to avoid destroying the tree structure during the augment operation
            }

            if (firstTreeChild == null)
            {
                child.treeSiblingPrev = child;
            } else
            {
                child.treeSiblingPrev = firstTreeChild.treeSiblingPrev;
                firstTreeChild.treeSiblingPrev = child;
            }
            firstTreeChild = child;
        }

        /// <summary>
        /// Helper method, returns a node this node is matched to
        /// </summary>
        /// <returns>a node this node is matched to</returns>
        public BlossomVNode? GetOppositeMatched()
        {
            return matched?.GetOpposite(this);
        }

        /// <summary>
        /// If this node is a tree root then this method removes this node from the tree root doubly
        /// linked list. Otherwise, removes this vertex from the doubly linked list of tree children and
        /// updates parent.firstTreeChild accordingly.
        /// </summary>
        public void RemoveFromChildList()
        {
            if (isTreeRoot)
            {
                treeSiblingPrev!.treeSiblingNext = treeSiblingNext;
                if (treeSiblingNext != null)
                {
                    treeSiblingNext.treeSiblingPrev = treeSiblingPrev;
                }
            } else
            {
                if (treeSiblingPrev!.treeSiblingNext == null)
                {
                    // this vertex is the first child => we have to update parent.firstTreeChild
                    parentEdge!.GetOpposite(this)!.firstTreeChild = treeSiblingNext; 
                } else
                {
                    // this vertex isn't the first child
                    treeSiblingPrev.treeSiblingNext = treeSiblingNext;
                }

                if (treeSiblingNext == null)
                {
                    // this vertex is the last child => we have to set treeSiblingPrev of the firstChild
                    if (parentEdge!.GetOpposite(this)!.firstTreeChild != null)
                    {
                        parentEdge.GetOpposite(this)!.firstTreeChild!.treeSiblingPrev = treeSiblingPrev;
                    }
                } else
                {
                    // this vertex isn't the last child
                    treeSiblingNext.treeSiblingPrev = treeSiblingPrev;
                }
            }
        }

        /// <summary>
        /// Appends the child list of this node to the beginning of the child list of the <c>blossom</c>
        /// </summary>
        /// <param name="blossom">the node to which the children of the current node are moved</param>
        public void MoveChildrenTo(BlossomVNode blossom)
        {
            if (firstTreeChild != null)
            {
                if (blossom.firstTreeChild == null)
                {
                    blossom.firstTreeChild = firstTreeChild;
                } else
                {
                    BlossomVNode t = blossom.firstTreeChild.treeSiblingPrev!;

                    // concatenating child lists
                    firstTreeChild.treeSiblingPrev!.treeSiblingNext = blossom.firstTreeChild;
                    blossom.firstTreeChild.treeSiblingPrev = firstTreeChild.treeSiblingPrev;

                    // setting reference to the last child and updating firstTreeChild reference of the
                    // blossom
                    firstTreeChild.treeSiblingPrev = t;
                    blossom.firstTreeChild = firstTreeChild;
                }
                firstTreeChild = null; // now this node has no children
            }
        }

        /// <summary>
        /// Computes and returns the penultimate blossom of this node, i.e. the blossom which isn't outer
        /// but whose blossomParent is outer. This method also applies path compression technique to the
        /// blossomGrandparent references. More precisely, it finds the penultimate blossom of this node
        /// and changes blossomGrandparent references of the previous nodes to point to the resulting
        /// penultimate blossom.
        /// </summary>
        /// <returns>the penultimate blossom of this node</returns>
        public BlossomVNode GetPenultimateBlossom()
        {
            BlossomVNode current = this;

            while(true)
            {
                if (!current.blossomGrandparent!.isOuter)
                {
                    current = current.blossomGrandparent;
                } else if (current.blossomGrandparent != current.blossomParent)
                {
                    // this is the case when current.blossomGrandparent has been removed
                    current.blossomGrandparent = current.blossomParent;
                } else
                {
                    break;
                }
            }

            //Current references the penultimate blossom we were looking for. Now we change
            //blossomParent references to point to current

            BlossomVNode prev = this;
            BlossomVNode next;

            while (prev != current)
            {
                next = prev.blossomGrandparent!;
                prev.blossomGrandparent = current; //apply path compression
                prev = next;
            }

            return current;
        }

        /// <summary>
        /// Computes and returns the penultimate blossom of this node. The return value of this method
        /// always equals to the value returned by {@link BlossomVNode#getPenultimateBlossom()}. However,
        /// the main difference is that this method changes the blossomGrandparent references to point to
        /// the node that is previous to the resulting penultimate blossom. This method is used during
        /// the expand operation.
        /// </summary>
        /// <returns>the penultimate blossom of this node</returns>
        public BlossomVNode GetPenultimateBlossomAndFixBlossomGrandparent()
        {
            BlossomVNode current = this;
            BlossomVNode? prev = null;
            while (true)
            {
                if (!current.blossomGrandparent!.isOuter)
                {
                    prev = current;
                    current = current.blossomGrandparent;
                }
                else if (current.blossomGrandparent != current.blossomParent)
                {
                    // this is the case when current.blossomGrandparent has been removed
                    current.blossomGrandparent = current.blossomParent;
                }
                else
                {
                    break;
                }
            }
            /*
             * Now current node is the penultimate blossom, prev.blossomParent == current. All the
             * nodes, that are lower than prev, must have blossomGrandparent referencing a node, that is
             * not higher than prev
             */
            if (prev != null)
            {
                BlossomVNode prevNode = this;
                BlossomVNode nextNode;
                while (prevNode != prev)
                {
                    nextNode = prevNode.blossomGrandparent!;
                    prevNode.blossomGrandparent = prev;
                    prevNode = nextNode;
                }
            }

            return current;
        }


        /// <summary>
        /// Checks whether this node is a plus node
        /// </summary>
        /// <returns>true if the label of this node is <see cref="Label.PLUS"/>, false otherwise</returns>
        public bool IsPlusNode()
        {
            return label == Label.PLUS;
        }

        /// <summary>
        /// Checks whether this node is a plus node
        /// </summary>
        /// <returns>true if the label of this node is <see cref="Label.MINUS"/>, false otherwise</returns>
        public bool IsMinusNode()
        {
            return label == Label.MINUS;
        }

        /// <summary>
        /// Checks whether this node is a plus node
        /// </summary>
        /// <returns>true if the label of this node is <see cref="Label.INFINITY"/>, false otherwise</returns>
        public bool IsInifinityNode()
        {
            return label == Label.INFINITY;
        }

        /// <summary>
        /// Returns the true dual variable of this node. If this node is outer and belongs to some tree
        /// then it is subject to the lazy delta spreading technique. Otherwise, its dual is valid.
        /// </summary>
        /// <returns>the actual dual variable of this node</returns>
        public double GetTrueDual()
        {
            if (IsInifinityNode() || !isOuter)
            {
                return dual;
            }

            return IsPlusNode() ? dual + tree!.eps : dual - tree!.eps;
        }

        /// <summary>
        /// Returns an iterator over all incident edges of this node
        /// </summary>
        /// <returns>a new instance of IncidentEdgeIterator for this node</returns>
        public IncidentEdgeEnumerator GetIncidentEdgeEnumerator()
        {
            return new IncidentEdgeEnumerator(this.first!);
        }

        public override string ToString()
        {
            return "BlossomVNode pos = " + pos + ", dual: " + dual + ", true dual: " + GetTrueDual()
            + ", label: " + label + (isMarked ? ", marked" : "")
            + (isProcessed ? ", processed" : "")
            + (blossomParent == null || isOuter ? "" : ", blossomParent = " + blossomParent.pos)
            + (matched == null ? "" : ", matched = " + matched);
        }

        #endregion public methods

        #region classes

        /// <summary>
        /// Represents nodes' labels
        /// </summary>
        public enum Label
        {
            /// <summary>
            /// The node is on an even layer in the tree (root has layer 0)
            /// </summary>
            PLUS,

            /// <summary>
            /// The node is on an odd layer in the tree (root has layer 0)
            /// </summary>
            MINUS,

            /// <summary>
            /// This node doesn't belong to any tree and is matched to some other node
            /// </summary>
            INFINITY
        }

        /// <summary>
        /// An iterator for traversing the edges incident to this node.<para/>
        /// 
        /// This iterator has a feature that during every step it knows the next edge it'll return to the
        /// caller. That's why it is safe to modify the current edge (move it to another node, for example).
        /// </summary>
        public class IncidentEdgeEnumerator : IEnumerator<BlossomVEdge>
        {

            /// <summary>
            /// The direction of the current edge
            /// </summary>
            private int currentDir;

            /// <summary>
            /// Direction of the <c>nextEdge</c>
            /// </summary>
            private int nextDir;

            /// <summary>
            /// Current Edge
            /// </summary>
            private BlossomVEdge? currentEdge;

            /// <summary>
            /// The edge that will be returned after the next call to <see cref="MoveNext"/>.
            /// Is null if all incident edges of the current node have been traversed.
            /// </summary>
            private BlossomVEdge? nextEdge;

            private readonly BlossomVEdge[] first;


            /// <summary>
            /// Constructs a new instance of the IncidentEdgeIterator.
            /// </summary>
            /// <param name="first"></param>
            public IncidentEdgeEnumerator(BlossomVEdge[] first)
            {
                this.first = first;
                nextDir = first[0] == null ? 1 : 0;
                nextEdge = first[nextDir];
            }


            /// <summary>
            /// Returns the direction of the edge returned by this iterator
            /// </summary>
            /// <returns>the direction of the edge returned by this iterator</returns>
            public int GetDir()
            {
                return currentDir;
            }

            public bool MoveNext()
            {
                currentEdge = nextEdge;
                Advance();

                return currentEdge != null;
            }

            /// <summary>
            /// Advances this iterator to the next incident edge. If previous edge was the last one with
            /// direction 0, then the direction of this iterator changes. If previous edge was the last
            /// incident edge, then <c>currentEdge</c> becomes null.
            /// </summary>
            private void Advance()
            {

                if (nextEdge == null)
                {
                    return;
                }

                currentDir = nextDir;
                nextEdge = nextEdge.next[nextDir];

                if (nextEdge == first[0])
                {
                    nextEdge = first[1];
                    nextDir = 1;
                } else if (nextEdge == first[1])
                {
                    nextEdge = null;
                }
            }


            public void Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose() { GC.SuppressFinalize(this); }

            public BlossomVEdge Current
            {
                get
                {
                    return currentEdge ?? new BlossomVEdge(-1);
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
