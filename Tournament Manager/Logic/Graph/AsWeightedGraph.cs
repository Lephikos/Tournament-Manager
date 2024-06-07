using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Graph
{

    /// <summary>
    /// Provides a weighted view of a graph. The class stores edge weights internally.
    /// All <see cref="GetEdgeWeight(E)"/> calls are handled by this view; all other graph operations are
    /// propagated to the graph backing this view.<para/>
    /// 
    /// This class can be used to make an unweighted graph weighted, to override the weights of a
    /// weighted graph, or to provide different weighted views of the same underlying graph. For
    /// instance, the edges of a graph representing a road network might have two weights associated with
    /// them: a travel time and a travel distance. Instead of creating two weighted graphs of the same
    /// network, one would simply create two weighted views of the same underlying graph.<para/>
    /// 
    /// This class offers two ways to associate a weight with an edge:
    /// 
    /// <list type="bullet">
    /// <item><description>
    /// Explicitly through a map which contains a mapping from an edge to a weight
    /// </description></item>
    /// <item><description>
    /// Implicitly through a function which computes a weight for a given edge
    /// </description></item>
    /// </list>
    /// 
    /// In the first way, the map is used to lookup edge weights. In the second way, a function is
    /// provided to calculate the weight of an edge. If the map does not contain a particular edge, or
    /// the function does not provide a weight for a particular edge, the @link{getEdgeWeight} call is
    /// propagated to the backing graph. <para/>
    /// 
    /// Finally, the view provides a <see cref="SetEdgeWeight(E, double)"/> method. This method behaves differently
    /// depending on how the view is constructed. See <see cref="SetEdgeWeight(E, double)"/> for details.
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal class AsWeightedGraph<V, E> : GraphDelegator<V, E>, IGraph<V, E> where E : notnull
    {

        private readonly Func<E, double>? weightFunction;

        private readonly Dictionary<E, double> weights;

        private readonly bool writeWeightsThrough;

        private readonly bool cacheWeights;


        /// <summary>
        /// Constructor for AsWeightedGraph where the weights are provided through a map. Invocations of
        /// the <see cref="SetEdgeWeight(E, double)"/> method will update the map. Moreover, calls to 
        /// <see cref="SetEdgeWeight(E, double)"/> are propagated to the underlying graph.
        /// </summary>
        /// <param name="graph">the backing graph over which a weighted view is to be created.</param>
        /// <param name="weights">the map containing the edge weights.</param>
        public AsWeightedGraph(IGraph<V, E> graph, Dictionary<E, double> weights) : this(graph, weights, graph.GetGraphType().IsWeighted()) { }

        /// <summary>
        /// Constructor for AsWeightedGraph which allows weight write propagation to be requested
        /// explicitly.
        /// </summary>
        /// <param name="graph">the backing graph over which an weighted view is to be created</param>
        /// <param name="weights">the map containing the edge weights</param>
        /// <param name="writeWeightsThrough">if set to true, the weights will get propagated to the backing graph
        ///                                   in the <see cref="SetEdgeWeight(E, double)"/> method</param>
        /// <exception cref="ArgumentException">
        public AsWeightedGraph(IGraph<V, E> graph, Dictionary<E, double> weights, bool writeWeightsThrough) : base(graph)
        {
            this.weights = weights;
            weightFunction = null;
            cacheWeights = false;
            this.writeWeightsThrough = writeWeightsThrough;

            if (writeWeightsThrough && !graph.GetGraphType().IsWeighted())
            {
                throw new ArgumentException("Graph must be weighted");
            }
        }

        /// <summary>
        /// Constructor for AsWeightedGraph which uses a weight function to compute edge weights. When
        /// the weight of an edge is queried, the weight function is invoked. If
        /// <c>cacheWeights</c> is set to <c>true</c>, the weight of an edge returned by the
        /// <c>weightFunction</c> after its first invocation is stored in a map. The weight of an
        /// edge returned by subsequent calls to <see cref="GetEdgeWeight(E)"/> for the same edge will then be
        /// retrieved directly from the map, instead of re-invoking the weight function. If
        /// <c>cacheWeights</c> is set to <c>false</c>, each invocation of
        /// the <see cref="GetEdgeWeight(E)"/> method will invoke the weight function. Caching the edge weights is
        /// particularly useful when pre-computing all edge weights is expensive and it is expected that
        /// the weights of only a subset of all edges will be queried.
        /// </summary>
        /// <param name="graph">the backing graph over which an weighted view is to be created</param>
        /// <param name="weightFunction">function which maps an edge to a weight</param>
        /// <param name="cacheWeights">if set to <c>true</c>, weights are cached once computed by the weight function</param>
        /// <param name="writeWeightsThrough">if set to <c>true</c>, the weight set directly by
        ///                                   the <see cref="GetEdgeWeight(E)"/> method will be propagated to the backing graph.</param>
        /// <exception cref="ArgumentException"></exception>
        public AsWeightedGraph(IGraph<V, E> graph, Func<E, double> weightFunction, bool cacheWeights, bool writeWeightsThrough) : base(graph)
        {
            this.weightFunction = weightFunction;
            this.cacheWeights = cacheWeights;
            this.writeWeightsThrough = writeWeightsThrough;
            weights = new Dictionary<E, double>();

            if (writeWeightsThrough && !graph.GetGraphType().IsWeighted())
            {
                throw new ArgumentException("Graph must be weighted");
            }
        }




        /// <summary>
        /// Returns the weight assigned to a given edge. If weights are provided through a map, first a
        /// map lookup is performed. If the edge is not found, the <see cref="GetEdgeWeight(E)"/> method of the
        /// underlying graph is invoked instead. If, on the other hand, the weights are provided through
        /// a function, this method will first attempt to lookup the weight of an edge in the cache (that
        /// is, if <c>cacheWeights</c> is set to <c>true</c> in the constructor). If caching
        /// was disabled, or the edge could not be found in the cache, the weight function is invoked. If
        /// the function does not provide a weight for a given edge, the call is again propagated to the
        /// underlying graph.
        /// </summary>
        /// <param name="e">edge of interest</param>
        /// <returns>the edge weight</returns>
        public override double GetEdgeWeight(E e)
        {
            double weight = base.GetEdgeWeight(e);

            if (weightFunction != null)
            {
                if (!cacheWeights || cacheWeights && !weights.TryGetValue(e, out weight))
                {
                    weight = weightFunction(e);
                }
            }
            else
            {
                weights.TryGetValue(e, out weight);
            }

            return weight;
        }

        /// <summary>
        /// Assigns a weight to an edge. If <c>writeWeightsThrough</c> is set to <c>true</c>,
        /// the same weight is set in the backing graph. If this class was constructed using a weight
        /// function, it only makes sense to invoke this method when <c>cacheWeights</c> is set to
        /// true. This method can then be used to preset weights in the cache, or to overwrite existing
        /// values.
        /// </summary>
        /// <param name="e">edge on which to set weight</param>
        /// <param name="weight">new weight for edge</param>
        /// <exception cref="NotSupportedException"></exception>
        public override void SetEdgeWeight(E e, double weight)
        {
            if (weightFunction != null && !cacheWeights)
            {
                throw new NotSupportedException("Cannot set an edge weight when a weight function is used and caching is disabled");
            }

            weights[e] = weight;

            if (writeWeightsThrough)
            {
                GetDelegate().SetEdgeWeight(e, weight);
            }
        }

        public override IGraphType GetGraphType()
        {
            return base.GetGraphType().AsWeighted();
        }
    }
}
