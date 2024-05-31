using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Graph.cs;

namespace Tournament_Manager.Logic.Matching.BlossomV
{

    /// <summary>
    /// This class stores data needed for the Kolmogorov's Blossom V algorithm; it is used by
    /// <see cref="KolmogorovWeightedPerfectMatching{V, E}"/>, <see cref="BlossomVPrimalUpdater{V, E}"/> and
    /// <see cref="BlossomVDualUpdater{V, E}"/> during the course of the algorithm.<para/>
    /// 
    /// We refer to this object with all the data stored in nodes, edges, trees, and tree edges as the
    /// state of the algorithm
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal class BlossomVState<V, E>
    {

        /// <summary>
        /// Number of nodes in the graph
        /// </summary>
        internal readonly int nodeNum;

        /// <summary>
        /// Number of edges in the graph
        /// </summary>
        internal readonly int edgeNum;

        /// <summary>
        /// The graph for which to find a matching
        /// </summary>
        internal IGraph<V, E> graph;

        /// <summary>
        /// An array of nodes of the graph.<para/>
        /// 
        /// The size of the array is nodeNum + 1. The node nodes[nodeNum] is an auxiliary
        /// node that is used as the first element in the linked list of tree roots
        /// </summary>
        internal BlossomVNode[] nodes;

        /// <summary>
        /// An array of edges of the graph
        /// </summary>
        internal BlossomVEdge[] edges;

        /// <summary>
        /// Number of trees
        /// </summary>
        internal int treeNum;

        /// <summary>
        /// Number of expanded blossoms
        /// </summary>
        internal int removedNum;

        /// <summary>
        /// Number of blossoms
        /// </summary>
        internal int blossomNum;

        /// <summary>
        /// Statistics of the algorithm performance
        /// </summary>
        internal KolmogorovWeightedPerfectMatching<V, E>.Statistics statistics;

        /// <summary>
        /// BlossomVOptions used to determine the strategies used in the algorithm
        /// </summary>
        internal BlossomVOptions options;

        /// <summary>
        /// Initial generic vertices of the graph
        /// </summary>
        internal List<V> graphVertices;

        /// <summary>
        /// Initial edges of the graph
        /// </summary>
        internal List<E> graphEdges;

        /// <summary>
        /// Minimum edge weight in the graph
        /// </summary>
        internal double minEdgeWeight;


        /// <summary>
        /// Constructs the algorithm's initial state
        /// </summary>
        /// <param name="graph">the graph for which to find a matching</param>
        /// <param name="nodes">nodes used in the algorithm</param>
        /// <param name="edges">edges used in the algorithm</param>
        /// <param name="nodeNum">number of nodes in the graph</param>
        /// <param name="edgeNum">number of edges in the graph</param>
        /// <param name="treeNum">number of trees in the graph</param>
        /// <param name="graphVertices">generic vertices of the <c>graph</c> in the same order as nodes in <c>nodes</c></param>
        /// <param name="graphEdges">generic edges of the <c>graph</c> in the same order as edges in <c>edges</c></param>
        /// <param name="options">default or user defined options</param>
        /// <param name="minEdgeWeight">minimum edge weight in the graph</param>
        public BlossomVState(IGraph<V, E> graph, BlossomVNode[] nodes, BlossomVEdge[] edges, int nodeNum, int edgeNum,
            int treeNum, List<V> graphVertices, List<E> graphEdges, BlossomVOptions options, double minEdgeWeight)
        {
            this.graph = graph;
            this.nodes = nodes;
            this.edges = edges;
            this.nodeNum = nodeNum;
            this.edgeNum = edgeNum;
            this.treeNum = treeNum;
            this.graphVertices = graphVertices;
            this.graphEdges = graphEdges;
            this.options = options;
            this.statistics = new KolmogorovWeightedPerfectMatching<V, E>.Statistics();
            this.minEdgeWeight = minEdgeWeight;
        }

    }
}
