using Tournament_Manager.Logic.util;
using Tournament_Manager.Logic;
using Tournament_Manager.Logic.Graph.cs;
using Tournament_Manager.Logic.Matching.BlossomV;
using Tournament_Manager.Logic.Matching;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.AxHost;
using System.Collections.Specialized;

namespace TournamentManagerTests.Logic.Matching.BlossomV
{
    [TestClass]
    public class KolmogorovWeightedPerfectMatchingTests
    {

        [TestMethod]
        public void TestInvalidDualSolution()
        {
            long[,] edges = new long[,] { { 1, 2, 7 }, { 2, 3, 4 }, { 3, 4, 3 }, { 4, 1, 4 }, };

            Pair<IGraph<long, Pair<long, long>>, Dictionary<Pair<long, long>, double>> res = TestUtil.ConstructGraph(edges);

            KolmogorovWeightedPerfectMatching<long, Pair<long, long>> matching =
                new KolmogorovWeightedPerfectMatching<long, Pair<long, long>>(new AsWeightedGraph<long, Pair<long, long>>(res.GetFirst(), res.GetSecond(), false), ObjectiveSense.MINIMIZE);
            matching.GetMatching();

            Dictionary<long, BlossomVNode> vertexMap = new Dictionary<long, BlossomVNode>(matching.state!.nodeNum);
            for (int i = 0; i < matching.state!.nodeNum; i++)
            {
                vertexMap[matching.state!.graphVertices[i]] = matching.state!.nodes[i];
            }
            BlossomVNode node1 = vertexMap[1];
            node1.dual += 1;

            Assert.IsFalse(matching.TestOptimality());
        }

        /// <summary>
        /// Test on a triangulation of 8 points Points: (2, 10), (9, 11), (10, 4), (11, 15), (12, 5),
        /// (12, 6), (13, 12), (14, 11)
        /// </summary>
        [TestMethod]
        public void TestGetMatching1()
        {

            long[,] edges = new long[,] { { 0, 1, 8 }, { 0, 2, 10 }, { 1, 2, 8 }, { 0, 3, 11 },
            { 1, 3, 5 }, { 2, 5, 3 }, { 1, 5, 6 }, { 2, 4, 3 }, { 4, 5, 1 }, { 1, 6, 5 },
            { 3, 6, 4 }, { 3, 7, 5 }, { 6, 7, 2 }, { 5, 7, 6 }, { 4, 7, 7 }, { 1, 7, 5 } };
                
            double maxWeight = 27;
            double minWeight = 18;

            Test(edges, minWeight, maxWeight);
        }

        /// <summary>
        /// Test on empty graph
        /// </summary>
        [TestMethod]
        public void TestGetMatching2()
        {
            long[,] edges = new long[,] {};

            double minWeight = 0;
            double maxWeight = 0;

            Test(edges, minWeight, maxWeight);
        }













        private static void Test(long[,] edges, double resultMin, double resultMax)
        {
            Pair<IGraph<long, Pair<long, long>>, Dictionary<Pair<long, long>, double>> res = TestUtil.ConstructGraph(edges);

            KolmogorovWeightedPerfectMatching<long, Pair<long, long>> min =
                new KolmogorovWeightedPerfectMatching<long, Pair<long, long>>(new AsWeightedGraph<long, Pair<long, long>>(res.GetFirst(), res.GetSecond(), false), ObjectiveSense.MINIMIZE);

            KolmogorovWeightedPerfectMatching<long, Pair<long, long>> max =
                new KolmogorovWeightedPerfectMatching<long, Pair<long, long>>(new AsWeightedGraph<long, Pair<long, long>>(res.GetFirst(), res.GetSecond(), false), ObjectiveSense.MAXIMIZE);

            Assert.AreEqual<double>(resultMax, max.GetMatching().GetWeight());
            Assert.AreEqual<double>(resultMin, min.GetMatching().GetWeight());
            Assert.IsTrue(min.TestOptimality());
            Assert.IsTrue(max.TestOptimality());

            CheckMatchingAndDualSolution(max.GetMatching(), max.GetDualSolution(), ObjectiveSense.MAXIMIZE);
            CheckMatchingAndDualSolution(min.GetMatching(), min.GetDualSolution(), ObjectiveSense.MINIMIZE);
        }

