using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.util;

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
    internal class UndirectedSimpleGraph : AbstractGraph<long, Pair<long, long>>, IGraph<long, Pair<long, long>>
    {

        private HashSet<long> vertices = new HashSet<long>();

        private HashSet<Pair<long, long>> edges = new HashSet<Pair<long, long>>();

        private long index = 0;

        private IGraphType graphType = new GraphType(false, false);


        public override HashSet<Pair<long, long>>? GetAllEdges(long sourceVertex, long targetVertex)
        {
            if (!vertices.Contains(sourceVertex) || !vertices.Contains(targetVertex))
            {
                return null;
            }

            return edges.Where(p => (p.GetFirst() == sourceVertex && p.GetSecond() == targetVertex)
                                 || (p.GetFirst() == targetVertex && p.GetSecond() == sourceVertex)).ToHashSet<Pair<long, long>>();
        }

        public override Pair<long, long>? GetEdge(long sourceVertex, long targetVertex)
        {
            if (!vertices.Contains(sourceVertex) || !vertices.Contains(targetVertex))
            {
                return null;
            }

            return edges.FirstOrDefault(p => (p.GetFirst() == sourceVertex && p.GetSecond() == targetVertex)
                                          || (p.GetFirst() == targetVertex && p.GetSecond() == sourceVertex));
        }

        public override Pair<long, long>? AddEdge(long sourceVertex, long targetVertex)
        {
            if (!vertices.Contains(sourceVertex) || !vertices.Contains(targetVertex))
            {
                throw new ArgumentException("Vertex not found");
            }

            if (GetEdge(sourceVertex, targetVertex) != null) //Edge already exists
            {
                return null;
            }

            Pair<long, long> edge = new Pair<long, long>(sourceVertex, targetVertex);
            edges.Add(edge);

            return edge;
        }

        public override long AddVertex()
        {
            //TODO
            while (vertices.Count < int.MaxValue && vertices.Contains(index))
            {
                index++;
            }

            long vertex = index;
            vertices.Add(vertex);

            return vertex;
        }

        public override bool AddVertex(long v)
        {
            if (vertices.Contains(v))
            {
                return false;
            }

            vertices.Add(v);
            return true;
        }

        public override bool ContainsEdge(Pair<long, long> edge)
        {
            return GetEdge(edge.GetFirst(), edge.GetSecond()) == null ? false : true;
        }

        public override bool ContainsVertex(long v)
        {
            return vertices.Contains(v);
        }

        public override HashSet<Pair<long, long>> EdgeSet()
        {
            return edges;
        }

        public override HashSet<long> VertexSet()
        {
            return vertices;
        }

        public override int DegreeOf(long v)
        {
            return edges.Where(edge => edge.GetFirst() == v || edge.GetSecond() == v).Count();
        }

        public override HashSet<Pair<long, long>> EdgesOf(long v)
        {
            return edges.Where(edge => edge.GetFirst() == v || edge.GetSecond() == v).ToHashSet();
        }

        public override int InDegreeOf(long v)
        {
            return DegreeOf(v);
        }

        public override HashSet<Pair<long, long>> IncomingEdgesOf(long v)
        {
            return EdgesOf(v);
        }

        public override int OutDegreeOf(long v)
        {
            return DegreeOf(v);
        }

        public override HashSet<Pair<long, long>> OutgoingEdgesOf(long v)
        {
            return EdgesOf(v);
        }


        public override bool RemoveEdge(Pair<long, long> e)
        {
            if (e == null)
            {
                return false;
            }

            if (ContainsEdge(e))
            {
                edges.RemoveWhere(edge => (edge.GetFirst() == e.GetFirst() && edge.GetSecond() == e.GetSecond())
                                        || (edge.GetFirst() == e.GetSecond() && edge.GetSecond() == e.GetFirst()));
                return true;
            } else
            {
                return false;
            }
        }

        public override Pair<long, long>? RemoveEdge(long sourceVertex, long targetVertex)
        {
            if (!vertices.Contains(sourceVertex) || !vertices.Contains(targetVertex))
            {
                return null;
            }

            if (ContainsEdge(sourceVertex, targetVertex))
            {
                Pair<long, long> edge = GetEdge(sourceVertex, targetVertex)!;

                RemoveEdge(edge);

                return edge;

            } else
            {
                return null;
            }
        }

        public override bool RemoveVertex(long v)
        {
            if (vertices.Contains(v))
            {
                HashSet<Pair<long, long>> toRemove = edges.Where(edge => edge.GetFirst() == v || edge.GetSecond() == v).ToHashSet();
                RemoveAllEdges(toRemove);
                vertices.Remove(v);

                return true;
            }
            return false;
        }


        public override long GetEdgeSource(Pair<long, long> e)
        {
            return e.GetFirst();
        }

        public override long GetEdgeTarget(Pair<long, long> e)
        {
            return e.GetSecond();
        }


        public override IGraphType GetGraphType()
        {
            return graphType;
        }


        public override double GetEdgeWeight(Pair<long, long> e)
        {
            return 1.0;
        }

        public override void SetEdgeWeight(Pair<long, long> e, double weight)
        {
            
        }

    }
}
