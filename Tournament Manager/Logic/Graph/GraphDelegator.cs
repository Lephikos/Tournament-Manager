using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Graph
{
    internal class GraphDelegator<V, E> : AbstractGraph<V, E>, IGraph<V, E>
    {

        private readonly IGraph<V, E> delegateGraph;

        public GraphDelegator(IGraph<V, E> graph)
        {
            delegateGraph = graph;
        }

        public override HashSet<E>? GetAllEdges(V sourceVertex, V targetVertex)
        {
            return delegateGraph.GetAllEdges(sourceVertex, targetVertex);
        }

        public override E? GetEdge(V sourceVertex, V targetVertex)
        {
            return delegateGraph.GetEdge(sourceVertex, targetVertex);
        }

        public override E? AddEdge(V sourceVertex, V targetVertex)
        {
            return delegateGraph.AddEdge(sourceVertex, targetVertex);
        }

        public override V? AddVertex()
        {
            return delegateGraph.AddVertex();
        }

        public override bool AddVertex(V v)
        {
            return delegateGraph.AddVertex(v);
        }

        public override bool ContainsEdge(E e)
        {
            return delegateGraph.ContainsEdge(e);
        }

        public override bool ContainsVertex(V v)
        {
            return delegateGraph.ContainsVertex(v);
        }

        public override int DegreeOf(V v)
        {
            return delegateGraph.DegreeOf(v);
        }

        public override HashSet<E> EdgeSet() { 
            return delegateGraph.EdgeSet(); 
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
            return OutDegreeOf(v);
        }

        public override HashSet<E> OutgoingEdgesOf(V v)
        {
            return delegateGraph.OutgoingEdgesOf(v);
        }

        public override bool RemoveEdge(E e)
        {
            return delegateGraph.RemoveEdge(e);
        }

        public override E? RemoveEdge(V sourceVertex, V targetVertex)
        {
            return delegateGraph.RemoveEdge(sourceVertex, targetVertex);
        }

        public override bool RemoveVertex(V v)
        {
            return delegateGraph.RemoveVertex(v);
        }

        public override string? ToString()
        {
            return delegateGraph.ToString();
        }

        public override HashSet<V> VertexSet()
        {
            return delegateGraph.VertexSet();
        }

        public override V GetEdgeSource(E e)
        {
            return delegateGraph.GetEdgeSource(e);
        }

        public override V GetEdgeTarget(E e)
        {
            return delegateGraph.GetEdgeTarget(e);
        }

        public override double GetEdgeWeight(E e)
        {
            return delegateGraph.GetEdgeWeight(e);
        }

        public override void SetEdgeWeight(E e, double weight)
        {
            delegateGraph.SetEdgeWeight(e, weight);
        }

        public override IGraphType GetGraphType()
        {
            return delegateGraph.GetGraphType();
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
