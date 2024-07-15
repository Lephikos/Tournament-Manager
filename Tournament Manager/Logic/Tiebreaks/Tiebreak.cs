using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Data;
using Tournament_Manager.Logic.util;

namespace Tournament_Manager.Logic.Tiebreaks
{
	internal static class Tiebreak
    {

        public static int ComputeTiebreak(Pair<Player, TournamentPlayerData> player, HashSet<TournamentPlayerData> data, Tiebreaks type)
        {

            switch(type)
            {
                case Tiebreaks.MOST_WINS:
                    {
                        return player.GetSecond().opponents.Values.
                            Where(res => res.Equals(Result.WHITE_WIN) || res.Equals(Result.BLACK_WIN)).Count();
                    }
                case Tiebreaks.RATING:
                    {
                        return player.GetFirst().rating;
                    }
                case Tiebreaks.BHZ:
                    {
                        int res = 0;

                        foreach (var opponent in player.GetSecond().opponents.Keys.Select(id => data.Where(d => d.id == id).First()))
                        {
                            res += opponent.points;
                        }

                        return res;
                    }
                case Tiebreaks.MATCH_COUNT:
                    {
                        return player.GetSecond().opponents.Values.Count();
                    }
                default: {
                        return 0;
                    }
            }
        }
    }
}
