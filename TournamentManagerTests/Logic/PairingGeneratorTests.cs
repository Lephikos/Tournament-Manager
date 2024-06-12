using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Data;
using Tournament_Manager.Logic;
using Tournament_Manager.Logic.WeightFunctions;
using Tournament_Manager.Logic.util;

namespace TournamentManagerTests.Logic
{

    [TestClass]
    public class PairingGeneratorTests
    {

        private TournamentPlayerData[] playerData = new TournamentPlayerData[20];

        [TestInitialize]
        public void FillPlayerData()
        {
            playerData = new TournamentPlayerData[1]
            {
                new TournamentPlayerData(1, 3, new Dictionary<long, int>() { {1L, 2 } }, new List<int>(), 0, 0, 1, 0),
            };
        }


        [TestMethod]
        public void TestWithNoPlayers()
        {
            List<TournamentPlayerData> players = new List<TournamentPlayerData>();

            List<Pair<TournamentPlayerData, TournamentPlayerData>> matchups = PairingGenerator.GeneratePairings(players, StandardRules.GetWeightFor);

            Assert.IsTrue(matchups.Count == 0);
        }

        [TestMethod]
        public void TestByeOnePlayer()
        {
            List<TournamentPlayerData> players = new List<TournamentPlayerData>();

            players.Add(players[0]);

            List<Pair<TournamentPlayerData, TournamentPlayerData>> matchups = PairingGenerator.GeneratePairings(players, StandardRules.GetWeightFor);

            Assert.IsTrue(matchups.Count == 0);
        }

        [TestMethod]
        public void TestByeThreePlayer()
        {
            List<TournamentPlayerData> players = new List<TournamentPlayerData>();

            //players.Add(new TournamentPlayerData(1, 0, new List<long>(), new List<int>(), 0, 0, 0, 0));
            //players.Add(new TournamentPlayerData(2, 0, new List<long>(), new List<int>(), 0, 0, 0, 0));
            //players.Add(new TournamentPlayerData(3, 0, new List<long>(), new List<int>(), 0, 0, 0, 0));

            List<Pair<TournamentPlayerData, TournamentPlayerData>> matchups = PairingGenerator.GeneratePairings(players, StandardRules.GetWeightFor);

            Assert.IsTrue(matchups.Count == 1);
        }

        [TestMethod]
        public void TestStandardRules1()
        {
            List<TournamentPlayerData> players = new List<TournamentPlayerData>();

            //players.Add(new TournamentPlayerData(1, 0, new List<long>(), new List<int>(), 0, 0, 1, 0));
            //players.Add(new TournamentPlayerData(2, 0, new List<long>(), new List<int>(), 0, 0, 1, 0));
            //players.Add(new TournamentPlayerData(3, 0, new List<long>(), new List<int>(), 0, 0, 0, 0));
            //players.Add(new TournamentPlayerData(4, 0, new List<long>(), new List<int>(), 0, 0, 0, 0));

            List<Pair<TournamentPlayerData, TournamentPlayerData>> matchups = PairingGenerator.GeneratePairings(players, StandardRules.GetWeightFor);

            matchups.ForEach(p =>
            {
                Console.WriteLine(p.GetFirst().id + " " + p.GetSecond().id);
            });
        }



    }
}
