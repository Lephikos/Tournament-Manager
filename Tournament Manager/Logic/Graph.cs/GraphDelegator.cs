using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Graph.cs
{

    /// <summary>
    /// A graph backed by the the graph specified at the constructor, which delegates all its methods to
    /// the backing graph. Operations on this graph "pass through" to the to the backing graph. Any
    /// modification made to this graph or the backing graph is reflected by the other.<para/>
    /// 
    /// This graph does <i>not</i> pass the hashCode and equals operations through to the backing graph,
    /// but relies on <c>Object</c>'s <c>Equals</c> and <c>GetHashCode</c> methods.<para/>
    /// 
    /// This class is mostly used as a base for extending subclasses.
    /// </summary>
    /// <typeparam name="V">the graph vertex type</typeparam>
    /// <typeparam name="E">the graph edge type</typeparam>
    internal class GraphDelegator<V, E> : AbstractGraph<V, E>, IGraph<V, E>
    {

        /// <summary>
        /// The graph to which operations are delegated.
        /// </summary>
        private readonly IGraph<V, E> delegateGraph;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graph">the backing graph (the delegate).</param>
        public GraphDelegator(IGraph<V, E> graph)
        {
            this.delegateGraph = graph;
        }


        public override HashSet<E> GetAllEdges(V sourceVertex, V targetVertex)
        {
            return delegateGraph.GetAllEdges(sourceVertex, targetVertex);
        }

        public override E GetEdge(V sourceVertex, V targetVertex)
        {
            return delegateGraph.GetEdge(sourceVertex, targetVertex);
        }

        public override E AddEdge(V sourceVertex, V targetVertex)
        {
            return delegateGraph.AddEdge(sourceVertex, targetVertex);
        }

        public override V AddVertex()
        {
            return delegateGraph.AddVertex();
        }


        public override bool ContainsEdge(E edge)
        {
            return delegateGraph.ContainsEdge(edge);
        }

        public override bool ContainsVertex(V v)
        {
            return delegateGraph.ContainsVertex(v);
        }

        public override HashSet<E> EdgeSet()
        {
            return delegateGraph.EdgeSet(); 
        }

        public override HashSet<V> VertexSet()
        {
           return delegateGraph.VertexSet(); 
        }

        public override int DegreeOf(V v)
        {
            return delegateGraph.DegreeOf(v);
        }

        public override HashSet<E> EdgesOf(V v)
        {
            return delegateGraph.EdgesOf(v);
        }

        public override int InDegreeOf(V v)
        {
            return delegateGraph.InDegreeOf(v);
        }

        public override HashSet<E> IncomingEdgesOf(V v)
        {
            return delegateGraph.IncomingEdgesOf(v);
        }

        public override int OutDegreeOf(V v)
        {
            return delegateGraph.OutDegreeOf(v);
        }

        public override HashSet<E> OutgoingEdgesOf(V v)
        {
            return delegateGraph.OutgoingEdgesOf(v);
        }


        public override bool RemoveEdge(E e)
        {
            return delegateGraph.RemoveEdge(e);
        }

        public override E RemoveEdge(V sourceVertex, V targetVertex)
        {
            return delegateGraph.RemoveEdge(sourceVertex, targetVertex);
        }

        public override bool RemoveVertex(V v)
        {
            return delegateGraph.RemoveVertex(v);
        }


        public override V GetEdgeSource(E e)
        {
            return delegateGraph.GetEdgeSource(e);
        }

        public override V GetEdgeTarget(E e)
        {
            return delegateGraph.GetEdgeTarget(e);
        }


        public override IGraphType GetGraphType()
        {
            return delegateGraph.GetGraphType();
        }


        public override double GetEdgeWeight(E e)
        {
            return delegateGraph.GetEdgeWeight(e);
        }

        public override void SetEdgeWeight(E e, double weight)
        {
            delegateGraph.SetEdgeWeight(e, weight);
        }


        /// <summary>
        /// Return the backing graph (the delegate).
        /// </summary>
        /// <returns>the backing graph (the delegate)</returns>
        protected IGraph<V, E> GetDelegate()
        {
            return delegateGraph; 
        }
    }
}
