using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// This class is a data structure for Kolmogorov's Blossom V algorithm.<para/>
    /// 
    /// It represents an edge between two nodes. Even though the weighted perfect matching problem is
    /// formulated on an undirected graph, each edge has direction, i.e. it is an arc. According to this
    /// direction it is present in two circular doubly linked lists of incident edges. The references to
    /// the next and previous edges of this list are maintained via <see cref="next"/> and
    /// <see cref="prev"/> references. The direction of an edge isn't stored in the edge, this
    /// property is only reflected by the presence of an edge in the list of outgoing or incoming edges.<para/>
    /// 
    /// For example, let a $e = \{u, v\}$ be an edge in the graph $G = (V, E)$. Let's assume that after
    /// initialization this edge has become directed from $u$ to $v$, i.e. now $e = (u, v)$. Then edge
    /// $e$ belongs to the linked lists <c>u.first[0]</c> and <c>v.first[1]</c>. In other words, $e$ is
    /// an outgoing edge of $u$ and an incoming edge of $v$. For convenience during computation,
    /// <c>e.head[0] = v</c> and <c>e.head[1] = u</c>. Therefore, while iterating over incident edges
    /// of a node <c>x</c> in the direction <c>dir</c>>, we can easily access opposite node by
    /// <c>x.head[dir]</c>.<para/>
    /// 
    /// An edge is called an <i>infinity</i> edge if it connects a "+" node with an infinity node. An
    /// edge is called <i>free</i> if it connects two infinity nodes. An edge is called <i>matched</i> if
    /// it belongs to the matching. During the shrink or expand operations an edge is called an
    /// <i>inner</i> edge if it connects two nodes of the blossom. It is called a <i>boundary</i> edge if
    /// it is incident to exactly one blossom node. An edge is called <i>tight</i> if its reduced cost
    /// (reduced weight, slack, all three notions are equivalent) is zero. <b>Note:</b> in this algorithm
    /// we use lazy delta spreading, so the <see cref="slack"/> isn't necessarily equal to the
    /// actual slack of an edge.
    /// </summary>
    internal class BlossomVEdge
    {

        #region member

        /// <summary>
        /// Position of this edge in the array <see cref="BlossomVState{V, E}.edges"/>. This helps to determine generic
        /// counterpart of this edge in constant time.
        /// </summary>
        internal int pos;

        /// <summary>
        /// The slack of this edge. If an edge is an outer edge and doesn't connect 2 infinity nodes,
        /// then its slack is subject to lazy delta spreading technique. Otherwise, this variable equals
        /// the edge's true slack.<para/>
        /// 
        /// The true slack of the edge can be computed as following: for each of its two current
        /// endpoints $\{u, v\}$ we subtract the endpoint.tree.eps if the endpoint is a "+" outer node or
        /// add this value if it is a "-" outer node. After that we have valid slack for this edge.
        /// </summary>
        internal double slack;

        /// <summary>
        /// A two-element array of original endpoints of this edge. They are used to quickly determine
        /// original endpoints of an edge and compute the penultimate blossom. This is done while one of
        /// the current endpoints of this edge is being shrunk or expanded.<para/>
        /// 
        /// These values stay unchanged throughout the course of the algorithm.
        /// </summary>
        internal BlossomVNode[] headOriginal;

        /// <summary>
        /// A two-element array of current endpoints of this edge. These values change when previous
        /// endpoints are contracted into blossoms or are expanded. For node head[0] this is an incoming
        /// edge (direction 1) and for the node head[1] this is an outgoing edge (direction 0). This
        /// feature is used to be able to access the opposite node via an edge by
        /// <c>incidentEdgeIterator.next().head[incidentEdgeIterator.getDir()]</c>
        /// </summary>
        internal BlossomVNode[] head;

        /// <summary>
        /// A two-element array of references to the previous elements in the circular doubly linked
        /// lists of edges. Each list belongs to one of the <b>current</b> endpoints of this edge.
        /// </summary>
        internal BlossomVEdge[] prev;

        /// <summary>
        /// A two-element array of references to the next elements in the circular doubly linked lists of
        /// edges. Each list belongs to one of the <b>current</b> endpoints of this edge.
        /// </summary>
        internal BlossomVEdge[] next;

        #endregion member

        #region constructor

        /// <summary>
        /// Constructs a new edge by initializing the arrays
        /// </summary>
        /// <param name="pos"></param>
        public BlossomVEdge(int pos)
        {
            headOriginal = new BlossomVNode[2];
            head = new BlossomVNode[2];
            next = new BlossomVEdge[2];
            prev = new BlossomVEdge[2];
            this.pos = pos;
        }

        #endregion constructor

        #region public methods

        /// <summary>
        /// Returns the opposite node with respect to the <c>endpoint</c>. <b>Note:</b> here we assume
        /// that <c>endpoint</c> is one of the current endpoints.
        /// </summary>
        /// <param name="node">one of the current endpoints of this edge</param>
        /// <returns>opposite to the <c>endpoint</c></returns>
        public BlossomVNode? GetOpposite(BlossomVNode endpoint)
        {
            if (endpoint != head[0] && endpoint != head[1]) //needed during finishing phase
            {
                return null;
            }
            return head[0] == endpoint ? head[1] : head[0];
        }

        /// <summary>
        /// Returns the original endpoint of this edge for some current <c>endpoint</c>.
        /// </summary>
        /// <param name="endpoint">one of the current endpoints of this edge</param>
        /// <returns>the original endpoint of this edge which has the same direction as
        ///          <c>endpoint</c> with respect to this edge</returns>
        public BlossomVNode? GetCurrentOriginal(BlossomVNode endpoint)
        {
            if (endpoint != head[0] && endpoint != head[1]) ////needed during finishing phase
            {
                return null;
            }
            return head[0] == endpoint ? headOriginal[0] : headOriginal[1];
        }

        /// <summary>
        /// Returns the direction to the opposite node with respect to the <c>current</c>.
        /// <c>current</c> must be one of the current endpoints of this edge.
        /// </summary>
        /// <param name="current">one of the current endpoints of this edge.</param>
        /// <returns>the direction from the <c>current</c></returns>
        public int GetDirFrom(BlossomVNode current)
        {
            return head[0] == current ? 1 : 0;
        }

        public override string ToString()
        {
            return "BlossomVEdge (" + head[0].pos + "," + head[1].pos + "), original: ["
            + headOriginal[0].pos + "," + headOriginal[1].pos + "], slack: " + slack
            + ", true slack: " + GetTrueSlack() + (GetTrueSlack() == 0 ? ", tight" : "");
        }

        /// <summary>
        /// Returns the true slack of this edge, i.e. the slack after applying lazy dual updates
        /// </summary>
        /// <returns>the true slack of this edge</returns>
        public double GetTrueSlack()
        {
            double result = slack;

            if (head[0].tree != null)
            {
                if (head[0].IsPlusNode())
                {
                    result -= head[0].tree!.eps;
                } else
                {
                    result += head[0].tree!.eps;
                }
            }

            if (head[1].tree != null)
            {
                if (head[1].IsPlusNode())
                {
                    result -= head[1].tree!.eps;
                }
                else
                {
                    result += head[1].tree!.eps;
                }
            }

            return result;
        }

        /// <summary>
        /// Moves the tail of the <c>edge</c> from the node <c>from</c> to the node <c>to</c>
        /// </summary>
        /// <param name="from">the previous edge's tail</param>
        /// <param name="to">the new edge's tail</param>
        public void MoveEdgeTail(BlossomVNode from, BlossomVNode to)
        {
            int dir = GetDirFrom(from);
            from.RemoveEdge(this, dir);
            to.AddEdge(this, dir);
        }

        /// <summary>
        /// Returns a new instance of blossom nodes iterator
        /// </summary>
        /// <param name="root">the root of the blossom</param>
        /// <returns>a new instance of blossom nodes iterator</returns>
        public BlossomNodesEnumerator GetBlossomNodesEnumerator(BlossomVNode root)
        {
            return new BlossomNodesEnumerator(root, this);
        }

        #endregion public methods

        #region classes

        /// <summary>
        /// An enumerator which traverses all nodes in the blossom. It starts from the endpoints of the
        /// (+,+) edge and goes up to the blossom root. These two paths to the blossom root are called
        /// branches. The branch of the blossomFormingEdge.head[0] has direction 0, the other one has direction
        /// 1<para/>
        /// 
        /// <b>Note:</b> the nodes returned by this iterator aren't consecutive<para/>
        /// 
        /// <b>Note:</b> this iterator must return the blossom root in the first branch, i.e. when the
        /// direction is 0. This feature is needed to setup the blossomSibling references correctly
        /// </summary>
        public class BlossomNodesEnumerator : IEnumerator<BlossomVNode>
        {

            /// <summary>
            /// Blossom's root
            /// </summary>
            private readonly BlossomVNode root;

            /// <summary>
            /// The node this iterator is currently on
            /// </summary>
            private BlossomVNode? currentNode;

            /// <summary>
            /// The current direction of this iterator
            /// </summary>
            private int currentDirection;

            /// <summary>
            /// The (+, +) edge of the blossom
            /// </summary>
            private readonly BlossomVEdge blossomFormingEdge;

            private bool initialized = false;


            /// <summary>
            /// Constructs a new BlossomNodeIterator for the <c>root</c> and <c>blossomFormingEdge</c>
            /// </summary>
            /// <param name="root">the root of the blossom (the node which isn't matched to another node in the blossom)</param>
            /// <param name="blossomFormingEdge">a (+, +) edge in the blossom</param>
            public BlossomNodesEnumerator(BlossomVNode root, BlossomVEdge blossomFormingEdge)
            {
                this.root = root;
                this.blossomFormingEdge = blossomFormingEdge;
                currentNode = blossomFormingEdge.head[0];
                currentDirection = 0;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <returns>the current direction of this iterator</returns>
            public int GetCurrentDirection()
            {
                return currentDirection;
            }

            public bool MoveNext()
            {
                if (!initialized)
                {
                    initialized = true;
                    return true;
                }

                Advance();

                return currentNode != null;
            }

            /// <summary>
            /// Advances this iterator to the next node in the blossom
            /// </summary>
            /// <returns>an unvisited node in the blossom</returns>
            private void Advance()
            {

                if (currentNode == null)
                {
                    return;
                }

                if (currentNode == root && currentDirection == 0)
                {
                    // we have just traversed blossom's root and now start to traverse the second branch
                    currentDirection = 1;
                    currentNode = blossomFormingEdge.head[1];
                    if (currentNode == root)
                    {
                        currentNode = null;
                    }
                } else if (currentNode.GetTreeParent() == root && currentDirection == 1)
                {
                    // we have just finished traversing the blossom's nodes
                    currentNode = null;
                } else
                {
                    currentNode = currentNode.GetTreeParent();
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

        #endregion classes

    }
}
