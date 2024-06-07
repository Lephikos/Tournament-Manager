using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Graph;
using Tournament_Manager.Logic.Heap;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// Is used to start the Kolmogorov's Blossom V algorithm. Performs initialization of the algorithm's
    /// internal data structures and finds an initial matching according to the strategy specified in
    /// <c>options</c>.<para/>
    /// 
    /// The initialization process involves converting the graph into internal representation, allocating
    /// trees for unmatched vertices, and creating an auxiliary graph whose nodes correspond to
    /// alternating trees. The only part that varies is the strategy to find an initial matching to speed
    /// up the main part of the algorithm.<para/>
    /// 
    /// The simple initialization (option <see cref="BlossomVOptions.InitializationType.NONE"/>) doesn't find
    /// any matching and initializes the data structures by allocating $|V|$ single vertex trees. This is
    /// the fastest initialization strategy; however, it slows the main algorithm down.<para/>
    /// 
    /// The greedy initialization (option <see cref="BlossomVOptions.InitializationType.GREEDY"/>) runs in two
    /// phases. First, for every node it determines an edge of minimum weight and assigns half of that
    /// weight to the node's dual variable. This ensures that the slacks of all edges are non-negative.
    /// After that it goes through all nodes again, greedily increases its dual variable and chooses an
    /// incident matching edge if it is possible. After that every node is incident to at least one tight
    /// edge. The resulting matching is an output of this initialization strategy.<para/>
    /// 
    /// The fractional matching initialization (option
    /// <see cref="BlossomVOptions.InitializationType.FRACTIONAL"/>) is both the most complicated and the most
    /// efficient type of initialization. The linear programming formulation of the fractional matching
    /// problem is identical to the one used for bipartite graphs. More precisely:
    /// <list type="bullet">
    /// <item><description>
    /// Minimize the $sum_{e\in E}x_e\times c_e$ subject to:
    /// </description></item>
    /// <item><description>
    /// For all nodes: $\sum_{e is incident to v}x_e = 1$
    /// </description></item>
    /// <item><description>
    /// For all edges: $x_e \ge 0$
    /// </description></item>
    /// </list>
    /// 
    /// <b>Note:</b> for an optimal solution in general graphs
    /// we have to require the variables $x_e$ to be $0$ or $1$. For more information on this type of
    /// initialization, see: <i>David Applegate and William J. Cook. \Solving Large-Scale Matching
    /// Problems". In: Network Flows And Matching. 1991.</i>
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal class BlossomVInitializer<V, E> where V : notnull where E : notnull
    {

        #region member

        private static readonly bool DEBUG = KolmogorovWeightedPerfectMatching<V, E>.DEBUG;

        /// <summary>
        /// The graph for which to find a matching
        /// </summary>
        private readonly IGraph<V, E> graph;

        /// <summary>
        /// Number of nodes in the graph
        /// </summary>
        private int nodeNum;

        /// <summary>
        /// Number of edges in the graph
        /// </summary>
        private int edgeNum = 0;

        /// <summary>
        /// An array of nodes that will be passed to the resulting state object
        /// </summary>
        private BlossomVNode[]? nodes;

        /// <summary>
        /// An array of edges that will be passed to the resulting state object
        /// </summary>
        private BlossomVEdge[]? edges;

        /// <summary>
        /// Generic vertices of the <c>>graph</c> in the same order as internal nodes in the array
        /// <c>nodes</c>. Since for each node in the <c>nodes</c> we know its position in the
        /// <c>nodes</c>, we can determine its generic counterpart in constant time
        /// </summary>
        private List<V>? graphVertices;

        /// <summary>
        /// Generic edges of the <c>>graph</c> in the same order as internal edges in the array
        /// <c>edges</c>. Since for each edge in the <c>edges</c> we know its position in the
        /// <c>edges</c>, we can determine its generic counterpart in constant time
        /// </summary>
        /// </summary>
        private List<E>? graphEdges;

        #endregion member

        #region constructor

        /// <summary>
        /// Creates a new BlossomVInitializer instance
        /// </summary>
        /// <param name="graph">the graph to search matching in</param>
        public BlossomVInitializer(IGraph<V, E> graph)
        {
            this.graph = graph;
            nodeNum = graph.VertexSet().Count;
        }

        #endregion constructor

        #region public methods

        /// <summary>
        /// Converts the generic graph representation into the data structure form convenient for the
        /// algorithm, and initializes the matching according to the strategy specified in
        /// <c>options</c>
        /// </summary>
        /// <param name="options">the options of the algorithm</param>
        /// <returns>the state object with all necessary information for the algorithm</returns>
        public BlossomVState<V, E> Initialize(BlossomVOptions options)
        {
            switch (options.initializationType)
            {
                case BlossomVOptions.InitializationType.NONE: return SimpleInitialization(options);

                case BlossomVOptions.InitializationType.GREEDY: return GreedyInitialization(options);

                default: return FractionalMatchingInitialization(options);
            }
        }

        /// <summary>
        /// Adds a new edge between <c>from</c> and <c>to</c>. The resulting edge points from
        /// <c>from</c> to <c>to</c>
        /// </summary>
        /// <param name="from">the tail of this edge</param>
        /// <param name="to">the head of this edge</param>
        /// <param name="slack">the slack of the resulting edge</param>
        /// <param name="pos">position of the resulting edge in the array <c>edges</c></param>
        /// <returns>the newly added edge</returns>
        public static BlossomVEdge AddEdge(BlossomVNode from, BlossomVNode to, double slack, int pos)
        {
            BlossomVEdge edge = new BlossomVEdge(pos);
            edge.slack = slack;
            edge.headOriginal[0] = to;
            edge.headOriginal[1] = from;

            // the call to the BlossomVNode#AddEdge implies setting head[dir] reference
            from.AddEdge(edge, 0);
            to.AddEdge(edge, 1);

            return edge;
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Performs simple initialization of the matching by allocating $|V|$ trees. The result of this
        /// type of initialization is an empty matching. That is why this is the most basic type of
        /// initialization.
        /// </summary>
        /// <param name="options">the options of the algorithm</param>
        /// <returns>the state object with all necessary information for the algorithm</returns>
        private BlossomVState<V, E> SimpleInitialization(BlossomVOptions options)
        {
            double minEdgeWeight = InitGraph();

            foreach (BlossomVNode node in nodes!)
            {
                node.isOuter = true;
            }

            AllocateTrees();
            InitAuxiliaryGraph();

            return new BlossomVState<V, E>(graph, nodes, edges!, nodeNum, edgeNum, nodeNum, graphVertices!, graphEdges!, options, minEdgeWeight);
        }

        /// <summary>
        /// Performs greedy initialization of the algorithm. For the description of this initialization
        /// strategy see the class description.
        /// </summary>
        /// <param name="options">the options of the algorithm</param>
        /// <returns>the state object with all necessary information for the algorithm</returns>
        private BlossomVState<V, E> GreedyInitialization(BlossomVOptions options)
        {
            double minEdgeWeight = InitGraph();
            int treeNum = InitGreedy();

            AllocateTrees();
            InitAuxiliaryGraph();

            return new BlossomVState<V, E>(graph, nodes!, edges!, nodeNum, edgeNum, treeNum, graphVertices!, graphEdges!, options, minEdgeWeight);
        }

        /// <summary>
        /// Performs fractional matching initialization, see class description
        /// for the description.
        /// </summary>
        /// <param name="options">the options of the algorithm</param>
        /// <returns>the state object with all necessary information for the algorithm</returns>
        private BlossomVState<V, E> FractionalMatchingInitialization(BlossomVOptions options)
        {
            double minEdgeWeight = InitGraph();
            InitGreedy();
            AllocateTrees();
            int treeNum = InitFractional();
            InitAuxiliaryGraph();

            return new BlossomVState<V, E>(graph, nodes!, edges!, nodeNum, edgeNum, treeNum, graphVertices!, graphEdges!, options, minEdgeWeight);
        }

        /// <summary>
        /// Converts the generic graph representation into the form convenient for the algorithm
        /// </summary>
        /// <returns></returns>
        private double InitGraph()
        {
            int expectedEdgeNum = graph.EdgeSet().Count;
            nodes = new BlossomVNode[nodeNum + 1];
            edges = new BlossomVEdge[expectedEdgeNum];
            graphVertices = new List<V>(nodeNum);
            graphEdges = new List<E>(expectedEdgeNum);
            Dictionary<V, BlossomVNode> vertexMap = new Dictionary<V, BlossomVNode>(nodeNum);
            int i = 0;

            // maps nodes
            foreach (V vertex in graph.VertexSet())
            {
                nodes[i] = new BlossomVNode(i);
                graphVertices.Add(vertex);
                vertexMap[vertex] = nodes[i];
                i++;
            }

            nodes[nodeNum] = new BlossomVNode(nodeNum); // auxiliary node to keep track of the first item in the linked list of tree roots
            i = 0;
            double minEdgeWeight = graph.EdgeSet().Select(edge => graph.GetEdgeWeight(edge)).DefaultIfEmpty(0d).Min();

            // maps edges
            foreach (E e in graph.EdgeSet())
            {
                BlossomVNode source = vertexMap[graph.GetEdgeSource(e)];
                BlossomVNode target = vertexMap[graph.GetEdgeTarget(e)];
                if (source != target)
                { 
                    // we avoid self-loops in order to support pseudographs
                    edgeNum++;
                    BlossomVEdge edge = AddEdge(source, target, graph.GetEdgeWeight(e) - minEdgeWeight, i);
                    edges[i] = edge;
                    graphEdges.Add(e);
                    i++;
                }
            }
            return minEdgeWeight;
        }

        /// <summary>
        /// Performs greedy matching initialization.<para/>
        /// 
        /// For every node we choose an incident edge of minimum slack and set its dual to half of this
        /// slack. This maintains the nonnegativity of edge slacks. After that we go through all nodes
        /// again, greedily increase their dual variables, and match them if it is possible.
        /// </summary>
        /// <returns>the number of unmatched nodes, which equals the number of trees</returns>
        private int InitGreedy()
        {
            // set all dual variables to infinity
            for (int i = 0; i < nodeNum; i++)
            {
                nodes![i].dual = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;
            }

            // set dual variables to half of the minimum weight of the incident edges
            for (int i = 0; i < edgeNum; i++)
            {
                BlossomVEdge edge = edges![i];
                if (edge.head[0].dual > edge.slack)
                {
                    edge.head[0].dual = edge.slack;
                }
                if (edge.head[1].dual > edge.slack)
                {
                    edge.head[1].dual = edge.slack;
                }
            }

            // divide dual variables by two; this ensures nonnegativity of all slacks;
            // decrease edge slacks accordingly
            for (int i = 0; i < edgeNum; i++)
            {
                BlossomVEdge edge = edges![i];
                BlossomVNode source = edge.head[0];
                BlossomVNode target = edge.head[1];
                if (!source.isOuter)
                {
                    source.isOuter = true;
                    source.dual /= 2;
                }
                edge.slack -= source.dual;
                if (!target.isOuter)
                {
                    target.isOuter = true;
                    target.dual /= 2;
                }
                edge.slack -= target.dual;
            }

            // go through all vertices, greedily increase their dual variables to the minimum slack of
            // incident edges;
            // if there exists a tight unmatched edge in the neighborhood, match it
            int treeNum = nodeNum;
            for (int i = 0; i < nodeNum; i++)
            {
                BlossomVNode node = nodes![i];
                if (!node.IsInfinityNode())
                {
                    double minSlack = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;

                    // find the minimum slack of incident edges
                    for (BlossomVNode.IncidentEdgeEnumerator incidentEdgeEnumerator = node.GetIncidentEdgeEnumerator(); incidentEdgeEnumerator.MoveNext(); )
                    {
                        BlossomVEdge edge = incidentEdgeEnumerator.Current;
                        if (edge.slack < minSlack)
                        {
                            minSlack = edge.slack;
                        }
                    }
                    node.dual += minSlack;
                    double resultMinSlack = minSlack;

                    // subtract minimum slack from the slacks of all incident edges
                    for (BlossomVNode.IncidentEdgeEnumerator incidentEdgeEnumerator = node.GetIncidentEdgeEnumerator(); incidentEdgeEnumerator.MoveNext();)
                    {
                        BlossomVEdge edge = incidentEdgeEnumerator.Current;
                        int dir = incidentEdgeEnumerator.GetDir();

                        if (edge.slack <= resultMinSlack && node.IsPlusNode()
                            && edge.head[dir].IsPlusNode())
                        {
                            node.label = BlossomVNode.Label.INFINITY;
                            edge.head[dir].label = BlossomVNode.Label.INFINITY;
                            node.matched = edge;
                            edge.head[dir].matched = edge;
                            treeNum -= 2;
                        }
                        edge.slack -= resultMinSlack;
                    }
                }
            }

            return treeNum;
        }

        /// <summary>
        /// Initializes an auxiliary graph by adding tree edges between trees and adding (+, +)
        /// cross-tree edges and (+, inf) edges to the appropriate heaps
        /// </summary>
        private void InitAuxiliaryGraph()
        {
            // go through all tree roots and visit all incident edges of those roots.
            // if a (+, inf) edge is encountered => add it to the infinity heap
            // if a (+, +) edge is encountered and the opposite node hasn't been processed yet =>
            // add this edge to the heap of (+, +) cross-tree edges
            for (BlossomVNode? root = nodes![nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                BlossomVTree tree = root.tree!;

                for (BlossomVNode.IncidentEdgeEnumerator edgeEnumerator = root.GetIncidentEdgeEnumerator(); edgeEnumerator.MoveNext(); )
                {
                    BlossomVEdge edge = edgeEnumerator.Current;
                    BlossomVNode opposite = edge.head[edgeEnumerator.GetDir()];

                    if (opposite.IsInfinityNode())
                    {
                        tree.AddPlusInfinityEdge(edge);
                    }
                    else if (!opposite.isProcessed)
                    {
                        if (opposite.tree!.currentEdge == null)
                        {
                            BlossomVTree.AddTreeEdge(tree, opposite.tree);
                        }
                        opposite.tree.currentEdge!.AddPlusPlusEdge(edge);
                    }
                }
                root.isProcessed = true;

                for (BlossomVTree.TreeEdgeEnumerator treeEdgeEnumerator = tree.GetTreeEdgeEnumerator(); treeEdgeEnumerator.MoveNext();)
                {
                    BlossomVTreeEdge treeEdge = treeEdgeEnumerator.Current;
                    treeEdge.head[treeEdgeEnumerator.GetCurrentDirection()]!.currentEdge = null;
                }
            }

            // clear isProcessed flags
            for (BlossomVNode? root = nodes[nodeNum].treeSiblingNext; root != null; root = root.treeSiblingNext)
            {
                root.isProcessed = false;
            }
        }

        /// <summary>
        /// Allocates trees. Initializes the doubly linked list of tree roots via treeSiblingPrev and
        /// treeSiblingNext. The same mechanism is used for keeping track of the children of a node in
        /// the tree. The lookup <c>nodes[nodeNum]</c> is used to quickly find the first root in the
        /// linked list
        /// </summary>
        private void AllocateTrees()
        {
            BlossomVNode lastRoot = nodes![nodeNum];
            for (int i = 0; i < nodeNum; i++)
            {
                BlossomVNode node = nodes[i];
                if (node.IsPlusNode())
                {
                    node.treeSiblingPrev = lastRoot;
                    lastRoot.treeSiblingNext = node;
                    lastRoot = node;
                    new BlossomVTree(node);
                }
            }
            lastRoot.treeSiblingNext = null;
        }

        /// <summary>
        /// Finishes the fractional matching initialization. Goes through all nodes and expands
        /// half-loops. The total number or trees equals to the number of half-loops. Tree roots are
        /// chosen arbitrarily.
        /// </summary>
        /// <returns>the number of trees in the resulting state object, which equals the number of unmatched nodes</returns>
        private int Finish()
        {
            if (DEBUG)
            {
                Console.WriteLine("Finishing fractional matching initialization");
            }

            BlossomVNode prevRoot = nodes![nodeNum];
            int treeNum = 0;

            for (int i = 0; i < nodeNum; i++)
            {
                BlossomVNode node = nodes[i];
                node.firstTreeChild = node.treeSiblingNext = node.treeSiblingPrev = null;
                if (!node.isOuter)
                {
                    ExpandInit(node, null); // this node becomes unmatched
                    node.parentEdge = null;
                    node.label = BlossomVNode.Label.PLUS;
                    new BlossomVTree(node);

                    prevRoot.treeSiblingNext = node;
                    node.treeSiblingPrev = prevRoot;
                    prevRoot = node;
                    treeNum++;
                }
            }

            return treeNum;
        }

        /// <summary>
        /// Performs lazy delta spreading during the fractional matching initialization.<para/>
        /// 
        /// Goes through all nodes in the tree rooted at {@code root} and adds {@code eps} to the "+"
        /// nodes and subtracts {@code eps} from "-" nodes. Updates incident edges respectively.
        /// </summary>
        /// <param name="heap">the heap for storing best edges</param>
        /// <param name="root">the root of the current tree</param>
        /// <param name="eps">the accumulated dual change of the tree</param>
        private static void UpdateDuals(PairingHeap<double, BlossomVEdge> heap, BlossomVNode root, double eps)
        {
            for (BlossomVTree.TreeNodeEnumerator treeNodeEnumerator = new BlossomVTree.TreeNodeEnumerator(root); treeNodeEnumerator.MoveNext(); )
            {
                BlossomVNode treeNode = treeNodeEnumerator.Current;
                if (treeNode.isProcessed)
                {
                    treeNode.dual += eps;
                    if (!treeNode.isTreeRoot)
                    {
                        BlossomVNode minusNode = treeNode.GetOppositeMatched()!;
                        minusNode.dual -= eps;
                        double delta = eps - treeNode.matched!.slack;

                        for (BlossomVNode.IncidentEdgeEnumerator enumerator = minusNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
                        {
                            enumerator.Current.slack += delta;
                        }
                    }

                    for (BlossomVNode.IncidentEdgeEnumerator enumerator = treeNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext();)
                    {
                        enumerator.Current.slack -= eps;
                    }
                    treeNode.isProcessed = false;
                }
            }

            // clear bestEdge after dual update
            while (!heap.IsEmpty())
            {
                BlossomVEdge edge = heap.FindMin().GetValue()!;
                BlossomVNode node = edge.head[0].IsInfinityNode() ? edge.head[0] : edge.head[1];
                RemoveFromHeap(node);
            }
        }

        /// <summary>
        /// Adds "best edges" to the <c>heap</c>
        /// </summary>
        /// <param name="heap">the heap for storing best edges</param>
        /// <param name="node">infinity node <c>bestEdge</c> is incident to</param>
        /// <param name="bestEdge">current best edge of the <c>node</c></param>
        private static void AddToHead(IAddressableHeap<Double, BlossomVEdge> heap, BlossomVNode node, BlossomVEdge bestEdge)
        {
            bestEdge.handle = heap.Insert(bestEdge.slack, bestEdge);
            node.bestEdge = bestEdge;
        }

        /// <summary>
        /// Removes "best edge" from <c>heap</c>
        /// </summary>
        /// <param name="node">the node which best edge should be removed from the heap it is stored in</param>
        private static void RemoveFromHeap(BlossomVNode node)
        {
            node.bestEdge!.handle!.Delete();
            node.bestEdge.handle = null;
            node.bestEdge = null;
        }

        /// <summary>
        /// Finds blossom root during the fractional matching initialization
        /// </summary>
        /// <param name="blossomFormingEdge">a tight (+, +) in-tree edge</param>
        /// <returns>the root of the blossom formed by the <c>blossomFormingEdge</c></returns>
        private static BlossomVNode FindBlossomRootInit(BlossomVEdge blossomFormingEdge)
        {
            BlossomVNode[] branches = new BlossomVNode[] { blossomFormingEdge.head[0], blossomFormingEdge.head[1] };
            BlossomVNode root, upperBound; // need to be scoped outside of the loop
            BlossomVNode jumpNode;
            int dir = 0;

            while (true)
            {
                if (!branches[dir].isOuter)
                {
                    root = branches[dir];
                    upperBound = branches[1 - dir];
                    break;
                }
                branches[dir].isOuter = false;

                if (branches[dir].isTreeRoot)
                {
                    upperBound = branches[dir];
                    jumpNode = branches[1 - dir];

                    while (jumpNode.isOuter)
                    {
                        jumpNode.isOuter = false;
                        jumpNode = jumpNode.GetTreeParent()!;
                        jumpNode.isOuter = false;
                        jumpNode = jumpNode.GetTreeParent()!;
                    }
                    root = jumpNode;
                    break;
                }
                BlossomVNode node = branches[dir].GetTreeParent()!;
                node.isOuter = false;
                branches[dir] = node.GetTreeParent()!;
                dir = 1 - dir;
            }

            jumpNode = root;
            while (jumpNode != upperBound)
            {
                jumpNode = jumpNode.GetTreeParent()!;
                jumpNode.isOuter = true;
                jumpNode = jumpNode.GetTreeParent()!;
                jumpNode.isOuter = true;
            }

            return root;
        }

        /// <summary>
        /// Handles encountered infinity edges incident to "+" nodes of the alternating tree. This method
        /// determines whether the <c>infinityEdge</c> is tight. If so, it applies grow operation to it.
        /// Otherwise, it determines whether it has smaller slack than <c>criticalEps</c>. If so, this
        /// edge becomes the best edge of the "+" node in the tree.
        /// </summary>
        /// <param name="heap">the heap of infinity edges incident to the currently processed tree</param>
        /// <param name="infinityEdge">encountered infinity edge</param>
        /// <param name="dir">irection of the infinityEdge to the infinity node</param>
        /// <param name="eps">the eps of the current branch</param>
        /// <param name="criticalEps">the value by which the epsilon of the current tree can be increased so
        ///                           that the slacks of (+, +) cross-tree and in-tree edges don't become negative</param>
        private static void HandleInfinityEdgeInit(IAddressableHeap<double, BlossomVEdge> heap, BlossomVEdge infinityEdge, 
            int dir, double eps, double criticalEps)
        {
            BlossomVNode inTreeNode = infinityEdge.head[1 - dir];
            BlossomVNode oppositeNode = infinityEdge.head[dir];
            if (infinityEdge.slack > eps)
            { // this edge isn't tight, but this edge can become a best
              // edge
                if (infinityEdge.slack < criticalEps)
                { // this edge can become a best edge
                    if (oppositeNode.bestEdge == null)
                    { // inTreeNode hadn't had any best edge before
                        AddToHead(heap, oppositeNode, infinityEdge);
                    }
                    else
                    {
                        if (infinityEdge.slack < oppositeNode.bestEdge.slack)
                        {
                            RemoveFromHeap(oppositeNode);
                            AddToHead(heap, oppositeNode, infinityEdge);
                        }
                    }
                }
            }
            else
            {
                if (DEBUG)
                {
                    Console.WriteLine("Growing an edge " + infinityEdge);
                }

                // this is a tight edge, can grow it
                if (oppositeNode.bestEdge != null)
                {
                    RemoveFromHeap(oppositeNode);
                }
                oppositeNode.label = BlossomVNode.Label.MINUS;
                inTreeNode.AddChild(oppositeNode, infinityEdge, true);

                BlossomVNode plusNode = oppositeNode.matched!.GetOpposite(oppositeNode)!;
                if (plusNode.bestEdge != null)
                {
                    RemoveFromHeap(plusNode);
                }
                plusNode.label = BlossomVNode.Label.PLUS;
                oppositeNode.AddChild(plusNode, plusNode.matched!, true);
            }
        }

        /// <summary>
        /// Augments the tree rooted at <c>treeRoot</c> via <c>augmentEdge</c>. The augmenting branch
        /// starts at <c>branchStart</c>
        /// </summary>
        /// <param name="treeRoot">the root of the tree to augment</param>
        /// <param name="branchStart">the endpoint of the <c>augmentEdge</c> which belongs to the currentTree</param>
        /// <param name="AugmentEdge">a tight (+, +) cross-tree edge</param>
        private static void AugmentBranchInit(BlossomVNode treeRoot, BlossomVNode branchStart, BlossomVEdge augmentEdge)
        {
            if (DEBUG)
            {
                Console.WriteLine("Augmenting an edge " + augmentEdge);
            }
            for (BlossomVTree.TreeNodeEnumerator enumerator = new BlossomVTree.TreeNodeEnumerator(treeRoot); enumerator.MoveNext(); )
            {
                enumerator.Current.label = BlossomVNode.Label.INFINITY;
            }

            BlossomVNode plusNode = branchStart;
            BlossomVNode? minusNode = branchStart.GetTreeParent();
            BlossomVEdge matchedEdge = augmentEdge;

            // alternate the matching from branch start up to the tree root
            while (minusNode != null)
            {
                plusNode.matched = matchedEdge;
                minusNode.matched = matchedEdge = minusNode.parentEdge!;
                plusNode = minusNode.GetTreeParent()!;
                minusNode = plusNode.GetTreeParent();
            }
            treeRoot.matched = matchedEdge;

            treeRoot.RemoveFromChildList();
            treeRoot.isTreeRoot = false;
        }

        /// <summary>
        /// Forms a 1/2-valued odd circuit. Nodes from the odd circuit aren't actually contracted into a
        /// single pseudonode. The blossomSibling references are set so that the nodes form a circular
        /// linked list. The matching is updated respectively.<para/>
        /// 
        /// <b>Note:</b> each node of the circuit can be expanded in the future and become a new tree root.
        /// </summary>
        /// <param name="blossomFormingEdge">a tight (+, +) in-tree edge that forms an odd circuit</param>
        /// <param name="treeRoot">the root of the tree odd circuit belongs to</param>
        private static void ShrinkInit(BlossomVEdge blossomFormingEdge, BlossomVNode treeRoot)
        {
            if (DEBUG)
            {
                Console.WriteLine("Shrinking an edge " + blossomFormingEdge);
            }

            for (BlossomVTree.TreeNodeEnumerator enumerator = new BlossomVTree.TreeNodeEnumerator(treeRoot); enumerator.MoveNext(); )
            {
                enumerator.Current.label = BlossomVNode.Label.INFINITY;
            }
            BlossomVNode blossomRoot = FindBlossomRootInit(blossomFormingEdge);

            BlossomVEdge prevEdge;

            // alternate the matching from blossom root up to the tree root
            if (!blossomRoot.isTreeRoot)
            {
                BlossomVNode minusNode = blossomRoot.GetTreeParent()!;
                prevEdge = minusNode.parentEdge!;
                minusNode.matched = minusNode.parentEdge;
                BlossomVNode plusNode = minusNode.GetTreeParent()!;
                while (plusNode != treeRoot)
                {
                    minusNode = plusNode.GetTreeParent()!;
                    plusNode.matched = prevEdge;
                    minusNode.matched = prevEdge = minusNode.parentEdge!;
                    plusNode = minusNode.GetTreeParent()!;
                }
                plusNode.matched = prevEdge;
            }

            // set the circular blossomSibling references
            prevEdge = blossomFormingEdge;

            for (BlossomVEdge.BlossomNodesEnumerator enumerator = blossomFormingEdge.GetBlossomNodesEnumerator(blossomRoot); enumerator.MoveNext(); )
            {
                BlossomVNode current = enumerator.Current;
                current.label = BlossomVNode.Label.PLUS;

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
            treeRoot.RemoveFromChildList();
            treeRoot.isTreeRoot = false;
        }

        /// <summary>
        /// Expands a 1/2-valued odd circuit. Essentially, changes the matching of the circuit so that
        /// the <c>blossomNode</c> becomes matched to the <c>blossomNodeMatched</c> edge and all other
        /// nodes become matched. Sets the labels of the matched nodes of the circuit to
        /// <see cref="BlossomVNode.Label.INFINITY"/>
        /// </summary>
        /// <param name="blossomNode">some node that belongs to the "contracted" odd circuit</param>
        /// <param name="blossomNodeMatched">a matched edge of the <c>blossomNode</c>, which doesn't belong to
        ///                                  the circuit. <b>Note:</b> this value can be <c>null</c></param>
        private static void ExpandInit(BlossomVNode blossomNode, BlossomVEdge? blossomNodeMatched)
        {
            if (DEBUG)
            {
                Console.WriteLine("Expanding node " + blossomNode);
            }
            BlossomVNode currentNode = blossomNode.blossomSibling!.GetOpposite(blossomNode)!;

            blossomNode.isOuter = true;
            blossomNode.label = BlossomVNode.Label.INFINITY;
            blossomNode.matched = blossomNodeMatched;
            // change the matching in the blossom
            do
            {
                currentNode.matched = currentNode.blossomSibling;
                BlossomVEdge prevEdge = currentNode.blossomSibling!;
                currentNode.isOuter = true;
                currentNode.label = BlossomVNode.Label.INFINITY;
                currentNode = currentNode.blossomSibling!.GetOpposite(currentNode)!;

                currentNode.matched = prevEdge;
                currentNode.isOuter = true;
                currentNode.label = BlossomVNode.Label.INFINITY;
                currentNode = currentNode.blossomSibling!.GetOpposite(currentNode)!;
            } while (currentNode != blossomNode);
        }

        /// <summary>
        /// Solves the fractional matching problem formulated on the initial graph. See the class
        /// description for more information about fractional matching initialization.
        /// </summary>
        /// <returns>the number of trees in the resulting state object, which equals to the number of
        ///          unmatched nodes.</returns>
        private int InitFractional()
        {
            /*
         * For every free node u, which is adjacent to at least one "+" node in the current tree, we
         * keep track of an edge that has minimum slack and connects node u and some "+" node in the
         * current tree. This edge is called a "best edge".
         */
            PairingHeap<double, BlossomVEdge> heap = new PairingHeap<double, BlossomVEdge>();

            for (BlossomVNode? root = nodes![nodeNum].treeSiblingNext; root != null;)
            {
                BlossomVNode? root2 = root.treeSiblingNext;
                BlossomVNode? root3 = null;
                if (root2 != null)
                {
                    root3 = root2.treeSiblingNext;
                }
                BlossomVNode currentNode = root;

                heap.Clear();

                double branchEps = 0;
                Action flag = Action.NONE;
                BlossomVNode branchRoot = currentNode;
                BlossomVEdge? criticalEdge = null;
                /*
                 * Let's denote the minimum slack of (+, inf) edges incident to nodes of this tree as
                 * infSlack. Critical eps is the minimum dual value which can be chosen as the branchEps
                 * so that it doesn't violate the dual constraints on (+, +) in-tree and cross-tree
                 * edges. It is always greater than or equal to the branchEps. If it is equal to the
                 * branchEps, a shrink or augment operation can be applied immediately. If it is greater
                 * than branchEps, we have to compare it with infSlack. If criticalEps is greater than
                 * infSlack, we have to do a grow operation after we increase the branchEps by infSlack
                 * - branchEps. Otherwise, we can apply shrink or augment operations after we increase
                 * the branchEps by criticalEps - branchEps.
                 */
                double criticalEps = KolmogorovWeightedPerfectMatching<V, E>.INFINITY;
                int criticalDir = -1;
                bool primalOperation = false;

                /*
                 * Grow a tree as much as possible. Main goal is to apply a primal operation. Therefore,
                 * if we encounter a tight (+, +) cross-tree or in-tree edge => we won't be able to
                 * increase dual objective function anymore (can't increase branchEps) => we go out of
                 * the loop, apply lazy dual changes to the current branch and perform an augment or
                 * shrink operation.
                 *
                 * A tree is grown in phases. Each phase starts with a new "branch"; the reason to start
                 * a new branch is that the tree can't be grown any further without dual changes and
                 * therefore no primal operation can be applied. That is why we choose an edge of
                 * minimum slack from heap, and set the eps of the branch so that this edge becomes
                 * tight
                 */
                while (true)
                {
                    currentNode!.isProcessed = true;
                    currentNode.dual -= branchEps; // apply lazy delta spreading

                    if (!currentNode.isTreeRoot)
                    {
                        // apply lazy delta spreading to the matched "-" node
                        currentNode.GetOppositeMatched()!.dual += branchEps;
                    }

                    // Process edges incident to the current node
                    BlossomVNode.IncidentEdgeEnumerator enumerator;
                    for (enumerator = currentNode.GetIncidentEdgeEnumerator(); enumerator.MoveNext(); )
                    {
                        BlossomVEdge currentEdge = enumerator.Current;
                        int dir = enumerator.GetDir();

                        currentEdge.slack += branchEps; // apply lazy delta spreading
                        BlossomVNode oppositeNode = currentEdge.head[dir];

                        if (oppositeNode.tree == root.tree)
                        {
                            // opposite node is in the same tree
                            if (oppositeNode.IsPlusNode())
                            {
                                double slack = currentEdge.slack;
                                if (!oppositeNode.isProcessed)
                                {
                                    slack += branchEps;
                                }
                                if (2 * criticalEps > slack || criticalEdge == null)
                                {
                                    flag = Action.SHRINK;
                                    criticalEps = slack / 2;
                                    criticalEdge = currentEdge;
                                    criticalDir = dir;
                                    if (criticalEps <= branchEps)
                                    {
                                        // found a tight (+, +) in-tree edge to shrink => go out of the
                                        // loop
                                        primalOperation = true;
                                        break;
                                    }
                                }
                            }

                        }
                        else if (oppositeNode.IsPlusNode())
                        {
                            // current edge is a (+, +) cross-tree edge
                            if (criticalEps >= currentEdge.slack || criticalEdge == null)
                            {
                                //
                                flag = Action.AUGMENT;
                                criticalEps = currentEdge.slack;
                                criticalEdge = currentEdge;
                                criticalDir = dir;
                                if (criticalEps <= branchEps)
                                {
                                    // found a tight (+, +) cross-tree edge to augment
                                    primalOperation = true;
                                    break;
                                }
                            }

                        }
                        else
                        {
                            // opposite node is an infinity node since all other trees contain only one
                            // "+" node
                            HandleInfinityEdgeInit(heap, currentEdge, dir, branchEps, criticalEps);
                        }
                    }
                    if (primalOperation)
                    {
                        // finish processing incident edges
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.slack += branchEps;
                        }
                        // exit the loop since we can perform shrink or augment operation
                        break;
                    }
                    else
                    {
                        /*
                         * Move currentNode to the next unprocessed "+" node in the tree, growing the
                         * tree if it is possible. Start a new branch if all nodes have been processed.
                         * Exit the loop if the slack of fibHeap.min().getData() is >= than the slack of
                         * critical edge (in this case we can perform primal operation after updating
                         * the duals).
                         */
                        if (currentNode.firstTreeChild != null)
                        {
                            // move to the next grandchild
                            currentNode = currentNode.firstTreeChild.GetOppositeMatched()!;
                        }
                        else
                        {
                            // try to find another unprocessed node
                            while (currentNode != branchRoot && currentNode!.treeSiblingNext == null)
                            {
                                currentNode = currentNode.GetTreeParent()!;
                            }
                            if (currentNode.IsMinusNode())
                            {
                                // found an unprocessed node
                                currentNode = currentNode.treeSiblingNext!.GetOppositeMatched()!;
                            }
                            else if (currentNode == branchRoot)
                            {
                                // we've processed all nodes in the current branch
                                BlossomVEdge? minSlackEdge = heap.IsEmpty() ? null : heap.FindMin().GetValue();

                                if (minSlackEdge == null || minSlackEdge.slack >= criticalEps)
                                {
                                    // can perform primal operation after updating duals
                                    if (DEBUG)
                                    {
                                        Console.WriteLine("Now current eps = " + criticalEps);
                                    }
                                    if (criticalEps > KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING_THRESHOLD)
                                    {
                                        throw new ArgumentException(KolmogorovWeightedPerfectMatching<V, E>.NO_PERFECT_MATCHING);
                                    }
                                    branchEps = criticalEps;
                                    break;
                                }
                                else
                                {
                                    // grow minimum slack edge
                                    if (DEBUG)
                                    {
                                        Console.WriteLine("Growing an edge " + minSlackEdge);
                                    }
                                    int dirToFreeNode = minSlackEdge.head[0].IsInfinityNode() ? 0 : 1;
                                    currentNode = minSlackEdge.head[1 - dirToFreeNode];

                                    BlossomVNode minusNode = minSlackEdge.head[dirToFreeNode];
                                    RemoveFromHeap(minusNode);
                                    minusNode.label = BlossomVNode.Label.MINUS;
                                    currentNode.AddChild(minusNode, minSlackEdge, true);
                                    branchEps = minSlackEdge.slack; // set new eps of the tree

                                    BlossomVNode plusNode = minusNode.GetOppositeMatched()!;
                                    if (plusNode.bestEdge != null)
                                    {
                                        RemoveFromHeap(plusNode);
                                    }
                                    plusNode.label = BlossomVNode.Label.PLUS;
                                    minusNode.AddChild(plusNode, minusNode.matched!, true);

                                    if (DEBUG)
                                    {
                                        Console.WriteLine("New branch root is " + plusNode + ", eps = " + branchEps);
                                    }
                                    // Start a new branch
                                    currentNode = branchRoot = plusNode;
                                }
                            }
                        }
                    }
                }

                // update duals
                UpdateDuals(heap, root, branchEps);

                // apply primal operation
                BlossomVNode from = criticalEdge!.head[1 - criticalDir];
                BlossomVNode to = criticalEdge.head[criticalDir];
                if (flag == Action.SHRINK)
                {
                    ShrinkInit(criticalEdge, root);
                }
                else
                {
                    AugmentBranchInit(root, from, criticalEdge);
                    if (to.isOuter)
                    {
                        // this node doesn't belong to a 1/2-values odd circuit
                        AugmentBranchInit(to, to, criticalEdge); // to is the root of the opposite tree
                    }
                    else
                    {
                        // this node belongs to a 1/2-values odd circuit
                        ExpandInit(to, criticalEdge);
                    }
                }

                root = root2;
                if (root != null && !root.isTreeRoot)
                {
                    root = root3;
                }
            }

            return Finish();
        }

        #endregion private methods

        #region classes

        /// <summary>
        /// Enum for specifying the primal operation to perform with critical edge during fractional
        /// matching initialization
        /// </summary>
        internal enum Action
        {
            NONE,
            SHRINK,
            AUGMENT
        }

        #endregion classes

    }
}
