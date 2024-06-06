using Tournament_Manager.Logic.util;
using Tournament_Manager.Logic;
using Tournament_Manager.Logic.Graph.cs;
using Tournament_Manager.Logic.Matching.BlossomV;

namespace TournamentManagerTests.Logic.Matching.BlossomV
{
    [TestClass]
    public class KolmogorovWeightedPerfectMatchingTests
    {
        [TestMethod]
        public void TestMethod1()
        {
         
            UndirectedSimpleGraph graph = new UndirectedSimpleGraph();

            Console.WriteLine("Added vertex: " + graph.AddVertex());
            Console.WriteLine("Added vertex: " + graph.AddVertex());
            Console.WriteLine("Added vertex: " + graph.AddVertex());
            Console.WriteLine("Added vertex: " + graph.AddVertex());

            Console.WriteLine("Added edge: " + graph.AddEdge(0, 1));
            Console.WriteLine("Added edge: " + graph.AddEdge(1, 2));
            Console.WriteLine("Added edge: " + graph.AddEdge(2, 3));
            Console.WriteLine("Added edge: " + graph.AddEdge(3, 0));
            Console.WriteLine("Added edge: " + graph.AddEdge(0, 2));
            Console.WriteLine("Added edge: " + graph.AddEdge(1, 3));

            Dictionary<Pair<long, long>, double> weights = new Dictionary<Pair<long, long>, double>();
            weights[graph.GetEdge(0, 1)!] = 4.0;
            weights[graph.GetEdge(1, 2)!] = 3.0;
            weights[graph.GetEdge(2, 3)!] = 4.0;
            weights[graph.GetEdge(3, 0)!] = 3.0;
            weights[graph.GetEdge(3, 0)!] = 2.0;
            weights[graph.GetEdge(3, 0)!] = 3.0;

            KolmogorovWeightedPerfectMatching<long, Pair<long, long>> matcher = 
                new KolmogorovWeightedPerfectMatching<long, Pair<long, long>>(new AsWeightedGraph<long, Pair<long, long>>(graph, weights, false), ObjectiveSense.MINIMIZE);

            Console.WriteLine(matcher.GetMatching());

        }
    }
}