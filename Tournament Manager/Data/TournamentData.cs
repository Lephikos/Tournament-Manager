using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Logic.Tiebreaks;
using Tournament_Manager.Logic.util;

namespace Tournament_Manager.Data
{
	internal class TournamentData
    {
        private Guid guid = new Guid();

        private string name;

        private HashSet<Pair<Player, TournamentPlayerData>> players = new HashSet<Pair<Player, TournamentPlayerData>> ();

        public List<TournamentPlayerData> active = new List<TournamentPlayerData> ();

        private List<Tiebreaks> tiebreaks = new List<Tiebreaks>() { Tiebreaks.RANDOM };


        public TournamentData(string name)
        {
            this.name = name;
        }

        public TournamentData(string name, List<Tiebreaks> tiebreaks) : this(name)
        {
            this.tiebreaks = tiebreaks;
            this.tiebreaks.Add(Tiebreaks.RANDOM);
        }



        public void AddPlayer(Player player)
        {
            players.Add(new Pair<Player, 
                TournamentPlayerData>(player, new TournamentPlayerData(GetId(), 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0)));

        }

        public bool RemovePlayer(Player player)
        {
            return players.RemoveWhere(p => p.GetFirst() == player) >= 1;
        }

        public List<TournamentPlayerData> GetAllActivePlayers()
        {
            return active;
        }

        public List<Player> GetPlayersWithName(string name)
        {
            return players.Where(player => player.GetFirst().name.Contains(name)).Select(p => p.GetFirst()).ToList();
        }

        public List<Player> GetPlayersWithRating(int rating)
        {
            return players.Where(player => player.GetFirst().rating.ToString().StartsWith(rating.ToString())).Select(p => p.GetFirst()).ToList();
        }

        public List<Player> GetPlayersWithId(TournamentPlayerData data)
        {
            return players.Where(p => p.GetSecond().id == data.id).Select(p => p.GetFirst()).ToList();
        }

        public void SetAllPlayersActive()
        {
            foreach (Pair<Player, TournamentPlayerData> player in players)
            {
                active.Add(player.GetSecond());
            }
            UpdateActiveList();
        }

        public void SetPlayerActive(Pair<Player, TournamentPlayerData> player)
        {
            active.Add(player.GetSecond());
            UpdateActiveList();
        }

        public void SetPlayerActive(Player player)
        {
            active.Add(players.Where(p => p.GetFirst().Equals(player)).First().GetSecond());
            UpdateActiveList();
        }

        public void RemovePlayerFromActive(Pair<Player, TournamentPlayerData> player)
        {
            TournamentPlayerData? data = active.Where(p => p.Equals(player.GetSecond())).FirstOrDefault();

            if (data == null)
            {
                return;
            }

            active.Remove(data);
            UpdateActiveList();
        }

        public void RemovePlayerFromActive(Player player)
        {
            TournamentPlayerData data = players.Where(p => p.GetFirst().Equals(player)).First().GetSecond();

            active.Remove(data);
            UpdateActiveList();
        }

        public void SetBye(Player player)
        {
			TournamentPlayerData data = players.Where(p => p.GetFirst().Equals(player)).First().GetSecond();

            data.byes++;
            data.points++;

			UpdateActiveList();
		}

        public void ResetGameday()
        {
            foreach (var player in players)
            {
                player.GetSecond().gamedayColors = 0;
            }
        }

        private void UpdateActiveList()
        {
            UpdateTiebreaks();

			SortedDictionary<int, HashSet<TournamentPlayerData>> scoregroups = 
                new SortedDictionary<int, HashSet<TournamentPlayerData>>(Comparer<int>.Create((x, y) => y.CompareTo(x)));

            //Collect all scoregroups
            foreach (var player in active)
            {
                if (scoregroups.ContainsKey(player.points))
                {
                    scoregroups[player.points].Add(player);

                } else
                {
                    scoregroups[player.points] = new HashSet<TournamentPlayerData>() { player };
                }
            }

            //Update scoregroups
			foreach (var player in active)
            {
                player.activeScoregroup = player.points;
                player.activeScoregroupSize = scoregroups[player.points].Count;
            }

            //Update rank
            int rankOffset = 0;
            foreach (var scoregroup in scoregroups)
            {
                foreach (var player in scoregroup.Value)
                {
                    player.activeRank = rankOffset + RankInScoregroup(player, scoregroup.Value);
                }
				rankOffset += scoregroup.Value.Count;
			}
        }

