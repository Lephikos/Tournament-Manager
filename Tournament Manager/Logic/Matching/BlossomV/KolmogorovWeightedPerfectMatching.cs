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
    internal class KolmogorovWeightedPerfectMatching<V, E> : IMatchingAlgorithm<V, E> where E : notnull
    {

        #region member

        private static readonly bool DEBUG = true;

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

            for (int i = 0; i < state.graphEdges.Count; i++)
            {

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

        }

        #endregion private methods

    }
}
