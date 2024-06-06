using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Graph.cs
{
    internal abstract class AbstractGraph<V, E > : IGraph<V, E>
    {

        protected AbstractGraph() { }
         


        public abstract HashSet<E>? GetAllEdges(V sourceVertex, V targetVertex);

        public abstract E? GetEdge(V sourceVertex, V targetVertex);

        public abstract E? AddEdge(V sourceVertex, V targetVertex);

        public abstract V? AddVertex();

        public abstract bool AddVertex(V v);


        public bool ContainsEdge(V sourceVertex, V targetVertex)
        {
            return GetEdge(sourceVertex, targetVertex) != null;
        }

        public abstract bool ContainsEdge(E edge);

        public abstract bool ContainsVertex(V v);

        public abstract HashSet<E> EdgeSet();

        public abstract HashSet<V> VertexSet();


        public abstract int DegreeOf(V v);

        public abstract HashSet<E> EdgesOf(V v);

        public abstract int InDegreeOf(V v);

        public abstract HashSet<E> IncomingEdgesOf(V v);

        public abstract int OutDegreeOf(V v);

        public abstract HashSet<E> OutgoingEdgesOf(V v);


        public HashSet<E>? RemoveAllEdges(V sourceVertex, V targetVertex)
        {
            HashSet<E>? removed = GetAllEdges(sourceVertex, targetVertex);
            if (removed == null)
            {
                return null;
            }
            RemoveAllEdges(removed);

            return removed;
        }

        public abstract E? RemoveEdge(V sourceVertex, V targetVertex);

        public bool RemoveAllEdges(HashSet<E> edges)
        {
            bool modified = false;

            foreach (E e in edges)
            {
                modified |= RemoveEdge(e);
            }
            return modified;
        }

        public abstract bool RemoveEdge(E e);

        public bool RemoveAllVertices(HashSet<V> vertices)
        {
            bool modified = false;

            foreach (V v in vertices)
            {
                modified |= RemoveVertex(v);
            }

            return modified;
        }

        public abstract bool RemoveVertex(V v);


        public abstract V GetEdgeSource(E e);

        public abstract V GetEdgeTarget(E e);


        public abstract IGraphType GetGraphType();

        public abstract double GetEdgeWeight(E e);

        public abstract void SetEdgeWeight(E e, double weight);


        /// <summary>
        /// Returns a hash code value for this graph. The hash code of a graph is defined to be the sum
        /// of the hash codes of vertices and edges in the graph. It is also based on graph topology and
        /// edges weights.
        /// </summary>
        /// <returns>the hash code value this graph</returns>
        public override int GetHashCode()
        {
            int hash = VertexSet().GetHashCode();

            bool isDirected = GetGraphType().IsDirected();

            foreach (E e in EdgeSet())
            {
                int part = e!.GetHashCode();
                int target = GetEdgeTarget(e)!.GetHashCode();
                int pairing = GetEdgeSource(e)!.GetHashCode() + target;

                if (isDirected)
                {
                    pairing = ((pairing) * (pairing + 1) / 2) + target;
                }

                part = (31 * part) + pairing;
                part = (31 * part) + GetEdgeWeight(e).GetHashCode();

                hash += part;
            }

            return hash;
        }

        /// <summary>
        /// Indicates whether some other object is "equal to" this graph. Returns <c>true</c> if
        /// the given object is also a graph, the two graphs are instances of the same graph class, have
        /// identical vertices and edges sets with the same weights.
        /// </summary>
        /// <param name="obj">object to be compared for equality with this graph</param>
        /// <returns><see langword="true"/> if the specified object is equal to this graph</returns>
        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }
            if ((obj == null) || (obj.GetType() != this.GetType()))
            {
                return false;
            }

            IGraph<V, E> g = (IGraph<V, E>) obj;

            if (!VertexSet().Equals(g.VertexSet()) || !EdgeSet().Equals(g.EdgeSet())) {
                return false;
            }

            bool isDirected = GetGraphType().IsDirected();

            foreach (E e in EdgeSet())
            {
                if (!g.ContainsEdge(e))
                {
                    return false;
                }

                V source = GetEdgeSource(e)!;
                V target = GetEdgeTarget(e)!;
                V gSource = g.GetEdgeSource(e)!;
                V gTarget = g.GetEdgeTarget(e)!;

                if (isDirected && (!gSource.Equals(source) || !gTarget.Equals(target)))
                {
                    return false;
                } 
                else if (!isDirected && 
                    (!gSource.Equals(source) || !gTarget.Equals(target)) &&
                    ((!gSource.Equals(target) || !gTarget.Equals(source))))
                {
                    return false;
                }

                if (this.GetEdgeWeight(e).CompareTo(g.GetEdgeWeight(e)) != 0)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
