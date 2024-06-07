using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Graph;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tournament_Manager.Logic.Matching
{

    /// <summary>
    /// A graph matching.
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal interface IMatching<V, E>
    {

        /// <summary>
        /// Returns the graph over which this matching is defined.
        /// </summary>
        /// <returns>the graph</returns>
        public IGraph<V, E> GetGraph();

        /// <summary>
        /// Returns the weight of the matching.
        /// </summary>
        /// <returns>the weight of the matching</returns>
        public double GetWeight();

        /// <summary>
        /// Get the edges of the matching.
        /// </summary>
        /// <returns>the edges of the matching</returns>
        public HashSet<E> GetEdges();

        /// <summary>
        /// Returns true if vertex v is incident to an edge in this matching.
        /// </summary>
        /// <param name="v">vertex</param>
        /// <returns>true if vertex v is incident to an edge in this matching.</returns>
        public bool IsMatched(V v)
        {
            HashSet<E> edges = GetEdges();
            return GetGraph().EdgesOf(v).Intersect(edges).Any();
        }

        /// <summary>
        /// Returns true if the matching is a perfect matching. A matching is perfect if every vertex
        /// in the graph is incident to an edge in the matching.
        /// </summary>
        /// <returns>true if the matching is perfect</returns>
        public bool IsPerfect()
        {
            return GetEdges().Count == GetGraph().VertexSet().Count / 2.0;
        }

    }

    internal class MatchingImpl<V, E> : IMatching<V, E>
    {

        /// <summary>
        /// graph on which the matching is defined
        /// </summary>
        private IGraph<V, E> graph;

        /// <summary>
        /// the edges of the matching
        /// </summary>
        private HashSet<E> edges;

        /// <summary>
        /// the weight of the matching
        /// </summary>
        private double weight;

        private HashSet<V>? matchedVertices = null;


        /// <summary>
        /// Construct a new instance
        /// </summary>
        /// <param name="graph">graph on which the matching is defined</param>
        /// <param name="edges">the edges of the matching</param>
        /// <param name="weight"></param>
        public MatchingImpl(IGraph<V, E> graph, HashSet<E> edges, double weight)
        {
            this.graph = graph;
            this.edges = edges;
            this.weight = weight;
        }


        public IGraph<V, E> GetGraph()
        {
            return graph;
        }

        public double GetWeight()
        {
            return weight;
        }

        public HashSet<E> GetEdges()
        {
            return edges;
        }

        public bool IsMatched(V v)
        {
            if (matchedVertices == null)
            { // lazily index the vertices that have been matched
                matchedVertices = new HashSet<V>();
                foreach (E e in edges)
                {
                    matchedVertices.Add(graph.GetEdgeSource(e));
                    matchedVertices.Add(graph.GetEdgeTarget(e));
                }
            }

            return matchedVertices.Contains(v);
        }

        public override string ToString()
        {
            return "Matching [edges=" + edges + ", weight=" + weight + "]";
        }

    }
}
