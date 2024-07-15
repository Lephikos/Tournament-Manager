using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Data;
using Tournament_Manager.Logic.Graph;
using Tournament_Manager.Logic.Matching;
using Tournament_Manager.Logic.Matching.BlossomV;
using Tournament_Manager.Logic.util;
using Tournament_Manager.Logic.WeightFunctions;

namespace Tournament_Manager.Logic
{
    internal class PairingGenerator
    {

        public static List<Pair<TournamentPlayerData, TournamentPlayerData>> GeneratePairings(
            List<TournamentPlayerData> activePlayers, Func<TournamentPlayerData, TournamentPlayerData, double> weightFunction)
        {

            List<Pair<TournamentPlayerData, TournamentPlayerData>> result = new List<Pair<TournamentPlayerData, TournamentPlayerData>>();
            List<long>? possibleByes = null;

            if (activePlayers.Count % 2 == 1)
            {
                possibleByes = GetByeCandidates(activePlayers);
                activePlayers.Add(new TournamentPlayerData(-1, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0)); //dummy player for bye
            }

            IGraph<long, Pair<long, long>> graph = ConstructGraph(activePlayers, possibleByes, weightFunction);

            KolmogorovWeightedPerfectMatching<long, Pair<long, long>> solver = new KolmogorovWeightedPerfectMatching<long, Pair<long, long>>(graph, ObjectiveSense.MAXIMIZE);
            IMatching<long, Pair<long, long>> matching = solver.GetMatching();
            if(!solver.TestOptimality())
            {
                throw new InvalidOperationException("TestOptimality is not true!");
            }
            
            HashSet<Pair<long, long>> matchups = matching.GetEdges();

            foreach (var pair in matchups)
            {
                TournamentPlayerData first = activePlayers.Where<TournamentPlayerData>(p => p.id == pair.GetFirst()).First();
                TournamentPlayerData second = activePlayers.Where<TournamentPlayerData>(p => p.id == pair.GetSecond()).First();

                if (StandardRules.FirstIsWhite(first, second))
                {
                    result.Add(new Pair<TournamentPlayerData, TournamentPlayerData>(first, second));
                } else
                {
                    result.Add(new Pair<TournamentPlayerData, TournamentPlayerData>(second, first));
                }
            }
            activePlayers.RemoveAll(p => p.id == -1); //remove dummy player for bye

            return result;
        }

        private static List<long> GetByeCandidates(List<TournamentPlayerData> activePlayers)
        {
            List<long> result = new List<long>();

            activePlayers.ForEach(p =>
            {
                if (p.byes == 0 || !activePlayers.Any(q => q.byes != p.byes) || p.byes < activePlayers.ConvertAll(p => p.byes).Max())
                {
                    result.Add(p.id);
                }
            });

            return result;
        }

        private static IGraph<long, Pair<long, long>> ConstructGraph(List<TournamentPlayerData> activePlayers, List<long>? possibleByes, Func<TournamentPlayerData, TournamentPlayerData, double> weightFunction)
        {
            UndirectedSimpleGraph graph = new UndirectedSimpleGraph();
            Dictionary<Pair<long, long>, double> weights = new Dictionary<Pair<long, long>, double>();

            foreach (var player in activePlayers)
            {
                //Add player
                if(!graph.AddVertex(player.id))
                {
                    throw new ArgumentException("Multiple players with same id are not allowed");
                }

				//Add edges to all other vertices and compute weight in dictionary
				HashSet<long> alreadyAdded = graph.VertexSet();

                foreach (long id in alreadyAdded)
                {
                    if (id != player.id)
                    {
						if (player.id != -1)
                        {
							graph.AddEdge(player.id, id);
                            weights[new Pair<long, long>(player.id, id)] =
                                weightFunction(player, activePlayers.Where<TournamentPlayerData>(p => p.id == id).First());
                        }
                        else
                        {
                            foreach (long v in possibleByes!)
                            {
								graph.AddEdge(v, player.id);
                                weights[new Pair<long, long>(v, player.id)] = 0;
							}
                        }
                    }
                }
            }


            //new graph with weighted edges
            return new AsWeightedGraph<long, Pair<long, long>>(graph, weights);
        }

    }
}
