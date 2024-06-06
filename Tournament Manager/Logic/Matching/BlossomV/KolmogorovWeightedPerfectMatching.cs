using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Tournament_Manager.Data;
using Tournament_Manager.Logic.Graph.cs;
using Tournament_Manager.Logic.util;

namespace Tournament_Manager.Logic.Matching.BlossomV
{
    internal class KolmogorovWeightedPerfectMatching<V, E> : IMatchingAlgorithm<V, E> where E : notnull where V : notnull
    {

        #region member

        internal static readonly bool DEBUG = true;

        public static readonly double EPS = IMatchingAlgorithm<V, E>.DEFAULT_EPSILON;

        public static readonly double INFINITY = 1e100;

        public static readonly double NO_PERFECT_MATCHING_THRESHOLD = 1e10;

        internal static readonly string NO_PERFECT_MATCHING = "There is no perfect matching in the specified graph";

        private static readonly BlossomVOptions DEFAULT_OPTIONS = new BlossomVOptions();

        internal readonly IGraph<V, E> initialGraph;

        internal readonly IGraph<V, E> graph;

        internal BlossomVState<V, E>? state;

        private BlossomVPrimalUpdater<V, E>? primalUpdater;

        private BlossomVDualUpdater<V, E>? dualUpdater;

        private IMatching<V, E>? matching;

        private DualSolution<V, E>? dualSolution;

        private BlossomVOptions options;

        private ObjectiveSense objectiveSense;

        #endregion member

        #region constructor

        /// <summary>
        /// Constructs a new instance of the algorithm using the default options. The goal of the
        /// constructed algorithm is to minimize the weight of the resulting perfect matching.
        /// </summary>
        /// <param name="graph">the graph for which to find a weighted perfect matching</param>
        public KolmogorovWeightedPerfectMatching(IGraph<V, E> graph) : this(graph, DEFAULT_OPTIONS, ObjectiveSense.MINIMIZE) { }

        /// <summary>
        /// Constructs a new instance of the algorithm using the default options. The goal of the
        /// constructed algorithm is to maximize or minimize the weight of the resulting perfect matching
        /// depending on the <c>maximize</c> parameter.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="objectiveSense"></param>
        public KolmogorovWeightedPerfectMatching(IGraph<V, E> graph, ObjectiveSense objectiveSense) : 
            this(graph, DEFAULT_OPTIONS, objectiveSense) { }

        /// <summary>
        /// Constructs a new instance of the algorithm with the specified <c>options</c>. The objective
        /// sense of the constructed algorithm is to minimize the weight of the resulting matching
        /// </summary>
        /// <param name="graph">the graph for which to find a weighted perfect matching</param>
        /// <param name="options">the options which define the strategies for the initialization and dual updates</param>
        public KolmogorovWeightedPerfectMatching(IGraph<V, E> graph, BlossomVOptions options) :
            this(graph, options, ObjectiveSense.MINIMIZE) { }

        /// <summary>
        /// Constructs a new instance of the algorithm with the specified <c>options</c>. The goal of
        /// the constructed algorithm is to maximize or minimize the weight of the resulting perfect
        /// matching depending on the <c>maximize</c> parameter.
        /// </summary>
        /// <param name="graph">the graph for which to find a weighted perfect matching</param>
        /// <param name="options">the options which define the strategies for the initialization and dual updates</param>
        /// <param name="objectiveSense">objective sense of the algorithm</param>
        /// <exception cref="ArgumentException"></exception>
        public KolmogorovWeightedPerfectMatching(IGraph<V, E> graph, BlossomVOptions options, ObjectiveSense objectiveSense)
        {
            this.objectiveSense = objectiveSense;

            if ((graph.VertexSet().Count & 1) == 1)
            {
                throw new ArgumentException(NO_PERFECT_MATCHING);
            } 
            else if (objectiveSense == ObjectiveSense.MAXIMIZE)
            {
                this.graph = new AsWeightedGraph<V, E>(graph, e=> - graph.GetEdgeWeight(e), true, false);
            } else
            {
                this.graph = graph;
            }

            this.initialGraph = graph;
            this.options = options;
        }

