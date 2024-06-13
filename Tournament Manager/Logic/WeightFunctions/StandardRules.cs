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

        //High weight = good
        public static double GetWeightFor(TournamentPlayerData firstPlayer, TournamentPlayerData secondPlayer)
        {
            double result = 0.0;

            //Rule 1: Different colors on same day
            if ((firstPlayer.gamedayColors >= 0 && secondPlayer.gamedayColors <= 0) //tries to match players with different colors
                    || (firstPlayer.gamedayColors <= 0 && secondPlayer.gamedayColors >= 0))
            {
                result += 50;                
            }
            result *= 10;

            //Rule 2: No same pairings
            if (!firstPlayer.opponents.ContainsKey(secondPlayer.id) && !secondPlayer.opponents.ContainsKey(firstPlayer.id))
            {
                result += 50;
            }
            result *= 10;

            //Rule 3: ColorStreak must be less than 3 and colorDiff is not allowed to exceed 2
            if (IsValidColorMatching(firstPlayer, secondPlayer))
            {
                result += 50;
            }
            result *= 10;

            //Rule 4: Match players with similar points
            result += 50 - (Math.Max(Math.Abs(firstPlayer.points - secondPlayer.points), 50));
            result *= 10;

            //Rule 5: Try to give everyone the desired color
            result += GetColorRating(firstPlayer, secondPlayer);
            result *= 10;

            //Rule 6: Try to give favourite opponent according to swiss rule
            result += GetGroupWeight(firstPlayer, secondPlayer);


            return result;
        }

        public static bool FirstIsWhite(TournamentPlayerData firstPlayer, TournamentPlayerData secondPlayer)
        {
            int firstPrio = GetColorPrio(firstPlayer);
            int secondPrio = GetColorPrio(secondPlayer);

            if (firstPrio >= 0 && secondPrio <= 0 && firstPrio != secondPrio) //First player wants white, second wants black
            {
                return true;
            } else if (firstPrio <= 0 && secondPrio >= 0 && firstPrio != secondPrio) //First player wants black, second wants white
            {
                return false;
            } else if (Math.Abs(firstPrio) > Math.Abs(secondPrio)) //Both want same color, but first player has higher prio
            {
                return firstPrio > 0;
            } else if (Math.Abs(firstPrio) < Math.Abs(secondPrio)) //Both want same color, but second player has higher prio
            {
                return secondPrio < 0;
            } else
            {
                Random random = new Random(); //Both same prio => choose random

                return random.Next() % 2 == 1;
            }
        }

        private static int GetColorPrio(TournamentPlayerData player)
        {
            int result = 0;

            if (player.colorStreak >= 2 || player.colorDiff >= 2 || player.gamedayColors >= 1) //Has to play as black next game
            {
                result = -3;
            } else if (player.colorStreak <= -2 || player.colorDiff <= -2 || player.gamedayColors <= -1) //Has to play as white next game
            {
                result = +3;
            } else if (player.colorDiff == 1) //Wants to play as black next game
            {
                result = -2;
            } else if (player.colorDiff == -1) //Wants to play as white next game
            {
                result = +2;
            } else if (player.colorDiff == 0 && player.colorStreak == 1) //Wants to switch colors -> black
            {
                result = -1;
            } else if (player.colorDiff == 0 && player.colorStreak == -1) //Wants to switch colors -> white
            {
                result = +1;
            }

            return result;
        }

        private static int GetColorRating(TournamentPlayerData firstPlayer, TournamentPlayerData secondPlayer)
        {
            int result = 0;
            int firstPrio = GetColorPrio(firstPlayer);
            int secondPrio = GetColorPrio(secondPlayer);

            if ((firstPrio == 3 && secondPrio == 3) || (firstPrio == -3 && secondPrio == -3)) //Can't play against each other
            {
                result = 0;
            } else if ((firstPrio < 0 && secondPrio > 0) || (firstPrio > 0 && secondPrio < 0)) //Both get favourite color
            {
                result = 50;
            } else if (firstPrio != secondPrio && ((firstPrio > 0 && secondPrio > 0) || (firstPrio < 0 && secondPrio < 0))) {
                //Both want same color, but have different prio
                switch (firstPrio + secondPrio)
                {
                    case 3: result = 40; break; //Prio 1 and 2 -> Not that bad
                    case 4: result = 25; break; //Prio 1 and 3 -> ok
                    case 5: result = 10; break; //Prio 2 and 3 -> Bad, because next roudn there is another prio 3
                    default: result = 50; break; //Shouldn't happen
                }

            } else if (firstPrio == secondPrio && firstPrio != 0)
            {
                //Same prio for same color
                result = 50 - 15 * firstPrio;
            } else
            {
                result = 50; //Both have prio 0, every pairing is fine
            }

            return result;
        }
        
        private static bool IsValidColorMatching(TournamentPlayerData firstPlayer, TournamentPlayerData secondPlayer)
        {
            int firstPrio = GetColorPrio(firstPlayer);
            int secondPrio = GetColorPrio(secondPlayer);

            return (firstPrio == 3 && secondPrio == 3) || (firstPrio == -3 && secondPrio == -3);
        }

        private static double GetGroupWeight(TournamentPlayerData firstPlayer, TournamentPlayerData secondPlayer)
        {
            double weight;
            int sg = 0;

            if (firstPlayer.activeScoregroup == secondPlayer.activeScoregroup)
            {
                sg = firstPlayer.activeScoregroupSize;
            }

            weight = -Math.Pow(Math.Abs((sg / 2) - Math.Abs(firstPlayer.activeRank - secondPlayer.activeRank)), 2);

            return weight;
        }


    }
}