        private int RankInScoregroup(TournamentPlayerData player, HashSet<TournamentPlayerData> playersInSameScoregroup)
        {
            int rank = 1;
            int index = 0;

            var copy = new HashSet<TournamentPlayerData>(playersInSameScoregroup);

            while (copy.Count > 1 && index < tiebreaks.Count) {

				copy.RemoveWhere(p => player.tiebreaks.ElementAt(index) > p.tiebreaks.ElementAt(index));
                rank += copy.RemoveWhere(p => player.tiebreaks.ElementAt(index) < p.tiebreaks.ElementAt(index));

                index++;
            }

            return rank;
        }

        public void UpdateResults(List<Pair<Pair<long, long>, Result>> results)
        {
            
            foreach (var result in results)
            {
                TournamentPlayerData white = active.Where(p => result.GetFirst().GetFirst() == p.id).First();
                TournamentPlayerData black = active.Where(p => result.GetFirst().GetSecond() == p.id).First();

                //Update points && opponents
                switch (result.GetSecond())
                {
                    case Result.DRAW:
                        {
                            white.points += 1;
                            black.points += 1;

                            if (white.opponents.ContainsKey(black.id))
                            {
                                white.opponents[black.id].Add(Result.DRAW);
                            } else
                            {
                                white.opponents[black.id] = new List<Result>() { Result.DRAW };
                            }

                            if (black.opponents.ContainsKey(white.id))
                            {
                                black.opponents[white.id].Add(Result.DRAW);
                            }
                            else
                            {
                                black.opponents[white.id] = new List<Result>() { Result.DRAW };
                            }

                            break;
                        }
                    case Result.WHITE_WIN:
                    case Result.BLACK_LOSS:
                        {
                            white.points += 2;

                            if (white.opponents.ContainsKey(black.id))
                            {
                                white.opponents[black.id].Add(Result.WHITE_WIN);
                            }
                            else
                            {
                                white.opponents[black.id] = new List<Result>() { Result.WHITE_WIN };
                            }

                            if (black.opponents.ContainsKey(white.id))
                            {
                                black.opponents[white.id].Add(Result.BLACK_LOSS);
                            }
                            else
                            {
                                black.opponents[white.id] = new List<Result>() { Result.BLACK_LOSS };
                            }

                            break;
                        }
                    case Result.WHITE_LOSS:
                    case Result.BLACK_WIN:
                        {
                            black.points += 2;

                            if (white.opponents.ContainsKey(black.id))
                            {
                                white.opponents[black.id].Add(Result.WHITE_LOSS);
                            }
                            else
                            {
                                white.opponents[black.id] = new List<Result>() { Result.WHITE_LOSS };
                            }

                            if (black.opponents.ContainsKey(white.id))
                            {
                                black.opponents[white.id].Add(Result.BLACK_WIN);
                            }
                            else
                            {
                                black.opponents[white.id] = new List<Result>() { Result.BLACK_WIN };
                            }

                            break;
                        }
                }

                //Update colorStreak and colorDiff
                white.colorDiff++;
                black.colorDiff--;

                white.colorStreak = white.colorStreak > 0 ? white.colorStreak + 1 : 1;
                black.colorStreak = black.colorStreak < 0 ? black.colorStreak - 1 : -1;

                //Update gameDayColors
                white.gamedayColors++;
                black.gamedayColors--;
            }

			UpdateTiebreaks();
			UpdateActiveList();
        }

        private void UpdateTiebreaks()
        {
            foreach (var player in players)
            {
                player.GetSecond().tiebreaks.Clear();
            }

            for (int i = 0; i < tiebreaks.Count - 1; i++) {

                foreach (var player in players)
                {
                    player.GetSecond().tiebreaks.Add( 
                        Tiebreak.ComputeTiebreak(player, players.Select(p => p.GetSecond()).ToHashSet(), tiebreaks[i]));
                }
            }

            //Random Tiebreak
            HashSet<int> ints = new HashSet<int>();
            Random random = new Random();

            for (int i = 0; i < players.Count; i++)
            {
                ints.Add(i);
            }
            foreach (var player in players)
            {
                player.GetSecond().tiebreaks.Add(ints.ElementAt(random.Next(ints.Count)));
                ints.Remove(player.GetSecond().tiebreaks.Last());
            }
        }

        private long GetId()
        {
            long id = 0;

            while (players.Select(player => player.GetSecond().id).Contains(id))
            {
                id++;
            }

            return id;
        }

    }
}
