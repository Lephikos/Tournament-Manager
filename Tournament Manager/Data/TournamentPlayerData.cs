using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Data
{
    internal class TournamentPlayerData
    {

        public long id;

        public int points;

        public List<long> opponents;

        public List<int> tiebreaks;

        public int colorStreak;

        public int colorDiff;

        public List<int> gamedayColors;

        public int byes;

        public TournamentPlayerData(long id, int points, List<long> opponents, List<int> tiebreaks, int colorStreak,
            int colorDiff, List<int> gamedayColors, int byes) 
        {
            this.id = id;
            this.points = points;
            this.opponents = opponents;
            this.tiebreaks = tiebreaks;
            this.colorStreak = colorStreak;
            this.colorDiff = colorDiff;
            this.gamedayColors = gamedayColors;
            this.byes = byes;
        }

    }
}
