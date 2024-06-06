using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Graph.cs;

namespace Tournament_Manager.Logic.util
{

    /// <summary>
    /// Utility for Graphs
    /// </summary>
    internal abstract class Graphs
    {

        /// <summary>
        /// Adds the specified source and target vertices to the graph, if not already included, and
        /// creates a new edge and adds it to the specified graph
        /// </summary>
        /// <typeparam name="V">the graph for which the specified edge to be added</typeparam>
        /// <typeparam name="E">source vertex of the edge</typeparam>
        /// <param name="g">target vertex of the edge</param>
        /// <param name="sourceVertex">the graph vertex type</param>
        /// <param name="targetVertex">the graph edge type</param>
        /// <returns>The newly created edge if added to the graph, otherwise <c>null</c></returns>
        public static E AddEdgeWithVertices<V, E>(IGraph<V, E> g, V sourceVertex, V targetVertex)
        {
            g.AddVertex(sourceVertex);
            g.AddVertex(targetVertex);

            return g.AddEdge(sourceVertex, targetVertex)!;
        }

    }
}