        #endregion constructor

        #region public methods

        public IMatching<V, E> GetMatching()
        {
            if (matching == null)
            {
                LazyComputeWeightedPerfectMatching();
            }
            return matching!;
        }

        /// <summary>
        /// Returns the computed solution to the dual linear program with respect to the weighted perfect
        /// matching linear program formulation.
        /// </summary>
        /// <returns>the solution to the dual linear program formulated on the <c>graph</c></returns>
        public DualSolution<V, E> GetDualSolution()
        {
            dualSolution = LazyComputeDualSolution();
            return dualSolution;
        }

        /// <summary>
        /// Performs an optimality test after the perfect matching is computed.<para/>
        /// 
        /// More precisely, checks whether dual variables of all pseudonodes and resulting slacks of all
        /// edges are non-negative and that slacks of all matched edges are exactly 0. Since the
        /// algorithm uses floating point arithmetic, this check is done with precision of
        /// <see cref="EPS"/>.<para/>
        /// 
        /// In general, this method should always return true unless the algorithm implementation has a bug.
        /// </summary>
        /// <returns>true if the assigned dual variables satisfy the dual linear program formulation AND
        ///          complementary slackness conditions are also satisfied. The total error must not
        ///          exceed EPS</returns>
        public bool TestOptimality()
        {
            LazyComputeWeightedPerfectMatching();
            return GetError() < EPS; // getError() won't return -1 since matching != null
        }

