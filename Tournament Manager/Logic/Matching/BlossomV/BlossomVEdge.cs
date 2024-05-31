using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// Position of this edge in the array <see cref="BlossomVState{V, E}.edges"/>. This helps to determine generic
        /// counterpart of this edge in constant time.
        /// </summary>
        readonly int pos;

        /// <summary>
        /// The slack of this edge. If an edge is an outer edge and doesn't connect 2 infinity nodes,
        /// then its slack is subject to lazy delta spreading technique. Otherwise, this variable equals
        /// the edge's true slack.<para/>
        /// 
        /// The true slack of the edge can be computed as following: for each of its two current
        /// endpoints $\{u, v\}$ we subtract the endpoint.tree.eps if the endpoint is a "+" outer node or
        /// add this value if it is a "-" outer node. After that we have valid slack for this edge.
        /// </summary>
        readonly double slack;

        internal BlossomVEdge[] next;

        internal BlossomVEdge[] prev;

        internal BlossomVNode[] head;

        public BlossomVNode GetOpposite(BlossomVNode node)
        {
            return new BlossomVNode(0);
        }

    }
}
