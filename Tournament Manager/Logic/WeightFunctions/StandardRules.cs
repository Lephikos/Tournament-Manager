using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Data;

namespace Tournament_Manager.Logic.WeightFunctions
{
    internal class StandardRules
    {

        public static double GetWeightFor(TournamentPlayerData firstPlayer, TournamentPlayerData secondPlayer)
        {
            double result = 0.0;

            //Rule 1: Different colors on same day
            if (firstPlayer.gamedayColors.Aggregate(false, (acc, b) => acc = acc ^ b) != secondPlayer.gamedayColors.Aggregate(false, (acc, b) => acc ^ b))
            {
                result += 50;
                result *= 10;
            }

            //true, false, true
            //weiß, schwarz, weiß
            return result;
        }




    }
}