        /// <summary>
        /// Computes the error in the solution to the dual linear program. More precisely, the total
        /// error equals the sum of:
        /// <list type=">bullet">
        /// <item><description>
        /// Absolute value of edge slack if negative or the edge is matched
        /// </description></item>
        /// <item><description>
        /// Absolute value of pseudonode variable if negative
        /// </description></item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        public double GetError()
        {
            LazyComputeWeightedPerfectMatching();

            double error = TestNonNegativity();
            HashSet<E> matchedEdges = matching!.GetEdges();

            for (int i = 0; i < state!.graphEdges.Count; i++)
            {
                E graphEdge = state.graphEdges[i];
                BlossomVEdge edge = state.edges[i];

                double slack = graph.GetEdgeWeight(graphEdge);
                slack -= state.minEdgeWeight;

                BlossomVNode a = edge.headOriginal[0];
                BlossomVNode b = edge.headOriginal[1];

                Pair<BlossomVNode, BlossomVNode> lca = Lca(a, b);
                slack -= TotalDual(a, lca.GetFirst());
                slack -= TotalDual(b, lca.GetSecond());

                if (lca.GetFirst() == lca.GetSecond())
                {
                    // if a and b have a common ancestor, its dual is subtracted from edge's slack
                    slack += 2 * lca.GetFirst().GetTrueDual();
                }
                if (slack < 0 || matchedEdges.Contains(graphEdge))
                {
                    error += Math.Abs(slack);
                }
            }

            return error;
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Lazily runs the algorithm on the specified graph.
        /// </summary>
        private void LazyComputeWeightedPerfectMatching()
        {
            if (matching != null)
            {
                return;
            }
            BlossomVInitializer<V, E> initializer = new BlossomVInitializer<V, E>(graph);
            this.state = initializer.Initialize(options);
            this.primalUpdater = new BlossomVPrimalUpdater<V, E>(state);
            this.dualUpdater = new BlossomVDualUpdater<V, E>(state, primalUpdater);

            if (DEBUG)
            {
                PrintMap();
            }

            while (true)
            {
                int cycleTreeNum = state.treeNum;

                for (BlossomVNode? currentRoot = state.nodes[state.nodeNum].treeSiblingNext; currentRoot != null; )
                {
                    // initialize variables
                    BlossomVNode? nextRoot = currentRoot.treeSiblingNext;
                    BlossomVNode? nextNextRoot = null;
                    if (nextRoot != null)
                    {
                        nextNextRoot = nextRoot.treeSiblingNext;
                    }
                    BlossomVTree tree = currentRoot.tree!;
                    int iterationTreeNum = state.treeNum;

                    if (DEBUG)
                    {
                        PrintState();
                    }

                    // first phase
                    SetCurrentEdgesAndTryToAugment(tree);

                    if (iterationTreeNum == state.treeNum && options.updateDualsBefore)
                    {
                        dualUpdater.UpdateDualsSingle(tree);
                    }

                    // second phase
                    // apply primal operations to the current tree while it is possible
                    while (iterationTreeNum == state.treeNum)
                    {
                        if (DEBUG)
                        {
                            PrintState();
                            Console.WriteLine("Current tree is " + tree + ", current root is " + currentRoot);
                        }

                        if (!tree.plusInfinityEdges.IsEmpty())
                        {
                            // can grow tree
                            BlossomVEdge edge = tree.plusInfinityEdges.FindMin().GetValue()!;
                            if (edge.slack <= tree.eps)
                            {
                                primalUpdater.Grow(edge, true, true);
                                continue;
                            }
                        }
                        if (!tree.plusPlusEdges.IsEmpty())
                        {
                            // can shrink blossom
                            BlossomVEdge edge = tree.plusPlusEdges.FindMin().GetValue()!;
                            if (edge.slack <= 2 * tree.eps)
                            {
                                primalUpdater.Shrink(edge, true);
                                continue;
                            }
                        }
                        if (!tree.minusBlossoms.IsEmpty())
                        {
                            // can expand blossom
                            BlossomVNode node = tree.minusBlossoms.FindMin().GetValue()!;
                            if (node.dual <= tree.eps)
                            {
                                primalUpdater.Expand(node, true);
                                continue;
                            }
                        }
                        // can't do anything
                        if (DEBUG)
                        {
                            Console.WriteLine("Can't do anything");
                        }
                        break;
                    }

                    if (DEBUG)
                    {
                        PrintState();
                    }

                    // third phase
                    if (state.treeNum == iterationTreeNum)
                    {
                        tree!.currentEdge = null;
                        if (options.updateDualsAfter && dualUpdater.UpdateDualsSingle(tree))
                        {
                            // since some progress has been made, continue with the same trees
                            continue;
                        }
                        // clear current edge pointers
                        tree.ClearCurrentEdges();
                    }
                    currentRoot = nextRoot;
                    if (nextRoot != null && nextRoot.IsInfinityNode())
                    {
                        currentRoot = nextNextRoot;
                    }
                }

                if (DEBUG)
                {
                    PrintTrees();
                    PrintState();
                }

                if (state.treeNum == 0)
                {
                    // we are done
                    break;
                }
                if (cycleTreeNum == state.treeNum && dualUpdater.UpdateDuals(options.dualUpdateStrategy) <= 0)
                {
                    dualUpdater.UpdateDuals(BlossomVOptions.DualUpdateStrategy.MULTIPLE_TREE_CONNECTED_COMPONENTS);
                }
            }

            Finish();
        }

        /// <summary>
        /// Sets the currentEdge and currentDirection variables for all trees adjacent to the
        /// <c>tree</c>
        /// </summary>
        /// <param name="tree">the tree whose adjacent trees' variables are modified</param>
        private void SetCurrentEdgesAndTryToAugment(BlossomVTree tree)
        {
            for (BlossomVTree.TreeEdgeEnumerator enumerator = tree.GetTreeEdgeEnumerator(); enumerator.MoveNext(); )
            {
                BlossomVTreeEdge treeEdge = enumerator.Current;
                BlossomVTree opposite = treeEdge.head[enumerator.GetCurrentDirection()]!;

                if (!treeEdge.plusPlusEdges.IsEmpty())
                {
                    BlossomVEdge edge = treeEdge.plusPlusEdges.FindMin().GetValue()!;
                    if (edge.slack <= tree.eps + opposite.eps)
                    {
                        if (DEBUG)
                        {
                            Console.WriteLine("Bingo traverse");
                        }
                        primalUpdater!.Augment(edge);
                        break;
                    }
                }

                opposite.currentEdge = treeEdge;
                opposite.currentDirection = enumerator.GetCurrentDirection();
            }
        }

        /// <summary>
        /// Tests whether a non-negative dual variable is assigned to every blossom
        /// </summary>
        /// <returns>true if the condition described above holds</returns>
        private double TestNonNegativity()
        {
            BlossomVNode[] nodes = state!.nodes;
            double error = 0;

            for (int i = 0; i < state.nodeNum; i++)
            {
                BlossomVNode node = nodes[i].blossomParent!;
                while (node != null && !node.isMarked)
                {
                    if (node.dual < 0)
                    {
                        error += Math.Abs(node.dual);
                        break;
                    }
                    node.isMarked = true;
                    node = node.blossomParent!;
                }
            }
            ClearMarked();

            return error;
        }

        /// <summary>
        /// Computes the sum of all duals from <c>start</c> inclusive to <c>end</c> inclusive
        /// </summary>
        /// <param name="start">the node to start from</param>
        /// <param name="end">the node to end with</param>
        /// <returns>the sum = start.dual + start.blossomParent.dual + ... + end.dual</returns>
        private static double TotalDual(BlossomVNode start, BlossomVNode end)
        {
            if (end == start)
            {
                return start.GetTrueDual();
            }
            else
            {
                double result = 0;
                BlossomVNode? current = start;
                do
                {
                    result += current.GetTrueDual();
                    current = current.blossomParent;
                } while (current != null && current != end);
                result += end.GetTrueDual();

                return result;
            }
        }

        /// <summary>
        /// Returns $(b, b)$ in the case where the vertices <c>a</c> and <c>b</c> have a common
        /// ancestor blossom $b$. Otherwise, returns the outermost parent blossoms of nodes <c>a</c> and
        /// <c>b</c>
        /// </summary>
        /// <param name="a">a vertex whose lca is to be found with respect to another vertex</param>
        /// <param name="b">the other vertex whose lca is to be found</param>
        /// <returns>either an lca blossom of <c>a</c> and <c>b</c> or their outermost blossoms</returns>
        private static Pair<BlossomVNode, BlossomVNode> Lca(BlossomVNode a, BlossomVNode b)
        {
            BlossomVNode[] branches = new BlossomVNode[] { a, b };
            int dir = 0;
            Pair<BlossomVNode, BlossomVNode> result;

            while (true)
            {
                if (branches[dir].isMarked)
                {
                    result = new Pair<BlossomVNode, BlossomVNode>(branches[dir], branches[dir]);
                    break;
                }

                branches[dir].isMarked = true;
                if (branches[dir].isOuter)
                {
                    BlossomVNode jumpNode = branches[1 - dir];
                    while (!jumpNode!.isOuter && !jumpNode.isMarked)
                    {
                        jumpNode = jumpNode.blossomParent!;
                    }
                    if (jumpNode.isMarked)
                    {
                        result = new Pair<BlossomVNode, BlossomVNode>(jumpNode, jumpNode);
                    }
                    else
                    {
                        result = dir == 0 ? new Pair<BlossomVNode, BlossomVNode>(branches[dir], jumpNode)
                            : new Pair<BlossomVNode, BlossomVNode>(jumpNode, branches[dir]);
                    }
                    break;
                }
                branches[dir] = branches[dir].blossomParent!;
                dir = 1 - dir;
            }
            ClearMarked(a);
            ClearMarked(b);

            return result;
        }

        /// <summary>
        /// Clears the marking of <c>node</c> and all its ancestors up until the first unmarked vertex
        /// is encountered
        /// </summary>
        /// <param name="node">the node to start from</param>
        private static void ClearMarked(BlossomVNode? node)
        {
            do
            {
                node!.isMarked = false;
                node = node.blossomParent;
            } while (node != null && node.isMarked);
        }

        /// <summary>
        /// Clears the marking of all nodes and pseudonodes
        /// </summary>
        private void ClearMarked()
        {
            BlossomVNode[] nodes = state!.nodes;
            for (int i = 0; i < state.nodeNum; i++)
            {
                BlossomVNode? current = nodes[i];
                do
                {
                    current.isMarked = false;
                    current = current.blossomParent;
                } while (current != null && current.isMarked);
            }
        }

        /// <summary>
        /// Finishes the algorithm after all nodes are matched. The main problem it solves is that the
        /// matching after the end of primal and dual operations may not be valid in the contracted
        /// blossoms.<para/>
        /// 
        /// Property: if a matching is changed in the parent blossom, the matching in all lower blossoms
        /// can become invalid. Therefore, we traverse all nodes, find an unmatched node (it is
        /// necessarily contracted), go up to the first blossom whose matching hasn't been fixed (we set
        /// blossomGrandparent references to point to the previous nodes on the path). Then we start to
        /// change the matching accordingly all the way down to the initial node.<para/>
        /// 
        /// Let's call an edge that is matched to a blossom root a "blossom edge". To make the matching
        /// valid we move the blossom edge one layer down at a time so that in the end its endpoints are
        /// valid initial nodes of the graph. After this transformation we can't traverse the
        /// blossomSibling references any more. That is why we initially compute a mapping of every
        /// pseudonode to the set of nodes that are contracted in it. This map is needed to construct a
        /// dual solution after the matching in the graph becomes valid.
        /// </summary>
        private void Finish()
        {
            if (DEBUG)
            {
                Console.WriteLine("Finishing matching");
            }

            HashSet<E> edges = new HashSet<E>();
            BlossomVNode[] nodes = state!.nodes;
            List<BlossomVNode> processed = new List<BlossomVNode>();

            for (int i = 0; i < state.nodeNum; i++)
            {
                if (nodes[i].matched == null)
                {
                    BlossomVNode? blossomPrev = null;
                    BlossomVNode blossom = nodes[i];

                    // traverse the path from unmatched node to the first unprocessed pseudonode
                    do
                    {
                        blossom.blossomGrandparent = blossomPrev;
                        blossomPrev = blossom;
                        blossom = blossomPrev.blossomParent!;
                    } while (!blossom.isOuter);

                    // now node.blossomGrandparent points to the previous blossom in the hierarchy (not
                    // counting the blossom node)
                    while (true)
                    {
                        // find the root of the blossom. This can be a pseudonode
                        BlossomVNode? blossomRoot = blossom.matched!.GetCurrentOriginal(blossom);
                        if (blossomRoot == null)
                        {
                            blossomRoot = blossom.matched.head[0].isProcessed
                                ? blossom.matched.headOriginal[1] : blossom.matched.headOriginal[0];
                        }

                        while (blossomRoot!.blossomParent != blossom)
                        {
                            blossomRoot = blossomRoot.blossomParent;
                        }
                        blossomRoot.matched = blossom.matched;

                        BlossomVNode? node = blossom.GetOppositeMatched();
                        if (node != null)
                        {
                            node.isProcessed = true;
                            processed.Add(node);
                        }
                        node = blossomRoot.blossomSibling!.GetOpposite(blossomRoot);

                        // change the matching in the blossom
                        while (node != blossomRoot)
                        {
                            node!.matched = node.blossomSibling;
                            BlossomVNode nextNode = node.blossomSibling!.GetOpposite(node)!;
                            nextNode.matched = node.matched;
                            node = nextNode.blossomSibling!.GetOpposite(nextNode);
                        }
                        if (!blossomPrev!.isBlossom)
                        {
                            break;
                        }
                        blossom = blossomPrev;
                        blossomPrev = blossom.blossomGrandparent;
                    }
                    foreach (BlossomVNode processedNode in processed)
                    {
                        processedNode.isProcessed = false;
                    }

                    processed.Clear();
                }
            }
            // compute the final matching
            double weight = 0;
            for (int i = 0; i < state.nodeNum; i++)
            {
                E graphEdge = state.graphEdges[nodes[i].matched!.pos];

                if (!edges.Contains(graphEdge))
                {
                    edges.Add(graphEdge);
                    weight += state.graph.GetEdgeWeight(graphEdge);
                }
            }
            if (objectiveSense == ObjectiveSense.MAXIMIZE)
            {
                weight = -weight;
            }

            matching = new MatchingImpl<V, E>(state.graph, edges, weight);
        }

        /// <summary>
        /// Sets the blossomGrandparent references so that from a pseudonode we can make one step down to
        /// some node that belongs to that pseudonode
        /// </summary>
        private void PrepareForDualSolution()
        {
            BlossomVNode[] nodes = state!.nodes;
            for (int i = 0; i < state.nodeNum; i++)
            {
                BlossomVNode? current = nodes[i];
                BlossomVNode? prev = null;
                do
                {
                    current.blossomGrandparent = prev;
                    current.isMarked = true;
                    prev = current;
                    current = current.blossomParent;
                } while (current != null && !current.isMarked);
            }

            ClearMarked();
        }

        /// <summary>
        /// Computes the set of original contracted vertices in the <c>pseudonode</c> and puts computes
        /// value into the <c>blossomNodes</c>. If <c>node</c> contains other pseudonodes which haven't
        /// been processed already, recursively computes the same set for them.
        /// </summary>
        /// <param name="pseudoNode">the pseudonode whose contracted nodes are computed</param>
        /// <param name="blossomNodes">the mapping from pseudonodes to the original nodes contained in them</param>
        /// <returns></returns>
        private HashSet<V> GetBlossomNodes(BlossomVNode pseudoNode, Dictionary<BlossomVNode, HashSet<V>> blossomNodes)
        {
            if (blossomNodes.ContainsKey(pseudoNode))
            {
                return blossomNodes[pseudoNode];
            }

            HashSet<V> result = new HashSet<V>();
            BlossomVNode endNode = pseudoNode.blossomGrandparent!;
            BlossomVNode current = endNode!;
            do
            {
                if (current.isBlossom)
                {
                    if (!blossomNodes.ContainsKey(current))
                    {
                        result.UnionWith(GetBlossomNodes(current, blossomNodes));
                    }
                    else
                    {
                        result.UnionWith(blossomNodes[current]);
                    }
                }
                else
                {
                    result.Add(state!.graphVertices[current.pos]);
                }
                current = current.blossomSibling!.GetOpposite(current)!;
            } while (current != endNode);

            blossomNodes[pseudoNode] = result;

            return result;
        }

        /// <summary>
        /// Computes a solution to a dual linear program formulated on the initial graph.
        /// </summary>
        /// <returns>the solution to the dual linear program</returns>
        private DualSolution<V, E> LazyComputeDualSolution()
        {
            LazyComputeWeightedPerfectMatching();
            if (dualSolution != null)
            {
                return dualSolution;
            }

            Dictionary<HashSet<V>, Double> dualMap = new Dictionary<HashSet<V>, double>();
            Dictionary<BlossomVNode, HashSet<V>> nodesInBlossoms = new Dictionary<BlossomVNode, HashSet<V>>();
            BlossomVNode[] nodes = state!.nodes;

            PrepareForDualSolution();

            double dualShift = state.minEdgeWeight / 2;

            for (int i = 0; i < state.nodeNum; i++)
            {
                BlossomVNode? current = nodes[i];
                // jump up while the first already processed node is encountered
                do
                {
                    double dual = current.GetTrueDual();
                    if (!current.isBlossom)
                    {
                        dual += dualShift;
                    }
                    if (objectiveSense == ObjectiveSense.MAXIMIZE)
                    {
                        dual = -dual;
                    }
                    if (Math.Abs(dual) > EPS)
                    {
                        if (current.isBlossom)
                        {
                            dualMap[GetBlossomNodes(current, nodesInBlossoms)] = dual;
                        }
                        else
                        {
                            dualMap[new HashSet<V> { state.graphVertices[current.pos] }] = dual;
                        }
                    }
                    current.isMarked = true;
                    if (current.isOuter)
                    {
                        break;
                    }
                    current = current.blossomParent;
                } while (current != null && !current.isMarked);
            }
            ClearMarked();

            return new DualSolution<V, E>(initialGraph, dualMap);
        }

        /// <summary>
        /// Prints the state of the algorithm. This is a debug method.
        /// </summary>
        private void PrintState()
        {
            BlossomVNode[] nodes = state!.nodes;
            BlossomVEdge[] edges = state.edges;

            Console.WriteLine();
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("-");
            }
            Console.WriteLine();

            HashSet<BlossomVEdge> matched = new HashSet<BlossomVEdge>();
            for (int i = 0; i < state.nodeNum; i++)
            {
                BlossomVNode node = nodes[i];
                if (node.matched != null)
                {
                    BlossomVEdge matchedEdge = node.matched;
                    matched.Add(node.matched);
                    if (matchedEdge.head[0].matched == null || matchedEdge.head[1].matched == null)
                    {
                        Console.WriteLine("Problem with edge " + matchedEdge);
                        throw new SystemException();
                    }
                }
                Console.WriteLine(nodes[i]);
            }
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("-");
            }
            Console.WriteLine();
            for (int i = 0; i < state.edgeNum; i++)
            {
                Console.WriteLine(edges[i] + (matched.Contains(edges[i]) ? ", matched" : ""));
            }
        }

