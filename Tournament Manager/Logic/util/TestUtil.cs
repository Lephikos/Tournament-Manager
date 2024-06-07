using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Forms.VisualStyles;
using Tournament_Manager.Logic.Graph;

namespace Tournament_Manager.Logic.util
{

    /// <summary>
    /// Test related utility methods.
    /// </summary>
    internal class TestUtil
    {

        public static Pair<IGraph<long, Pair<long, long>>, Dictionary<Pair<long, long>, double>> ConstructGraph(long[,] edges)
        {
            UndirectedSimpleGraph graph = new UndirectedSimpleGraph();
            Dictionary<Pair<long, long>, double> weights = new Dictionary<Pair<long, long>, double>();

            for (int i = 0; i < edges.GetLength(0); i++)
            {
                Pair<long, long> e = AddEdgeWithVertices(graph, edges[i,0], edges[i,1]);
                weights.Add(e, edges[i,2]);
            }

            return new Pair<IGraph<long, Pair<long, long>>, Dictionary<Pair<long, long>, double>>
                (new AsWeightedGraph<long, Pair<long, long>>(graph, weights, false), weights);
        }

        public static Pair<long, long> AddEdgeWithVertices(IGraph<long, Pair<long, long>> g, long sourceVertex, long targetVertex)
        {
            g.AddVertex(sourceVertex);
            g.AddVertex(targetVertex);

            return g.AddEdge(sourceVertex, targetVertex)!;
        }

        public static long GetOppositeVertex(IGraph<long, Pair<long, long>> graph, Pair<long, long> edge, long vertex)
        {
            long source = graph.GetEdgeSource(edge);
            long target = graph.GetEdgeTarget(edge);

            if (vertex == source)
            {
                return target;
            }
            else if (vertex == target)
            {
                return source;
            }
            else
            {
                throw new ArgumentException("no such vertex: " + vertex);
            }
        }
    }
}
