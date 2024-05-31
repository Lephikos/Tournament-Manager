using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Data;

namespace Tournament_Manager
{
    internal class PairingGenerator
    {

        private List<TournamentPlayerData> activePlayers;


        public PairingGenerator(List<TournamentPlayerData> activePlayers)
        {
            this.activePlayers = activePlayers;
        }

        public List<Pair<long>> GeneratePairings()
        {

            List<long> possibleByes = GetByeCandidates();

            //ChooseBye();


            //GetGroups();
            //CreateGraph();
            //SolveGraph();



            return null;
        }




        private List<long> GetByeCandidates()
        {
            List<long> result = new List<long>();

            activePlayers.ForEach(p =>
            {
                if (p.byes == 0 || p.byes < activePlayers.ConvertAll(p => p.byes).Max())
                {
                    result.Add(p.id);
                }
            });

            return result;
        }


    }
}
