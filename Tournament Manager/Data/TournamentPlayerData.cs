using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament_Manager.Data
{
    internal class TournamentPlayerData
    {

        /// <summary>
        /// Player id
        /// </summary>
        public long id;

        /// <summary>
        /// Points in the tournament (real points * 2 to avoid floating numbers)
        /// </summary>
        public int points;

        /// <summary>
        /// Ids of the players the player has already played against as key and the result as value 
        /// (1 = win with white, 2 = win with black, 0 = draw, -1 = loss with white, -2 = loss with black)
        /// </summary>
        public Dictionary<long, int> opponents;

        /// <summary>
        /// Tiebreak scores according to different tiebreak systems
        /// </summary>
        public List<int> tiebreaks;

        /// <summary>
        /// ColorStreak > 0 => Streak with white, ColorStreak < 0 Streak with black
        /// </summary>
        public int colorStreak;

        /// <summary>
        /// Sum of all white and black games, where white = 1 and black = -1
        /// </summary>
        public int colorDiff;

        /// <summary>
        /// ColorDiff/Streak for each gameday
        /// </summary>
        public int gamedayColors;

        /// <summary>
        /// number of received byes
        /// </summary>
        public int byes;

        public TournamentPlayerData(long id, int points, Dictionary<long, int> opponents, List<int> tiebreaks, int colorStreak,
            int colorDiff, int gamedayColors, int byes) 
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




        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(TournamentPlayerData)) return false;

            TournamentPlayerData other = (TournamentPlayerData) obj;

            return id == other.id 
                && points == other.points 
                && opponents.SequenceEqual(other.opponents) 
                && tiebreaks.SequenceEqual(other.tiebreaks)
                && colorStreak == other.colorStreak 
                && colorDiff == other.colorDiff 
                && gamedayColors == other.gamedayColors
                && byes == other.byes;
        }

        public override int GetHashCode()
        {
            int hash = 137;

            hash = (hash * 397) ^ id.GetHashCode();
            hash = (hash * 397) ^ points.GetHashCode();
            hash = (hash * 397) ^ opponents.GetHashCode();
            hash = (hash * 397) ^ tiebreaks.GetHashCode();
            hash = (hash * 397) ^ colorStreak.GetHashCode();
            hash = (hash * 397) ^ colorDiff.GetHashCode();
            hash = (hash * 397) ^ gamedayColors.GetHashCode();
            hash = (hash * 397) ^ byes.GetHashCode();

            return hash;
        }
    }
}
