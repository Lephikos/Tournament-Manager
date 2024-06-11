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

        public long id;

        public int points;

        public List<long> opponents;

        public List<int> tiebreaks;

        public int colorStreak;

        public int colorDiff;

        public List<bool> gamedayColors;

        public int byes;

        public TournamentPlayerData(long id, int points, List<long> opponents, List<int> tiebreaks, int colorStreak,
            int colorDiff, List<bool> gamedayColors, int byes) 
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
                && gamedayColors.SequenceEqual(other.gamedayColors) 
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
