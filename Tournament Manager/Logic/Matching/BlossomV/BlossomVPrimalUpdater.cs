using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Heap;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// This class is used by <see cref="KolmogorovWeightedPerfectMatching{V, E}"/> for performing primal operations:
    /// grow, augment, shrink and expand. This class operates on alternating trees, blossom structures,
    /// and node states. It changes them after applying any primal operation. Also, this class can add
    /// and subtract some values from nodes' dual variables; it never changes their actual dual
    /// variables.<para/>
    /// 
    /// The augment operation is used to increase the cardinality of the matching. It is applied to a
    /// tight (+, +) cross-tree edge. Its main purpose is to alter the matching on the simple path
    /// between tree roots through the tight edge, destroy the previous tree structures, update the state
    /// of the node, and change the presence of edges in the priority queues. This operation doesn't
    /// destroy the tree structure; this technique is called <i>lazy tree structure destroying</i>. The
    /// information of the nodes from the tree structure block is overwritten when a node is being added
    /// to another tree. This operation doesn't change the matching in the contracted blossoms.<para/>
    /// 
    /// The grow operation is used to add new nodes to a given tree. This operation is applied only to
    /// tight infinity edges. It always adds even number of nodes. This operation can grow the tree
    /// recursively in the depth-first order. If it encounters a tight (+, +) cross-tree edge, it stops
    /// growing and performs immediate augmentation.<para/>
    /// 
    /// The shrink operation contracts an odd node circuit and introduces a new pseudonode. It is applied
    /// to tight (+, +) in-tree edges. It changes the state so than the contracted nodes don't appear in
    /// the surface graph. If during the changing of the endpoints of boundary edge a tight (+, +)
    /// cross-tree edge is encountered, an immediate augmentation is performed.<para/>
    /// 
    /// The expand operation brings the contracted blossom nodes to the surface graph. It is applied only
    /// to a "-" blossom with zero dual variable. The operation determines the two branches of a blossom:
    /// an even and an odd one. The former contains an even number of edges and can be empty, the latter
    /// contains an odd number of edges and necessarily contains at least one edge. An even branch is
    /// inserted into the tree. The state of the algorithm is changed respectively (node duals, tree
    /// structure, etc.). If some boundary edge in a tight (+, +) cross-tree edge, an immediate
    /// augmentation is performed.<para/>
    /// 
    /// The immediate augmentations are used to speed up the algorithm. More detailed description of the
    /// primal operations can be found in their summary
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal class BlossomVPrimalUpdater<V, E> where V : notnull where E : notnull
    {

        private readonly static bool DEBUG = KolmogorovWeightedPerfectMatching<V, E>.DEBUG;

        /// <summary>
        /// State information needed for the algorithm
        /// </summary>
        private readonly BlossomVState<V, E> state;

        /// <summary>
        /// Constructs a new instance of BlossomVPrimalUpdater
        /// </summary>
        /// <param name="state">contains the graph and associated information</param>
        public BlossomVPrimalUpdater(BlossomVState<V, E> state)
        {
            this.state = state;
        }

        #region public methods

        /// <summary>
        /// Performs grow operation. This is invoked on the plus-infinity <c>growEdge</c>, which
        /// connects a "+" node in the tree and an infinity matched node. The <c>growEdge</c> and the
        /// matched free edge are added to the tree structure. Two new nodes are added to the tree: minus
        /// node and plus node. Let's call the node incident to the <c>growEdge</c> and opposite to the
        /// minusNode the "tree node".<para/>
        /// 
        /// As the result, following actions are performed:
        /// <list type="bullet">
        /// <item><description>
        /// Add new child to the children of tree node and minus node
        /// </description></item>
        /// <item><description>
        /// Set parent edges of minus and plus nodes
        /// </description></item>
        /// <item><description>
        /// If minus node is a blossom, add it to the heap of "-" blossoms
        /// </description></item>
        /// <item><description>
        /// Remove growEdge from the heap of infinity edges
        /// </description></item>
        /// <item><description>
        /// Remove former infinity edges and add new (+, +) in-tree and cross-tree edges, (+, -)
        /// cross tree edges to the appropriate heaps (due to the changes of the labels of the minus and
        /// plus nodes
        /// </description></item>
        /// <item><description>
        /// Add new infinity edge from the plus node
        /// </description></item>
        /// <item><description>
        /// Add new tree edges if necessary
        /// </description></item>
        /// <item><description>
        /// Subtract tree.eps from the slacks of all edges incident to the minus node
        /// </description></item>
        /// <item><description>
        /// Add tree.eps to the slacks of all edges incident to the plus node
        /// </description></item>
        /// </list>
        /// 
        /// If the <c>manyGrows</c> flag is true, performs recursive growing of the tree.
        /// </summary>
        /// <param name="growEdge">the tight edge between node in the tree and minus node</param>
        /// <param name="recursiveGrow">specifies whether to perform recursive growing</param>
        /// <param name="immediateAugment">a flag that indicates whether to perform immediate augmentation if a
        ///                                tight (+, +) cross-tree edge is encountered</param>
        public void Grow(BlossomVEdge growEdge, bool recursiveGrow, bool immediateAugment)
        {
            if (DEBUG)
            {
                Console.WriteLine("Growing edge " + growEdge);
            }

            long start = GetNanoTime();

            int initialTreeNum = state.treeNum;
            int dirToMinusNode = growEdge.head[0].IsInfinityNode() ? 0 : 1;

            BlossomVNode nodeInTheTree = growEdge.head[1 - dirToMinusNode];
            BlossomVNode minusNode = growEdge.head[dirToMinusNode];
            BlossomVNode plusNode = minusNode.GetOppositeMatched()!;

            nodeInTheTree.AddChild(minusNode, growEdge, true);
            minusNode.AddChild(plusNode, minusNode.matched!, true);

            BlossomVNode stop = plusNode;

            while (true)
            {
                minusNode.label = BlossomVNode.Label.MINUS;
                plusNode.label = BlossomVNode.Label.PLUS;

                minusNode.isMarked = plusNode.isMarked = false;
                ProcessMinusNodeGrow(minusNode);
                ProcessPlusNodeGrow(plusNode, recursiveGrow, immediateAugment);

                if (initialTreeNum != state.treeNum)
                {
                    break;
                }

                if (plusNode.firstTreeChild != null)
                {
                    minusNode = plusNode.firstTreeChild;
                    plusNode = minusNode.GetOppositeMatched()!;
                } else
                {
                    while (plusNode != stop && plusNode.treeSiblingNext == null)
                    {
                        plusNode = plusNode.GetTreeParent()!;
                    }

                    if (plusNode.IsMinusNode())
                    {
                        minusNode = plusNode.treeSiblingNext!;
                        plusNode = minusNode.GetOppositeMatched()!;
                    } else
                    {
                        break;
                    }
                }
            }

            state.statistics.growTime += GetNanoTime() - start;
        }

        /// <summary>
        /// Performs augment operation. This is invoked on a tight (+, +) cross-tree edge. It increases
        /// the matching by 1, converts the trees on both sides into the set of free matched edges, and
        /// applies lazy delta spreading.<para/>
        /// 
        /// For each tree the following actions are performed:
        /// <list type="bullet">
        /// <item><description>
        /// Labels of all nodes change to INFINITY
        /// </description></item>
        /// <item><description>
        /// tree.eps is subtracted from "-" nodes' duals and added to the "+" nodes' duals
        /// </description></item>
        /// <item><description>
        /// tree.eps is subtracted from all edges incident to "+" nodes and added to all edges
        /// incident to "-" nodes. Consecutively, the slacks of the (+, -) in-tree edges stay
        /// unchanged
        /// </description></item>
        /// <item><description>
        /// Former (-, +) and (+, +) are substituted with the (+, inf) edges (removed and added to
        /// appropriate heaps).
        /// </description></item>
        /// <item><description>
        /// The cardinality of the matching is increased by 1
        /// </description></item>
        /// <item><description>
        /// Tree structure references are set to null
        /// </description></item>
        /// <item><description>
        /// Tree roots are removed from the linked list of tree roots
        /// </description></item>
        /// </list>
        /// 
        /// These actions change only the surface graph. They don't change the nodes and edges in the
        /// pseudonodes.
        /// </summary>
        /// <param name="augmentEdge">the edge to augment</param>
        public void Augment(BlossomVEdge augmentEdge)
        {
            if (DEBUG)
            {
                Console.WriteLine("Augmenting edge " + augmentEdge);
            }
            long start = GetNanoTime();

            // augment trees on both sides
            for (int dir = 0; dir < 2; dir++)
            {
                BlossomVNode node = augmentEdge.head[dir];
                AugmentBranch(node, augmentEdge);
                node.matched = augmentEdge;
            }

            state.statistics.augmentTime += GetNanoTime() - start;
        }

        /// <summary>
        /// Performs shrink operation. This is invoked on a tight (+, +) in-tree edge. The result of this
        /// operation is the substitution of an odd circuit with a single node. This means that we
        /// consider the set of nodes of odd cardinality as a single node.<para/>
        /// 
        /// In the shrink operation the following main actions are performed:
        /// <list type="bullet">
        /// <item><description>
        /// Lazy dual updates are applied to all inner edges and nodes on the circuit. Thus, the
        /// inner edges and nodes in the pseudonodes have valid slacks and dual variables
        /// </description></item>
        /// <item><description>
        /// The endpoints of the boundary edges are moved to the new blossom node, which has label
        /// <see cref="BlossomVNode.Label"/>#PLUS
        /// </description></item>
        /// <item><description>
        /// Lazy dual updates are applied to boundary edges and newly created blossom
        /// </description></item>
        /// <item><description>
        /// Children of blossom nodes are moved to the blossom, their parent edges are changed
        /// respectively
        /// </description></item>
        /// <item><description>
        /// The blossomSibling references are set so that they form a circular linked list
        /// </description></item>
        /// <item><description>
        /// If the blossom becomes a tree root, it substitutes the previous tree's root in the linked
        /// list of tree roots
        /// </description></item>
        /// <item><description>
        /// Since the newly created blossom with "+" label can change the classification of edges,
        /// their presence in heaps is updated
        /// </description></item>
        /// </list>
        /// </summary>
        /// <param name="blossomFormingEdge">the tight (+, +) in-tree edge</param>
        /// <param name="immediateAugment">a flag that indicates whether to perform immediate augmentation if a
        ///                                tight (+, +) cross-tree edge is encountered</param>
        /// <returns>the newly created blossom</returns>
        public BlossomVNode Shrink(BlossomVEdge blossomFormingEdge, bool immediateAugment)
        {
            if (DEBUG)
            {
                Console.WriteLine("Shrinking edge " + blossomFormingEdge);
            }
            long start = GetNanoTime();

            BlossomVNode blossomRoot = FindBlossomRoot(blossomFormingEdge);
            BlossomVTree tree = blossomRoot.tree!;

            //We don't actually need position of the blossom node since blossom nodes aren't stored in
            //the state.nodes array. We use blossom's position as its id for debug purposes.
            BlossomVNode blossom = new BlossomVNode(state.nodeNum + state.blossomNum);

            //initialize blossom node
            blossom.tree = tree;
            blossom.isBlossom = true;
            blossom.isOuter = true;
            blossom.isTreeRoot = blossomRoot.isTreeRoot;
            blossom.dual = -tree.eps;
            if (blossom.isTreeRoot)
            {
                tree.root = blossom;
            }
            else
            {
                blossom.matched = blossomRoot.matched;
            }

            //mark all blossom nodes
            for (BlossomVEdge.BlossomNodesEnumerator enumerator = blossomFormingEdge.GetBlossomNodesEnumerator(blossomRoot);  enumerator.MoveNext(); )
            {
                enumerator.Current.isMarked = true;
            }

            // move edges and children, change slacks if necessary
            BlossomVEdge? augmentEdge = UpdateTreeStructure(blossomRoot, blossomFormingEdge, blossom);

            // create circular linked list of circuit nodes
            SetBlossomSiblings(blossomRoot, blossomFormingEdge);

            // reset marks of blossom nodes
            blossomRoot.isMarked = false;
            blossomRoot.isProcessed = false;
            for (BlossomVNode current = blossomRoot.blossomSibling!.GetOpposite(blossomRoot)!;
                current != blossomRoot; current = current.blossomSibling!.GetOpposite(current)!)
            {
                current.isMarked = false;
                current.isProcessed = false;
            }
            blossomRoot.matched = null; // now new blossom is matched (used when finishing the matching

            //update statistics and augment if possible
            state.statistics.shrinkNum++;
            state.blossomNum++;

            state.statistics.shrinkTime += GetNanoTime() - start;

            if (augmentEdge != null && immediateAugment)
            {
                if (DEBUG)
                {
                    Console.WriteLine("Bingo shrink");
                }
                Augment(augmentEdge);
            }

            return blossom;
        }

        /// <summary>
        /// Performs expand operation. This is invoked on a previously contracted pseudonode. The result
        /// of this operation is bringing the nodes in the blossom to the surface graph. An even branch
        /// of the blossom is inserted into the tree structure. Endpoints of the edges incident to the
        /// blossom are moved one layer down. The slack of the inner and boundary edges are updated
        /// according to the lazy delta spreading technique.<para/>
        /// 
        /// <b>Note:</b> only "-" blossoms can be expanded. At that moment their dual variables are
        /// always zero. This is the reason why they don't need to be stored to compute the dual
        /// solution.<para/>
        /// 
        /// In the expand operation the following actions are performed:
        /// <list type="bullet">
        /// <item><description>
        /// Endpoints of the boundary edges are updated
        /// </description></item>
        /// <item><description>
        /// The matching in the blossom is changed. <b>Note:</b> the resulting matching doesn't
        /// depend on the previous matching
        /// </description></item>
        /// <item><description>
        /// isOuter flags are updated
        /// </description></item>
        /// <item><description>
        /// node.tree are updated
        /// </description></item>
        /// <item><description>
        /// Tree structure is updated including parent edges and tree children of the nodes on the
        /// even branch
        /// </description></item>
        /// <item><description>
        /// The endpoints of some edges change their labels to "+" => their slacks are changed
        /// according to the lazy delta spreading and their presence in heaps also changes
        /// </description></item>
        /// </list>
        /// </summary>
        /// <param name="blossom">the blossom to expand</param>
        /// <param name="immediateAugment">a flag that indicates whether to perform immediate augmentation if a
        ///                                tight (+, +) cross-tree edge is encountered</param>
        public void Expand(BlossomVNode blossom, bool immediateAugment)
        {
            if (DEBUG)
            {
                Console.WriteLine("Expanding blossom " + blossom);
            }
            long start = GetNanoTime();

            BlossomVTree tree = blossom.tree!;
            double eps = tree.eps;
            blossom.dual -= eps;
            BlossomVTree.RemoveMinusBlossom(blossom); // it doesn't belong to the tree no more

            BlossomVNode branchesEndpoint = blossom.parentEdge!.GetCurrentOriginal(blossom)!.GetPenultimateBlossom();

            if (DEBUG)
            {
                Console.WriteLine(branchesEndpoint);
            }

            // the node which is matched to the node from outside
            BlossomVNode blossomRoot = blossom.matched!.GetCurrentOriginal(blossom)!.GetPenultimateBlossom();

            // mark blossom nodes
            BlossomVNode current = blossomRoot;
            do
            {
                current.isMarked = true;
                current = current.blossomSibling!.GetOpposite(current)!;
            } while (current != blossomRoot);

            // move all edge from blossom to penultimate children
            blossom.RemoveFromChildList();
            for (BlossomVNode.IncidentEdgeEnumerator enumerator = blossom.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode penultimateChild = edge.headOriginal[1 - enumerator.GetDir()].GetPenultimateBlossomAndFixBlossomGrandparent();
                edge.MoveEdgeTail(blossom, penultimateChild);
            }

            // reverse the circular blossomSibling references so that the first branch in even branch
            if (!ForwardDirection(blossomRoot, branchesEndpoint))
            {
                ReverseBlossomSiblings(blossomRoot);
            }

            // change the matching, the labeling and the dual information on the odd branch
            ExpandOddBranch(blossomRoot, branchesEndpoint, tree);

            // change the matching, the labeling and dual information on the even branch
            BlossomVEdge? augmentEdge = ExpandEvenBranch(blossomRoot, branchesEndpoint, blossom);

            // reset marks of blossom nodes
            current = blossomRoot;
            do
            {
                current.isMarked = false;
                current.isProcessed = false;
                current = current.blossomSibling!.GetOpposite(current)!;
            } while (current != blossomRoot);

            state.statistics.expandNum++;
            state.removedNum++;
            if (DEBUG)
            {
                tree.PrintTreeNodes();
            }
            state.statistics.expandTime += GetNanoTime() - start;

            if (immediateAugment && augmentEdge != null)
            {
                if (DEBUG)
                {
                    Console.WriteLine("Bingo expand");
                }
                Augment(augmentEdge);
            }
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Processes a minus node in the grow operation. Applies lazy delta spreading, adds new (-,+)
        /// cross-tree edges, removes former (+, inf) edges.
        /// </summary>
        /// <param name="minusNode">a minus endpoint of the matched edge that is being appended to the tree</param>
        private static void ProcessMinusNodeGrow(BlossomVNode minusNode)
        {
            double eps = minusNode.tree!.eps;
            minusNode.dual += eps;

            // maintain heap of "-" blossoms
            if (minusNode.isBlossom)
            {
                minusNode.tree.AddMinusBlossom(minusNode);
            }

            // maintain minus-plus edges in the minus-plus heaps in the tree edges
            for (BlossomVNode.IncidentEdgeEnumerator enumerator = minusNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode opposite = edge.head[enumerator.GetDir()];

                edge.slack -= eps;

                if (opposite.IsPlusNode())
                {
                    if (opposite.tree != minusNode.tree)
                    {
                        // encountered (-,+) cross-tree edge
                        if (opposite.tree!.currentEdge == null)
                        {
                            BlossomVTree.AddTreeEdge(minusNode.tree, opposite.tree);
                        }
                        BlossomVTree.RemovePlusInfinityEdge(edge);
                        opposite.tree.currentEdge!.AddToCurrentMinusPlusHeap(edge, opposite.tree.currentDirection);

                    } else if (opposite != minusNode.GetOppositeMatched())
                    {
                        // encountered a former (+, inf) edge
                        BlossomVTree.RemovePlusInfinityEdge(edge);
                    }
                }
            }
        }

        /// <summary>
        /// Processes a plus node during the grow operation. Applies lazy delta spreading, removes former
        /// (+, inf) edges, adds new (+, +) in-tree and cross-tree edges, new (+, -) cross-tree edges.
        /// When the <c>manyGrows</c> flag is on, collects the tight (+, inf) edges and grows them as
        /// well.
        /// </summary>
        /// <param name="node">a plus endpoint of the matched edge that is being appended to the tree</param>
        /// <param name="recursiveGrow">a flag that indicates whether to grow the tree recursively</param>
        /// <param name="immediateAugment">a flag that indicates whether to perform immediate augmentation if a
        ///                                tight (+, +) cross-tree edge is encountered</param>
        private void ProcessPlusNodeGrow(BlossomVNode node, bool recursiveGrow, bool immediateAugment)
        {
            double eps = node.tree!.eps;
            node.dual -= eps;

            BlossomVEdge? augmentEdge = null;

            for (BlossomVNode.IncidentEdgeEnumerator enumerator = node.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode opposite = edge.head[enumerator.GetDir()];

                //maintain heap of plus-infinity edges
                edge.slack += eps;

                if (opposite.IsPlusNode())
                {
                    if (opposite.tree == node.tree)
                    {
                        //this is a (+, +) edge
                        BlossomVTree.RemovePlusInfinityEdge(edge);
                        node.tree.AddPlusPlusEdge(edge);
                    }
                    else
                    {
                        //this is a plus-plus edge to another tree
                        if (opposite.tree!.currentEdge == null)
                        {
                            BlossomVTree.AddTreeEdge(node.tree!, opposite.tree);
                        }
                        BlossomVTree.RemovePlusInfinityEdge(edge);
                        opposite.tree.currentEdge!.AddPlusPlusEdge(edge);
                        if (edge.slack <= node.tree.eps + opposite.tree.eps)
                        {
                            augmentEdge = edge;
                        }
                    }
                } else if (opposite.IsMinusNode())
                {
                    //this is a (+,-) edge
                    if (opposite.tree != node.tree)
                    {
                        //this is a (+,-) edge to another tree
                        if (opposite.tree!.currentEdge == null)
                        {
                            BlossomVTree.AddTreeEdge(node.tree, opposite.tree);
                        }
                        opposite.tree.currentEdge!.AddToCurrentPlusMinusHeap(edge, opposite.tree.currentDirection);
                    }
                } else if (opposite.IsInfinityNode())
                {
                    node.tree.AddPlusInfinityEdge(edge);

                    // this edge can be grown as well
                    // it can be the case that this edge can't be grown because opposite vertex is
                    // already added to this tree via some other grow operation
                    if (recursiveGrow && edge.slack <= eps && !edge.GetOpposite(node)!.isMarked)
                    {
                        if (DEBUG)
                        {
                            Console.WriteLine("Growing edge " + edge);
                        }

                        BlossomVNode minusNode = edge.GetOpposite(node)!;
                        BlossomVNode plusNode = minusNode.GetOppositeMatched()!;
                        minusNode.isMarked = plusNode.isMarked = true;
                        node.AddChild(minusNode, edge, true);
                        minusNode.AddChild(plusNode, minusNode.matched!, true);
                    }
                }
            }

            if (immediateAugment && augmentEdge != null)
            {
                if (DEBUG)
                {
                    Console.WriteLine("Bingo grow");
                }
                Augment(augmentEdge);
            }
            state.statistics.growNum++;
        }

        /// <summary>
        /// Expands an even branch of the blossom. Here it is assumed that the blossomSiblings are
        /// directed in the way that the even branch goes from <c>blossomRoot</c> to
        /// <c>branchesEndpoint</c>.<para/>
        /// 
        /// The method traverses the nodes twice: first it changes the tree structure, updates the
        /// labeling and flags, adds children, and changes the matching. After that it changes the slacks
        /// of the edges according to the lazy delta spreading and their presence in heaps. This
        /// operation is done in two steps because the later step requires correct labeling of the nodes
        /// on the branch.<para/>
        /// 
        /// <b>Note:</b> this branch may consist of only one node. In this case <c>blossomRoot</c> and
        /// <c>branchesEndpoint</c> are the same nodes
        /// </summary>
        /// <param name="blossomRoot">the node of the blossom which is matched from the outside</param>
        /// <param name="branchesEndpoint">the common endpoint of the even and odd branches</param>
        /// <param name="blossom">the node that is being expanded</param>
        /// <returns>a tight (+, +) cross-tree edge if it is encountered, null otherwise</returns>
        private static BlossomVEdge? ExpandEvenBranch(BlossomVNode blossomRoot, BlossomVNode branchesEndpoint, BlossomVNode blossom)
        {
            BlossomVEdge? augmentEdge = null;
            BlossomVTree tree = blossom.tree!;
            blossomRoot.matched = blossom.matched;
            blossomRoot.tree = tree;
            blossomRoot.AddChild(blossom.matched!.GetOpposite(blossomRoot)!, blossomRoot.matched!, false);

            BlossomVNode current = blossomRoot;
            BlossomVNode prevNode = current;
            current.label = BlossomVNode.Label.MINUS;
            current.isOuter = true;
            current.parentEdge = blossom.parentEdge;

            // first traversal. It is done from blossomRoot to branchesEndpoint, i.e. from higher
            // layers of the tree to the lower
            while (current != branchesEndpoint)
            {
                //process "+" node
                current = current.blossomSibling!.GetOpposite(current)!;
                current.label = BlossomVNode.Label.PLUS;
                current.isOuter = true;
                current.tree = tree;
                current.matched = current.blossomSibling;
                BlossomVEdge prevMatched = current.blossomSibling!;
                current.AddChild(prevNode, prevNode.blossomSibling!, false);
                prevNode = current;

                //process "-" node
                current = current.blossomSibling!.GetOpposite(current)!;
                current.label = BlossomVNode.Label.MINUS;
                current.isOuter = true;
                current.tree = tree;
                current.matched = prevMatched;
                current.AddChild(prevNode, prevNode.blossomSibling!, false);
                prevNode = current;
            }
            blossom.parentEdge!.GetOpposite(branchesEndpoint)!.AddChild(branchesEndpoint, blossom.parentEdge, false);

            // second traversal, update edge slacks and their presence in heaps
            current = blossomRoot;
            ExpandMinusNode(current);
            while (current != branchesEndpoint)
            {
                current = current.blossomSibling!.GetOpposite(current)!;
                BlossomVEdge? edge = ExpandPlusNode(current);
                if (edge != null)
                {
                    augmentEdge = edge;
                }
                current.isProcessed = true; // this is needed for correct processing of (+, +) edges
                                            // connecting two node on the branch

                current = current.blossomSibling!.GetOpposite(current)!;
                ExpandMinusNode(current);
            }

            return augmentEdge;
        }

        /// <summary>
        /// Expands the nodes on an odd branch. Here it is assumed that the blossomSiblings are directed
        /// in the way the odd branch goes from <c>branchesEndpoint</c> to <c>blossomRoot</c>.<para/>
        /// 
        /// The method traverses the nodes only once setting the labels, flags, updating the matching,
        /// removing former (+, -) edges and creating new (+, inf) edges in the corresponding heaps. The
        /// method doesn't process the <c>blossomRoot</c> and <c>branchesEndpoint</c> as they belong to
        /// the even branch.
        /// </summary>
        /// <param name="blossomRoot">the node that is matched from the outside</param>
        /// <param name="branchesEndpoint">the common node of the even and odd branches</param>
        /// <param name="tree">the tree the blossom was previously in</param>
        private static void ExpandOddBranch(BlossomVNode blossomRoot, BlossomVNode branchesEndpoint, BlossomVTree tree)
        {
            BlossomVNode current = branchesEndpoint.blossomSibling!.GetOpposite(branchesEndpoint)!;

            // the traversal is done from branchesEndpoint to blossomRoot, i.e. from
            // lower layers to higher

            while (current != blossomRoot)
            {
                current.label = BlossomVNode.Label.INFINITY;
                current.isOuter = true;
                current.tree = null;
                current.matched = current.blossomSibling;
                BlossomVEdge prevMatched = current.blossomSibling!;
                ExpandInfinityNode(current, tree);
                current = current.blossomSibling!.GetOpposite(current)!;

                current.label = BlossomVNode.Label.INFINITY;
                current.isOuter = true;
                current.tree = null;
                current.matched = prevMatched;
                ExpandInfinityNode(current, tree);
                current = current.blossomSibling!.GetOpposite(current)!;
            }
        }

        /// <summary>
        /// Changes dual information of the {@code plusNode} and edge incident to it. This method relies
        /// on the labeling produced by the first traversal of the <see cref="ExpandEvenBranch(BlossomVNode, BlossomVNode, BlossomVNode)"/>
        /// and on the isProcessed flags of the nodes on the even branch that have been traversed already. It
        /// also assumes that all blossom nodes are marked.<para/>
        /// 
        /// Since one of endpoints of the edges previously incident to the blossom changes its label, we
        /// have to update the slacks of the boundary edges incindent to the <c>plusNode</c>.
        /// </summary>
        /// <param name="plusNode">the "+" node from the even branch</param>
        /// <returns>a tight (+, +) cross-tree edge if it is encountered, null otherwise</returns>
        private static BlossomVEdge? ExpandPlusNode(BlossomVNode plusNode)
        {
            BlossomVEdge? augmentEdge = null;

            double eps = plusNode.tree!.eps; //the plusNode.tree is assumed to be correct
            plusNode.dual -= eps; // apply lazy delta spreading

            for (BlossomVNode.IncidentEdgeEnumerator enumerator = plusNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext();)
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode opposite = edge.head[enumerator.GetDir()];

                //update slack of the edge
                if (opposite.isMarked && opposite.IsPlusNode())
                {
                    //this is an inner (+,+) edge
                    if (!opposite.isProcessed)
                    {
                        //we encounter this edge for the first time
                        edge.slack += 2 * eps;
                    }
                } else if (!opposite.isMarked)
                {
                    //this is a boundary edge
                    edge.slack += 2 * eps; //the endpoint changes it's label to "+"
                } else if (!opposite.IsMinusNode())
                {
                    // this edge is inner edge between even and odd branches or it is an inner (+, +)
                    // edge
                    edge.slack += eps;
                }

                // update its presence in the heap of edges
                if (opposite.IsPlusNode())
                {
                    if (opposite.tree == plusNode.tree)
                    {
                        //this edge becomes a (+,+) in-tree edge
                        if (!opposite.isProcessed)
                        {
                            // if opposite.isProcessed = true => this is an inner (+, +) edge => its
                            // slack has been updated already and it has been added to the plus-plus edges heap already
                            plusNode.tree.AddPlusPlusEdge(edge);
                        }
                    } else
                    {
                        // opposite is from another tree since it's label is "+"
                        BlossomVTreeEdge.RemoveFromCurrentMinusPlusHeap(edge);
                        opposite.tree!.currentEdge!.AddPlusPlusEdge(edge);
                        if (edge.slack <= eps + opposite.tree.eps)
                        {
                            augmentEdge = edge;
                        }
                    }
                } else if (opposite.IsMinusNode())
                {
                    if (opposite.tree != plusNode.tree)
                    {
                        // this edge becomes a (+, -) cross-tree edge
                        if (opposite.tree!.currentEdge == null)
                        {
                            BlossomVTree.AddTreeEdge(plusNode.tree, opposite.tree);
                        }
                        opposite.tree.currentEdge!.AddToCurrentPlusMinusHeap(edge, opposite.tree.currentDirection);
                    }
                } else
                {
                    // this is either an inner edge, that becomes a (+, inf) edge, or it is a former (-,+) edge,
                    //that also becomes a (+, inf) edge
                    plusNode.tree.AddPlusInfinityEdge(edge); //updating edge's key
                }
            }

            return augmentEdge;
        }

        /// <summary>
        /// Expands a minus node from the odd branch. Changes the slacks of inner (-,-) and (-, inf)
        /// edges.
        /// </summary>
        /// <param name="minusNode">a "-" node from the even branch</param>
        private static void ExpandMinusNode(BlossomVNode minusNode)
        {
            double eps = minusNode.tree!.eps; //the minusNode.tree is assumed to be correct
            minusNode.dual += eps;

            if (minusNode.isBlossom)
            {
                minusNode.tree.AddMinusBlossom(minusNode);
            }

            for (BlossomVNode.IncidentEdgeEnumerator enumerator = minusNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode opposite = edge.head[enumerator.GetDir()];

                if (opposite.isMarked && !opposite.IsPlusNode())
                {
                    // this is a (-, inf) or (-, -) inner edge
                    edge.slack -= eps;
                }
            }
        }

        /// <summary>
        /// Expands an infinity node from the odd branch
        /// </summary>
        /// <param name="infinityNode">a node from the odd branch</param>
        /// <param name="tree">the tree the blossom was previously in</param>
        private static void ExpandInfinityNode(BlossomVNode infinityNode, BlossomVTree tree)
        {
            double eps = tree.eps;

            for (BlossomVNode.IncidentEdgeEnumerator enumerator = infinityNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode opposite = edge.head[enumerator.GetDir()];

                if (!opposite.isMarked)
                {
                    // if this node is marked => it's a blossom node => this edge has been processed
                    // already

                    edge.slack += eps; // since edge's label changes to inf and this is a boundary edge

                    if (opposite.IsPlusNode())
                    {
                        if (opposite.tree != tree)
                        {
                            BlossomVTreeEdge.RemoveFromCurrentMinusPlusHeap(edge);
                        }
                        opposite.tree!.AddPlusInfinityEdge(edge);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a tree into a set of free matched edges. Changes the matching starting from
        /// <c>firstNode</c> all the way up to the firstNode.tree.root. It changes the labeling of the
        /// nodes, applies lazy delta spreading, updates edges' presence in the heaps. This method also
        /// deletes unnecessary tree edges.<para/>
        /// 
        /// This method doesn't change the nodes and edge contracted in the blossoms.
        /// </summary>
        /// <param name="firstNode">an endpoint of the <c>augmentEdge</c> which belongs to the tree to augment</param>
        /// <param name="augmentEdge">a tight (+, +) cross tree edge</param>
        private void AugmentBranch(BlossomVNode firstNode, BlossomVEdge augmentEdge)
        {
            BlossomVTree tree = firstNode.tree!;
            double eps = tree.eps;
            BlossomVNode root = tree.root;

            // set currentEdge and currentDirection of all opposite trees connected via treeEdge
            tree.SetCurrentEdges();

            // apply tree.eps to all tree nodes and updating slacks of all incident edges
            for (BlossomVTree.TreeNodeEnumerator treeNodeEnumerator = tree.GetTreeNodeEnumerator(); treeNodeEnumerator.MoveNext(); )
            {
                BlossomVNode node = treeNodeEnumerator.Current;

                if (!node.isMarked)
                {
                    //apply lazy delta spreading
                    if (node.IsPlusNode())
                    {
                        node.dual += eps;
                    } else
                    {
                        node.dual -= eps;
                    }

                    for (BlossomVNode.IncidentEdgeEnumerator incidentEdgeEnumerator = node.GetIncidentEdgeEnumerator(); 
                        incidentEdgeEnumerator.MoveNext(); )
                    {
                        BlossomVEdge edge = incidentEdgeEnumerator.Current;
                        int dir = incidentEdgeEnumerator.GetDir();
                        BlossomVNode opposite = edge.head[dir];
                        BlossomVTree? oppositeTree = opposite.tree;

                        if (node.IsPlusNode())
                        {
                            edge.slack -= eps;
                            if (oppositeTree != null && oppositeTree != tree)
                            {
                                // if this edge is a cross-tree edge
                                BlossomVTreeEdge treeEdge = oppositeTree.currentEdge!;
                                if (opposite.IsPlusNode())
                                {
                                    //this is a (+,+) cross-tree edge
                                    BlossomVTreeEdge.RemoveFromPlusPlusHeap(edge);
                                    oppositeTree.AddPlusInfinityEdge(edge);
                                } else if (opposite.IsMinusNode())
                                {
                                    // this is a (+,-) cross-tree edge
                                    BlossomVTreeEdge.RemoveFromCurrentPlusMinusHeap(edge);
                                }
                            }
                        } else
                        {
                            // current node is a "-" node
                            edge.slack += eps;
                            if (oppositeTree != null && oppositeTree != tree && opposite.IsPlusNode())
                            {
                                // this is a (-,+) cross-tree edge
                                BlossomVTreeEdge treeEdge = oppositeTree.currentEdge!;
                                BlossomVTreeEdge.RemoveFromCurrentMinusPlusHeap(edge);
                                oppositeTree.AddPlusInfinityEdge(edge);
                            }
                        }
                    }
                    node.label = BlossomVNode.Label.INFINITY;
                } else
                {
                    // this node was added to the tree by the grow operation,
                    // but it hasn't been processed, so we don't need to process it here
                    node.isMarked = false;
                }
            }

            // add all elements from the (-,+) and (+,+) heaps to (+, inf) heaps of the opposite trees
            // and delete tree edges
            for (BlossomVTree.TreeEdgeEnumerator treeEdgeEnumerator = tree.GetTreeEdgeEnumerator(); treeEdgeEnumerator.MoveNext(); )
            {
                BlossomVTreeEdge treeEdge = treeEdgeEnumerator.Current;
                int dir = treeEdgeEnumerator.GetCurrentDirection();
                BlossomVTree opposite = treeEdge.head[dir]!;
                opposite.currentEdge = null;

                opposite.plusPlusEdges.Meld(treeEdge.plusPlusEdges);
                opposite.plusPlusEdges.Meld(treeEdge.GetCurrentMinusPlusHeap(dir));
                treeEdge.RemoveFromTreeEdgeList();
            }

            // update the matching
            BlossomVEdge? matchedEdge = augmentEdge;
            BlossomVNode plusNode = firstNode;
            BlossomVNode? minusNode = plusNode.GetTreeParent();
            while (minusNode != null)
            {
                plusNode.matched = matchedEdge;
                matchedEdge = minusNode.parentEdge;
                minusNode.matched = matchedEdge;
                plusNode = minusNode.GetTreeParent()!;
                minusNode = plusNode.GetTreeParent();
            }
            root.matched = matchedEdge;

            // remove root from the linked list of roots;
            root.RemoveFromChildList();
            root.isTreeRoot = false;

            state.treeNum--;
        }

        /// <summary>
        /// Updates the tree structure in the shrink operation. Moves the endpoints of the boundary edges
        /// to the <c>blossom</c>, moves the children of the nodes on the circuit to the blossom,
        /// updates edges's slacks and presence in heaps accordingly.
        /// </summary>
        /// <param name="blossomRoot">the node that is matched from the outside or is a tree root</param>
        /// <param name="blossomFormingEdge">a tight (+, +) edge</param>
        /// <param name="blossom">the node that is being inserted into the tree structure</param>
        /// <returns>a tight (+, +) cross-tree edge if it is encountered, null otherwise</returns>
        private static BlossomVEdge? UpdateTreeStructure(BlossomVNode blossomRoot,  BlossomVEdge blossomFormingEdge, BlossomVNode blossom)
        {
            BlossomVEdge? augmentEdge = null;
            BlossomVTree tree = blossomRoot.tree!;
            BlossomVEdge? edge;

            //Go through every vertex in the blossom and move its child list to blossom child list.
            //Handle all blossom nodes except for the blossom root. The reason is that we can't move
            //root's correctly to the blossom until both children from the circuit are removed from the
            //its children list
            for (BlossomVEdge.BlossomNodesEnumerator enumerator = blossomFormingEdge.GetBlossomNodesEnumerator(blossomRoot); enumerator.MoveNext(); )
            {
                BlossomVNode blossomNode = enumerator.Current;
                
                if (blossomNode != blossomRoot)
                {
                    if (blossomNode.IsPlusNode())
                    {
                        // substitute varNode with the blossom in the tree structure
                        blossomNode.RemoveFromChildList();
                        blossomNode.MoveChildrenTo(blossom);

                        edge = ShrinkPlusNode(blossomNode, blossom);
                        if (edge != null)
                        {
                            augmentEdge = edge;
                        }
                        blossomNode.isProcessed = true;
                    } else
                    {
                        if (blossomNode.isBlossom)
                        {
                            BlossomVTree.RemoveMinusBlossom(blossomNode);
                        }
                        blossomNode.RemoveFromChildList(); // minus node have only one child and this child belongs to the circuit
                        ShrinkMinusNode(blossomNode, blossom);
                    }
                }
            }

            // substitute varNode with the blossom in the tree structure
            blossomRoot.RemoveFromChildList();

            if (!blossomRoot.isTreeRoot)
            {
                blossomRoot.GetTreeParent()!.AddChild(blossom, blossomRoot.parentEdge!, false);
            } else
            {
                // substitute blossomRoot with blossom in the linked list of tree roots
                blossom.treeSiblingNext = blossomRoot.treeSiblingNext;
                blossom.treeSiblingPrev = blossomRoot.treeSiblingPrev;
                blossomRoot.treeSiblingPrev!.treeSiblingNext = blossom;
                if (blossomRoot.treeSiblingNext != null)
                {
                    blossomRoot.treeSiblingNext.treeSiblingPrev = blossom;
                }
            }

            // finally process blossomRoot
            blossomRoot.MoveChildrenTo(blossom);
            edge = ShrinkPlusNode(blossomRoot, blossom);
            if (edge != null)
            {
                augmentEdge = edge;
            }
            blossomRoot.isTreeRoot = false;

            return augmentEdge;
        }

        /// <summary>
        /// Processes a plus node on an odd circuit in the shrink operation. Moves endpoints of the
        /// boundary edges, updates slacks of incident edges.
        /// </summary>
        /// <param name="plusNode">a plus node from an odd circuit</param>
        /// <param name="blossom">a newly created pseudonode</param>
        /// <returns>a tight (+, +) cross-tree edge if it is encountered, null otherwise</returns>
        private static BlossomVEdge? ShrinkPlusNode(BlossomVNode plusNode, BlossomVNode blossom)
        {
            BlossomVEdge? augmentEdge = null;
            BlossomVTree tree = plusNode.tree!;
            double eps = tree.eps;
            plusNode.dual += eps;

            for (BlossomVNode.IncidentEdgeEnumerator enumerator = plusNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode opposite = edge.head[enumerator.GetDir()];

                if (!opposite.isMarked)
                {
                    // opposite isn't a node inside the blossom
                    edge.MoveEdgeTail(plusNode, blossom);
                    if (opposite.tree != tree && opposite.IsPlusNode() && edge.slack <= eps + opposite.tree!.eps)
                    {
                        augmentEdge = edge;
                    }
                } else if (opposite.IsPlusNode())
                {
                    // inner edge, subtract eps only in the case the opposite node is a "+" node
                    if (!opposite.isProcessed)
                    {
                        // here we rely on the proper setting of the isProcessed flag
                        // remove this edge when it is encountered for the first time
                        BlossomVTree.RemovePlusPlusEdge(edge);
                    }
                    edge.slack -= eps;
                }
            }

            return augmentEdge;
        }

        /// <summary>
        /// Processes a minus node from an odd circuit in the shrink operation. Moves the endpoints of
        /// the boundary edges, updates their slacks
        /// </summary>
        /// <param name="minusNode">a minus node from an odd circuit</param>
        /// <param name="blossom">a newly create pseudonode</param>
        private static void ShrinkMinusNode(BlossomVNode minusNode, BlossomVNode blossom)
        {
            BlossomVTree tree = minusNode.tree!;
            double eps = tree.eps;
            minusNode.dual -= eps;

            for (BlossomVNode.IncidentEdgeEnumerator enumerator = minusNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVEdge edge = enumerator.Current;
                BlossomVNode opposite = edge.head[enumerator.GetDir()];
                BlossomVTree oppositeTree = opposite.tree!;

                if (!opposite.isMarked)
                {
                    // opposite isn't a node inside the blossom
                    edge.MoveEdgeTail(minusNode, blossom);
                    edge.slack += 2 * eps;

                    if (opposite.tree == tree)
                    {
                        // edge to the node from the same tree, need only to add it to "++" heap if
                        // opposite is "+" node
                        if (opposite.IsPlusNode())
                        {
                            tree.AddPlusPlusEdge(edge);
                        }
                    } else
                    {
                        // cross-tree edge or infinity edge
                        if (opposite.IsPlusNode())
                        {
                            BlossomVTreeEdge.RemoveFromCurrentMinusPlusHeap(edge);
                            oppositeTree.currentEdge!.AddPlusPlusEdge(edge);

                        } else if (opposite.IsMinusNode())
                        {
                            if (oppositeTree.currentEdge == null)
                            {
                                BlossomVTree.AddTreeEdge(tree, oppositeTree);
                            }
                            oppositeTree.currentEdge!.AddToCurrentPlusMinusHeap(edge, oppositeTree.currentDirection);
                        } else
                        {
                            tree.AddPlusInfinityEdge(edge);
                        }

                    }
                }
                else if (opposite.IsMinusNode())
                {
                    // this is an inner edge
                    edge.slack += eps;
                }
            }
        }

        /// <summary>
        /// Creates a circular linked list of blossom nodes.<para/>
        /// 
        /// <b>Note:</b> this method heavily relies on the property of the
        /// <see cref="BlossomVEdge.BlossomNodesEnumerator"/> that it returns the blossomRoot while processing
        /// the first branch (with direction 0).
        /// </summary>
        /// <param name="blossomRoot">the common endpoint of two branches</param>
        /// <param name="blossomFormingEdge">a tight (+, +) in-tree edge</param>
        private static void SetBlossomSiblings(BlossomVNode blossomRoot, BlossomVEdge blossomFormingEdge)
        {
            // set blossom sibling nodes
            BlossomVEdge prevEdge = blossomFormingEdge;

            for (BlossomVEdge.BlossomNodesEnumerator enumerator = blossomFormingEdge.GetBlossomNodesEnumerator(blossomRoot); enumerator.MoveNext(); )
            {
                BlossomVNode current = enumerator.Current;

                if (enumerator.GetCurrentDirection() == 0)
                {
                    current.blossomSibling = prevEdge;
                    prevEdge = current.parentEdge!;
                }
                else
                {
                    current.blossomSibling = current.parentEdge;
                }
            }
        }

        /// <summary>
        /// Finds a blossom root of the circuit created by the <c>edge</c>. More precisely, finds an lca
        /// of edge.head[0] and edge.head[1].
        /// </summary>
        /// <param name="blossomFormingEdge">a tight (+, +) in-tree edge</param>
        /// <returns>the lca of edge.head[0] and edge.head[1]</returns>
        private static BlossomVNode FindBlossomRoot(BlossomVEdge blossomFormingEdge)
        {
            BlossomVNode root, upperBound; // need to be scoped outside of the loop
            BlossomVNode[] endPoints = new BlossomVNode[2];
            endPoints[0] = blossomFormingEdge.head[0];
            endPoints[1] = blossomFormingEdge.head[1];
            int branch = 0;
            BlossomVNode jumpNode;

            while (true)
            {
                if (endPoints[branch].isMarked)
                {
                    root = endPoints[branch];
                    upperBound = endPoints[1 - branch];
                    break;
                }
                endPoints[branch].isMarked = true;
                if (endPoints[branch].isTreeRoot)
                {
                    upperBound = endPoints[branch];
                    jumpNode = endPoints[1 - branch];
                    while (!jumpNode.isMarked)
                    {
                        jumpNode = jumpNode.GetTreeGrandparent()!;
                    }
                    root = jumpNode;
                    break;
                }
                endPoints[branch] = endPoints[branch].GetTreeGrandparent()!;
                branch = 1 - branch;
            }

            jumpNode = root;

            while (jumpNode != upperBound)
            {
                jumpNode = jumpNode.GetTreeGrandparent()!;
                jumpNode.isMarked = false;
            }
            ClearIsMarkedAndSetIsOuter(root, blossomFormingEdge.head[0]);
            ClearIsMarkedAndSetIsOuter(root, blossomFormingEdge.head[1]);

            return root;
        }

        /// <summary>
        /// Traverses the nodes in the tree from <c>start</c> to <c>root</c> and sets isMarked and
        /// isOuter to false
        /// </summary>
        /// <param name="root">first node</param>
        /// <param name="start">second node</param>
        private static void ClearIsMarkedAndSetIsOuter(BlossomVNode root, BlossomVNode start)
        {
            while (start != root)
            {
                start.isMarked = false;
                start.isOuter = false;
                start = start.GetTreeParent()!;
                start.isOuter = false;
                start = start.GetTreeParent()!;
            }
            root.isOuter = false;
            root.isMarked = false;
        }

        /// <summary>
        /// Reverses the direction of blossomSibling references
        /// </summary>
        /// <param name="blossomNode">some node on an odd circuit</param>
        private static void ReverseBlossomSiblings(BlossomVNode blossomNode)
        {
            BlossomVEdge prevEdge = blossomNode.blossomSibling!;
            BlossomVNode current = blossomNode;
            do
            {
                current = prevEdge.GetOpposite(current)!;
                BlossomVEdge tmpEdge = prevEdge;
                prevEdge = current.blossomSibling!;
                current.blossomSibling = tmpEdge;
            } while (current != blossomNode);
        }

        /// <summary>
        /// Checks whether the direction of blossomSibling references is suitable for the expand
        /// operation, i.e. an even branch goes from <c>blossomRoot</c> to <c>branchesEndpoint</c>.
        /// </summary>
        /// <param name="blossomRoot">a node on an odd circuit that is matched from the outside</param>
        /// <param name="branchesEndpoint">a node common to both branches</param>
        /// <returns>true if the condition described above holds, false otherwise</returns>
        private static bool ForwardDirection(BlossomVNode blossomRoot, BlossomVNode branchesEndpoint)
        {
            int hops = 0;
            BlossomVNode current = blossomRoot;
            while (current != branchesEndpoint)
            {
                ++hops;
                current = current.blossomSibling!.GetOpposite(current)!;
            }
            return (hops & 1) == 0;
        }

        /// <summary>
        /// Prints <c>blossomNode</c> and all its blossom siblings. This method is for debug purposes.
        /// </summary>
        /// <param name="blossomNode">the node to start from</param>
        private static void PrintBlossomNodes(BlossomVNode blossomNode)
        {
            Console.WriteLine("Printing blossom nodes");

            BlossomVNode current = blossomNode;
            do
            {
                Console.WriteLine(current);
                current = current.blossomSibling!.GetOpposite(current)!;
            } while (current != blossomNode);
        }

        /// <summary>
        /// Gets time in nano seconds
        /// </summary>
        /// <returns>time in nanoseconds</returns>
        private static long GetNanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

        #endregion private methods

    }
}
