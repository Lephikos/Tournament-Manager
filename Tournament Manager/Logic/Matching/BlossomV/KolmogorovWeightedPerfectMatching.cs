using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Tournament_Manager.Logic.Graph.cs;

namespace Tournament_Manager.Logic.Matching.BlossomV
{
    internal class KolmogorovWeightedPerfectMatching<V, E> : IMatchingAlgorithm<V, E> where E : notnull where V : notnull
    {

        #region member

        internal static readonly bool DEBUG = true;

        public static readonly double EPS = IMatchingAlgorithm<V, E>.DEFAULT_EPSILON;

        public static readonly double INFINITY = 1e100;

        public static readonly double NO_PERFECT_MATCHING_THRESHOLD = 1e10;

        private static readonly string NO_PERFECT_MATCHING = "There is no perfect matching in the specified graph";

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
                
                


            }


        }

        #endregion public methods

        #region private methods

        private void LazyComputeWeightedPerfectMatching()
        {

        }

        private DualSolution<V, E> LazyComputeDualSolution()
        {
            return null;
        }

        private double TestNonNegativity()
        {
            return 0.0;
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
            internal Dictionary<W, double> dualVariables;

            /// <summary>
            /// Constructs a new solution for the dual linear program
            /// </summary>
            /// <param name="graph">the graph on which the linear program is formulated</param>
            /// <param name="dualVariables">the mapping from sets of vertices of odd cardinality to their dual variables</param>
            public DualSolution(IGraph<W, F> graph, Dictionary<W, double> dualVariables)
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
            public Dictionary<W, double> GetDualVariables()
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