        /// <summary>
        /// Debug method
        /// </summary>
        private void PrintMap()
        {
            Console.WriteLine("Printing trees");
            for (BlossomVNode? root = state!.nodes[state.nodeNum].treeSiblingNext!; root != null; root = root.treeSiblingNext)
            {
                BlossomVTree tree = root.tree!;
                Console.WriteLine(tree);
            }
        }

        /// <summary>
        /// Debug method
        /// </summary>
        private void PrintTrees()
        {
            Console.WriteLine(state!.nodeNum + " " + state.edgeNum);
            for (int i = 0; i < state.nodeNum; i++)
            {
                Console.WriteLine(state.graphVertices[i] + " -> " + state.nodes[i]);
            }
        }

        #endregion private methods

        #region classes

        /// <summary>
        /// A solution to the dual linear program formulated on the <c>graph</c>
        /// </summary>
        /// <typeparam name="W">the graph vertex type</typeparam>
        /// <typeparam name="F">the graph edge type</typeparam>
        public class DualSolution<W, F> where W : notnull
        {

            /// <summary>
            /// The graph on which both primal and dual linear programs are formulated
            /// </summary>
            internal IGraph<W, F> graph;

            /// <summary>
            /// Mapping from sets of vertices of odd cardinality to their dual variables. Represents a
            /// solution to the dual linear program
            /// </summary>
            internal Dictionary<HashSet<W>, double> dualVariables;

