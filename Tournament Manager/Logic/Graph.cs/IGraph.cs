using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Logic.Graph.cs
{
    internal interface IGraph<V, E>
    {

        /// <summary>
        /// Returns a set of all edges connecting source vertex to target vertex if such vertices exist
        /// in this graph. If any of the vertices does not exist or is <c>null</c>, returns <c>null</c>.
        /// If both vertices exist but no edges found, returns an empty set.<para/>
        /// 
        /// In undirected graphs, some of the returned edges may have their source and target vertices in
        /// the opposite order. In simple graphs the returned set is either singleton set or empty set.
        /// </summary>
        /// <param name="sourceVertex">source vertex of the edge.</param>
        /// <param name="targetVertex">target vertex of the edge.</param>
        /// <returns>a set of all edges connecting source vertex to target vertex.</returns>
        public HashSet<E> GetAllEdges(V sourceVertex,  V targetVertex);

        /// <summary>
        /// Returns an edge connecting source vertex to target vertex if such vertices and such edge
        /// exist in this graph. Otherwise returns <c>null</c>. 
        /// If any of the specified vertices is <c>null</c> returns <c>null</c>.<para/>
        /// 
        /// In undirected graphs, the returned edge may have its source and target vertices in the
        /// opposite order.
        /// </summary>
        /// <param name="sourceVertex">source vertex of the edge.</param>
        /// <param name="targetVertex">target vertex of the edge.</param>
        /// <returns>an edge connecting source vertex to target vertex.</returns>
        public E GetEdge(V sourceVertex, V targetVertex);

        /// <summary>
        /// Creates a new edge in this graph, going from the source vertex to the target vertex, and
        /// returns the created edge. Some graphs do not allow edge-multiplicity. In such cases, if the
        /// graph already contains an edge from the specified source to the specified target, then this
        /// method does not change the graph and returns <c>null</c>.<para/>
        /// 
        /// The source and target vertices must already be contained in this graph. If they are not found
        /// in graph <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="sourceVertex">source vertex of the edge.</param>
        /// <param name="targetVertex">target vertex of the edge.</param>
        /// <returns>The newly created edge if added to the graph, otherwise <c>null</c></returns>
        public E AddEdge(V sourceVertex, V targetVertex);

        /// <summary>
        /// Creates a new vertex in this graph and returns it.
        /// </summary>
        /// <returns>The newly created vertex if added to the graph.</returns>
        public V AddVertex();


        /// <summary>
        /// Returns <c>true</c> if and only if this graph contains an edge going from the source
        /// vertex to the target vertex. In undirected graphs the same result is obtained when source and
        /// target are inverted. If any of the specified vertices does not exist in the graph, or if is
        /// <c>null</c>, returns <c>false</c>.
        /// </summary>
        /// <param name="sourceVertex">source vertex of the edge.</param>
        /// <param name="targetVertex">target vertex of the edge.</param>
        /// <returns><see langword="true"/> if this graph contains the specified edge.</returns>
        public bool ContainsEdge(V sourceVertex, V targetVertex);

        /// <summary>
        /// Returns <c>true</c> if this graph contains the specified edge. More formally, returns
        /// <c>true</c> if and only if this graph contains an edge <c>e2</c> such that
        /// <c>e.equals(e2)</c>. If the specified edge is <c>null</c> returns
        /// <c>false</c>.
        /// </summary>
        /// <param name="edge">edge whose presence in this graph is to be tested.</param>
        /// <returns><see langword="true"/> if this graph contains the specified edge.</returns>
        public bool ContainsEdge(E edge);

        /// <summary>
        /// Returns <c>true</c> if this graph contains the specified vertex. More formally, returns
        /// <c>true</c> if and only if this graph contains a vertex <c>u</c> such that
        /// <c>u.equals(v)</c>. If the specified vertex is <c>null</c> returns
        /// <c>false</c>.
        /// </summary>
        /// <param name="v">vertex whose presence in this graph is to be tested.</param>
        /// <returns><see langword="true"/> if this graph contains the specified vertex.</returns>
        public bool ContainsVertex(V v);

        /// <summary>
        /// Returns a set of the edges contained in this graph. The set is backed by the graph, so
        /// changes to the graph are reflected in the set. If the graph is modified while an iteration
        /// over the set is in progress, the results of the iteration are undefined.
        /// </summary>
        /// <returns>a set of the edges contained in this graph.</returns>
        public HashSet<E> EdgeSet();

        /// <summary>
        /// Returns a set of the vertices contained in this graph. The set is backed by the graph, so
        /// changes to the graph are reflected in the set. If the graph is modified while an iteration
        /// over the set is in progress, the results of the iteration are undefined.
        /// </summary>
        /// <returns>a set view of the vertices contained in this graph.</returns>
        public HashSet<V> VertexSet();


        /// <summary>
        /// Returns the degree of the specified vertex.<para/>
        /// 
        /// A degree of a vertex in an undirected graph is the number of edges touching that vertex.
        /// Edges with same source and target vertices (self-loops) are counted twice.<para/>
        /// 
        /// In directed graphs this method returns the sum of the "in degree" and the "out degree".
        /// </summary>
        /// <param name="v">vertex whose degree is to be calculated.</param>
        /// <returns>the degree of the specified vertex.</returns>
        public int DegreeOf(V v);

        /// <summary>
        /// Returns a set of all edges touching the specified vertex. If no edges are touching the
        /// specified vertex returns an empty set.
        /// </summary>
        /// <param name="v">the vertex for which a set of touching edges is to be returned.</param>
        /// <returns>a set of all edges touching the specified vertex.</returns>
        public HashSet<E> EdgesOf(V v);

        /// <summary>
        /// Returns the "in degree" of the specified vertex.<para/>
        /// 
        /// The "in degree" of a vertex in a directed graph is the number of inward directed edges from
        /// that vertex.<para/>
        /// 
        /// In the case of undirected graphs this method returns the number of edges touching the vertex.
        /// Edges with same source and target vertices (self-loops) are counted twice.
        /// </summary>
        /// <param name="v">vertex whose degree is to be calculated.</param>
        /// <returns>the degree of the specified vertex.</returns>
        public int InDegreeOf(V v);

        /// <summary>
        /// Returns a set of all edges incoming into the specified vertex.<para/>
        /// In the case of undirected graphs this method returns all edges touching the vertex, thus,
        /// some of the returned edges may have their source and target vertices in the opposite order.
        /// </summary>
        /// <param name="v">the vertex for which the list of incoming edges to be returned.</param>
        /// <returns>a set of all edges incoming into the specified vertex.</returns>
        public HashSet<E> IncomingEdgesOf(V v);

        /// <summary>
        /// Returns the "out degree" of the specified vertex.<para/>
        /// 
        /// The "out degree" of a vertex in a directed graph is the number of outward directed edges from
        /// that vertex.<<para/>
        /// 
        /// In the case of undirected graphs this method returns the number of edges touching the vertex.
        /// Edges with same source and target vertices (self-loops) are counted twice.
        /// </summary>
        /// <param name="v">vertex whose degree is to be calculated.</param>
        /// <returns>the degree of the specified vertex.</returns>
        public int OutDegreeOf(V v);

        /// <summary>
        /// Returns a set of all edges outgoing from the specified vertex.<para/>
        /// 
        /// In the case of undirected graphs this method returns all edges touching the vertex, thus,
        /// some of the returned edges may have their source and target vertices in the opposite order.
        /// </summary>
        /// <param name="v">the vertex for which the list of outgoing edges to be returned.</param>
        /// <returns>a set of all edges outgoing from the specified vertex.</returns>
        public HashSet<E> OutgoingEdgesOf(V v);


        /// <summary>
        /// Removes all the edges going from the specified source vertex to the specified target vertex,
        /// and returns a set of all removed edges. Returns <c>null</c> if any of the specified
        /// vertices does not exist in the graph. If both vertices exist but no edge is found, returns an
        /// empty set. This method will either invoke the <see  cref="RemoveEdge(E)"/> method, or the 
        /// <see cref="RemoveEdge(V, V)"/> method.
        /// </summary>
        /// <param name="sourceVertex">source vertex of the edge.</param>
        /// <param name="targetVertex">target vertex of the edge.</param>
        /// <returns>the removed edges, or <c>null</c> if either vertex is not part of graph</returns>
        public HashSet<E>? RemoveAllEdges(V sourceVertex, V targetVertex);

        /// <summary>
        /// Removes an edge going from source vertex to target vertex, if such vertices and such edge
        /// exist in this graph. Returns the edge if removed or <c>null</c> otherwise.
        /// </summary>
        /// <param name="sourceVertex">source vertex of the edge.</param>
        /// <param name="targetVertex">target vertex of the edge.</param>
        /// <returns>The removed edge, or <c>null</c> if no edge removed.</returns>
        public E RemoveEdge(V sourceVertex, V targetVertex);

        /// <summary>
        /// Removes all the edges in this graph that are also contained in the specified edge collection.
        /// After this call returns, this graph will contain no edges in common with the specified edges.
        /// This method will invoke the <see cref="RemoveEdge(E)"/> method.
        /// </summary>
        /// <param name="edges">edges edges to be removed from this graph.</param>
        /// <returns><see langword="true"/> if this graph changed as a result of the call</returns>
        public bool RemoveAllEdges(HashSet<E> edges);

        /// <summary>
        /// Removes the specified edge from the graph. Removes the specified edge from this graph if it
        /// is present. More formally, removes an edge <c>e2</c> such that <c>e2.equals(e)</c>,
        /// if the graph contains such edge. Returns <c>true</c> if the graph contained the specified edge.
        /// (The graph will not contain the specified edge once the call returns).<para/>
        /// 
        /// If the specified edge is <c>null</c> returns <c>false</c>.
        /// </summary>
        /// <param name="e">edge to be removed from this graph, if present.</param>
        /// <returns><see langword="true"/> if and only if the graph contained the specified edge.</returns>
        public bool RemoveEdge(E e);

        /// <summary>
        /// Removes all the vertices in this graph that are also contained in the specified vertex
        /// collection. After this call returns, this graph will contain no vertices in common with the
        /// specified vertices. This method will invoke the <see cref="RemoveVertex(V)"/> method.
        /// </summary>
        /// <param name="vertices">vertices to be removed from this graph.</param>
        /// <returns><see langword="true"/> if this graph changed as a result of the call</returns>
        public bool RemoveAllVertices(HashSet<V> vertices);

        /// <summary>
        /// Removes the specified vertex from this graph including all its touching edges if present.
        /// More formally, if the graph contains a vertex <c>u</c> such that <c>u.equals(v)</c>, 
        /// the call removes all edges that touch <c>u</c> and then removes <c>u</c> itself.
        /// If no such <c>u</c> is found, the call leaves the graph unchanged.
        /// Returns <c>true</c> if the graph contained the specified vertex.
        /// (The graph will not contain the specified vertex once the call returns).ypara/>
        /// 
        /// If the specified vertex is <c>null</c> returns <c>false</c>.
        /// </summary>
        /// <param name="v">vertex to be removed from this graph, if present.</param>
        /// <returns><see langword="true"/> if the graph contained the specified vertex; 
        /// <see langword="false"/> otherwise.</returns>
        public bool RemoveVertex(V v);


        /// <summary>
        /// Returns the source vertex of an edge. For an undirected graph, source and target are
        /// distinguishable designations (but without any mathematical meaning).
        /// </summary>
        /// <param name="e">edge of interest</param>
        /// <returns>source vertex</returns>
        public V GetEdgeSource(E e);

        /// <summary>
        /// Returns the target vertex of an edge. For an undirected graph, source and target are
        /// distinguishable designations (but without any mathematical meaning).
        /// </summary>
        /// <param name="e">edge of interest</param>
        /// <returns>target vertex</returns>
        public V GetEdgeTarget(E e);

        /// <summary>
        /// Get the graph type. The graph type can be used to query for additional metadata such as
        /// whether the graph supports directed or undirected edges, self-loops, multiple (parallel)
        /// edges, weights, etc.
        /// </summary>
        /// <returns>the graph type</returns>
        public IGraphType GetGraphType();


        /// <summary>
        /// The default weight for an edge.
        /// </summary>
        static double DEFAULT_EDGE_WEIGHT = 1.0;

        /// <summary>
        /// Returns the weight assigned to a given edge. Unweighted graphs return 1.0 (as defined by
        /// <see cref="DEFAULT_EDGE_WEIGHT"/>), allowing weighted-graph algorithms to apply to them when
        /// meaningful.
        /// </summary>
        /// <param name="e">edge of interest</param>
        /// <returns>edge weight</returns>
        public double GetEdgeWeight(E e);

        /// <summary>
        /// Assigns a weight to an edge.
        /// </summary>
        /// <param name="e">edge on which to set weight</param>
        /// <param name="weight">weight new weight for edge</param>
        public void SetEdgeWeight(E e, double weight);

        /// <summary>
        /// Assigns a weight to an edge between <code>sourceVertex</code> and <code>targetVertex</code>.
        /// </summary>
        /// <param name="sourceVertex">source vertex of the edge</param>
        /// <param name="targetVertex">target vertex of the edge</param>
        /// <param name="weight">new weight for edge</param>
        public void SetEdgeWeight(V sourceVertex, V targetVertex,  double weight)
        {
            this.SetEdgeWeight(this.GetEdge(sourceVertex, targetVertex), weight);
        }

    }
}
