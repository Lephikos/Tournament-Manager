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

        /// <summary>
        /// Variable for debug purposes
        /// </summary>
        private static int currentId = 1;


        BlossomVTreeEdge[] first;



        internal double eps;
    }
}