        private static void CheckMatchingAndDualSolution(
            IMatching<long, Pair<long, long>> matching, 
            KolmogorovWeightedPerfectMatching<long, Pair<long, long>>.DualSolution<long, Pair<long, long>> dualSolution, 
            ObjectiveSense objectiveSense)
        {
            IGraph<long, Pair<long, long>> graph = dualSolution.GetGraph();
            Assert.AreEqual(graph.VertexSet().Count, 2 * matching.GetEdges().Count);

            HashSet<Pair<long, long>> matchedEdges = matching.GetEdges();
            HashSet<long> vertices = new HashSet<long>();
            Dictionary<Pair<long, long>, Double> slacks = new Dictionary<Pair<long, long>, double>();

            //Check that each vertex has only 1 edge
            foreach (Pair<long, long> edge in matchedEdges)
            {
                long source = graph.GetEdgeSource(edge);
                long target = graph.GetEdgeTarget(edge);
                if (source != target)
                {
                    Assert.IsFalse(vertices.Contains(source));
                    Assert.IsFalse(vertices.Contains(target));

                    vertices.Add(source);
                    vertices.Add(target);
                    slacks[edge] = graph.GetEdgeWeight(edge);
                }
                else
                {
                    Assert.Fail();
                }
            }

            //slacks aller nicht im Matching enthaltenen Kanten
            foreach (Pair<long, long> edge in graph.EdgeSet())
            {
                if (!matchedEdges.Contains(edge))
                {
                    long source = graph.GetEdgeSource(edge);
                    long target = graph.GetEdgeTarget(edge);
                    if (source != target)
                    {
                        slacks[edge] = graph.GetEdgeWeight(edge);
                    }
                }
            }
            CollectionAssert.AreEquivalent(graph.VertexSet().ToList(), vertices.ToList());

            Dictionary<HashSet<long>, Double> dualMap = dualSolution.GetDualVariables();
            foreach (HashSet<long> key in dualMap.Keys)
            {
                double dualVariable = dualMap[key];

                if (key.Count > 1)
                {
                    if (objectiveSense == ObjectiveSense.MAXIMIZE)
                    {
                        // the dual variable of a pseudonode can't be greater than EPS
                        // for maximization problem
                        Assert.IsTrue(dualVariable - KolmogorovWeightedPerfectMatching<long, Pair<long, long>>.EPS <= 0);
                    }
                    else
                    {
                        // the dual variable of a pseudonode can't be less than -EPS
                        // for minimization problem
                        Assert.IsTrue(dualVariable + KolmogorovWeightedPerfectMatching<long, Pair<long, long>>.EPS >= 0);
                    }
                }
                foreach (long vertex in key)
                {
                    foreach (Pair<long, long> edge in graph.EdgesOf(vertex))
                    {
                        if (!key.Contains(TestUtil.GetOppositeVertex(graph, edge, vertex)))
                        { // checking whether the edge is boundary
                            slacks[edge] = slacks[edge] - dualVariable;
                        }
                    }
                }
            }
            foreach (Pair<long, long> key in slacks.Keys)
            {
                Pair<long, long> edge = key;
                double edgeSlack = slacks[key];

                if (matchedEdges.Contains(edge))
                {
                    // matched edge must have 0 slack
                    Assert.IsTrue(Math.Abs(edgeSlack) < KolmogorovWeightedPerfectMatching<long, Pair<long, long>>.EPS);
                }
                else if (objectiveSense == ObjectiveSense.MAXIMIZE)
                {
                    // in the optimal solution to the maximization problem edge slacks must be
                    // non-positive
                    Assert.IsTrue(edgeSlack - KolmogorovWeightedPerfectMatching<long, Pair<long, long>>.EPS <= 0);
                }
                else
                {
                    // in the optimal solution to the minimization problem edge slacks must be
                    // non-negative
                    Assert.IsTrue(edgeSlack + KolmogorovWeightedPerfectMatching<long, Pair<long, long>>.EPS >= 0);
                }
            }
        }
    }
}