            /// <summary>
            /// Constructs a new solution for the dual linear program
            /// </summary>
            /// <param name="graph">the graph on which the linear program is formulated</param>
            /// <param name="dualVariables">the mapping from sets of vertices of odd cardinality to their dual variables</param>
            public DualSolution(IGraph<W, F> graph, Dictionary<HashSet<W>, double> dualVariables)
            {
                this.graph = graph;
                this.dualVariables = dualVariables;
            }


            /// <summary>
            /// returns the graph on which the linear program is formulated
            /// </summary>
            /// <returns>the graph on which the linear program is formulated</returns>
            public IGraph<W, F> GetGraph()
            {
                return graph;
            }

            /// <summary>
            /// The mapping from sets of vertices of odd cardinality to their dual variables, which
            /// represents a solution to the dual linear program
            /// </summary>
            /// <returns>the mapping from sets of vertices of odd cardinality to their dual variables</returns>
            public Dictionary<HashSet<W>, double> GetDualVariables()
            {
                return dualVariables;
            }


            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("DualSolution{");
                sb.Append("graph=").Append(graph);
                sb.Append(", dualVariables=").Append(dualVariables);
                sb.Append('}');

                return sb.ToString();
            }
        }

        /// <summary>
        /// Describes the performance characteristics of the algorithm and numeric data about the number
        /// of performed dual operations during the main phase of the algorithm
        /// </summary>
        public class Statistics
        {

            /// <summary>
            /// Number of shrink operations
            /// </summary>
            internal int shrinkNum;

            /// <summary>
            /// Number of expand operations
            /// </summary>
            internal int expandNum;

            /// <summary>
            /// Number of grow operations
            /// </summary>
            internal int growNum;

            /// <summary>
            /// Time spent during the augment operation in nanoseconds
            /// </summary>
            internal long augmentTime = 0;

            /// <summary>
            /// Time spent during the expand operation in nanoseconds
            /// </summary>
            internal long expandTime = 0;

            /// <summary>
            /// Time spent during the shrink operation in nanoseconds
            /// </summary>
            internal long shrinkTime = 0;

            /// <summary>
            /// Time spent during the grow operation in nanoseconds
            /// </summary>
            internal long growTime = 0;

            /// <summary>
            /// Time spent during the dual update phase (either single tree or global) in nanoseconds
            /// </summary>
            internal long dualUpdatesTime = 0;


            /// <summary>
            /// 
            /// </summary>
            /// <returns>the number of shrink operations</returns>
            public int GetShrinkNum()
            {
                return shrinkNum;
            }

            /// <summary>
            /// the number of expand operations
            /// </summary>
            /// <returns></returns>
            public int GetExpandNum()
            {
                return expandNum;
            }

            /// <summary>
            /// the number of grow operations
            /// </summary>
            /// <returns></returns>
            public int GetGrowNum()
            {
                return growNum;
            }

            /// <summary>
            /// the time spent during the augment operation in nanoseconds
            /// </summary>
            /// <returns></returns>
            public long GetAugmentTime()
            {
                return augmentTime;
            }

            /// <summary>
            /// the time spent during the expand operation in nanoseconds
            /// </summary>
            /// <returns></returns>
            public long GetExpandTime()
            {
                return expandTime;
            }

            /// <summary>
            /// the time spent during the shrink operation in nanoseconds
            /// </summary>
            /// <returns></returns>
            public long GetShrinkTime()
            {
                return shrinkTime;
            }

            /// <summary>
            /// the time spent during the grow operation in nanoseconds
            /// </summary>
            /// <returns></returns>
            public long GetGrowTime()
            {
                return growTime;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>the time spent during the dual update phase (either single tree or global) in nanoseconds</returns>
            public long GetDualUpdatesTime()
            {
                return dualUpdatesTime;
            }


            public override string ToString()
            {
                return "Statistics{shrinkNum=" + shrinkNum + ", expandNum=" + expandNum + ", growNum="
                + growNum + ", augmentTime=" + augmentTime + ", expandTime=" + expandTime
                + ", shrinkTime=" + shrinkTime + ", growTime=" + growTime + '}';
            }

        }

        #endregion

    }
}
