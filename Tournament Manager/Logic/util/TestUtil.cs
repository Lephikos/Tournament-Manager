using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Tournament_Manager.Logic.Graph.cs;
using System.Windows.Forms.VisualStyles;

namespace Tournament_Manager.Logic.util
{

    /// <summary>
    /// Test related utility methods.
    /// </summary>
    internal class TestUtil
    {

        public static void ConstructGraph(IGraph<long, Pair<long, long>> graph, long[][] edges)
        {
            bool weighted = edges.Length > 0 && edges[0].Length > 2;
            foreach (long[] edge in edges)
            {
                Pair<long, long> graphEdge = Graphs.AddEdgeWithVertices(graph, edge[0], edge[1]);
                if (weighted)
                {
                    graph.SetEdgeWeight(graphEdge, edge[2]);
                }
            }
        }


        public static IGraph<long, Pair<long, long>> CreateUndirected(long[][] edges)
        {
            IGraph<long, Pair<long, long>> graph = new UndirectedSimpleGraph();

            ConstructGraph(graph, edges);

            return graph;
        }



    }
}
