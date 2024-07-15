using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament_Manager.Data;
using Tournament_Manager.Logic;
using Tournament_Manager.Logic.WeightFunctions;
using Tournament_Manager.Logic.util;
using Tournament_Manager.Logic.Tiebreaks;
using System.Text.RegularExpressions;

namespace TournamentManagerTests.Logic
{

	[TestClass]
    public class PairingGeneratorTests
    {

        private TournamentPlayerData[] playerData = new TournamentPlayerData[20];

		private Player[] player = new Player[20];

		[TestInitialize]
        public void FillPlayerData()
        {
            playerData = new TournamentPlayerData[8]
            {
                new TournamentPlayerData(1, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
				new TournamentPlayerData(2, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
				new TournamentPlayerData(3, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
				new TournamentPlayerData(4, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
				new TournamentPlayerData(5, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
				new TournamentPlayerData(6, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
				new TournamentPlayerData(7, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
				new TournamentPlayerData(8, 0, new Dictionary<long, List<Result>>(), new List<int>(), 0, 0, 0, 0),
			};

            player = new Player[20]
            {
                new Player("Connor Kröger", 1714),//0
                new Player("Duncan Kröger", 1777),
                new Player("Lennart Gienke", 1470),
                new Player("Gerhard Hallekamp", 1962),
                new Player("Bernd Cronjäger", 1629),
                new Player("Jan Stubbe", 0),//5
                new Player("Thies", 0),
                new Player("Christian Stubbe", 0),
				new Player("Stephan Heinemann", 1107),
				new Player("Jason", 0),
				new Player("Martin Hänsel", 0),//10
				new Player("Hanna Feindt", 0),
				new Player("Tobias Grünzel", 1675),
				new Player("Dominique", 0),
				new Player("Bernd Samuelsen", 0),
			    new Player("Ralf zum Felde", 0),//15
				new Player("Rüdiger Buchhorn", 1203),
				new Player("Christiane Rommeck", 1450),
				new Player("Martin Winkelmann", 1779),
				new Player("Manfred Niemann", 1256),

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

            players.Add(playerData[0]);

            List<Pair<TournamentPlayerData, TournamentPlayerData>> matchups = PairingGenerator.GeneratePairings(players, StandardRules.GetWeightFor);

            Assert.IsTrue(matchups.Count == 1);
        }

        [TestMethod]
        public void TestByeThreePlayer()
        {
            List<TournamentPlayerData> players = new List<TournamentPlayerData>();

			players.Add(playerData[0]);
			players.Add(playerData[1]);
			players.Add(playerData[2]);

			List<Pair<TournamentPlayerData, TournamentPlayerData>> matchups = PairingGenerator.GeneratePairings(players, StandardRules.GetWeightFor);

            Assert.IsTrue(matchups.Count == 2);
        }

        [TestMethod]
        public void TestStandardRules1()
        {
            TournamentData tournamentData = new TournamentData("Test", new List<Tiebreaks>() { Tiebreaks.MATCH_COUNT, Tiebreaks.RATING });
            List<Pair<Pair<long, long>, Result>> results = new List<Pair<Pair<long, long>, Result>>();

            for (int i = 0; i < player.Count(); i++)
            {
				tournamentData.AddPlayer(player[i]);
			}

			#region first round

			for (int i = 0; i < 14; i++)
			{
				tournamentData.SetPlayerActive(player[i]);
			}

			List<Pair<TournamentPlayerData, TournamentPlayerData>> matchups = 
                PairingGenerator.GeneratePairings(tournamentData.GetAllActivePlayers(), StandardRules.GetWeightFor);

            matchups.ForEach(p =>
            {
                Console.WriteLine(p.GetFirst().id + "/" + tournamentData.GetPlayersWithId(p.GetFirst()).First().name + " vs. " +
					p.GetSecond().id + "/" + tournamentData.GetPlayersWithId(p.GetSecond()).First().name);
			});

			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(3, 4), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(2, 1), Result.BLACK_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(0, 8), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(12, 9), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(13, 7), Result.BLACK_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(11, 6), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(5, 10), Result.BLACK_WIN));
			tournamentData.UpdateResults(results);

			#endregion first round

			#region second round

			tournamentData.SetPlayerActive(player[14]);
            tournamentData.SetPlayerActive(player[15]);
            tournamentData.RemovePlayerFromActive(player[6]);
            tournamentData.RemovePlayerFromActive(player[9]);

			Console.WriteLine("");

			matchups = PairingGenerator.GeneratePairings(tournamentData.GetAllActivePlayers(), StandardRules.GetWeightFor);
			matchups.ForEach(p =>
			{
				Console.WriteLine(p.GetFirst().id + "/" + tournamentData.GetPlayersWithId(p.GetFirst()).First().name + " vs. " +
					p.GetSecond().id + "/" + tournamentData.GetPlayersWithId(p.GetSecond()).First().name);
			});

            results.Clear();
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(10, 0), Result.BLACK_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(1, 3), Result.BLACK_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(15, 2), Result.BLACK_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(4, 11), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(14, 5), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(7, 12), Result.BLACK_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(8, 13), Result.BLACK_WIN));
            tournamentData.UpdateResults(results);

			#endregion second round

			tournamentData.ResetGameday();

			#region third round

			for (int i = 0; i < player.Length; i++)
			{
				tournamentData.RemovePlayerFromActive(player[i]);
			}
			tournamentData.SetPlayerActive(player[1]);
			tournamentData.SetPlayerActive(player[2]);
			tournamentData.SetPlayerActive(player[3]);
			tournamentData.SetPlayerActive(player[4]);
			tournamentData.SetPlayerActive(player[11]);
			tournamentData.SetPlayerActive(player[14]);
			tournamentData.SetPlayerActive(player[16]);
			tournamentData.SetPlayerActive(player[17]);
			tournamentData.SetPlayerActive(player[18]);

			Console.WriteLine("");

			matchups = PairingGenerator.GeneratePairings(tournamentData.GetAllActivePlayers(), StandardRules.GetWeightFor);
			matchups.ForEach(p =>
			{
				Player firstname = tournamentData.GetPlayersWithId(p.GetFirst()).FirstOrDefault();
				var second = tournamentData.GetPlayersWithId(p.GetSecond()).FirstOrDefault();

				if (firstname == null) Console.WriteLine("Bye vs. " + p.GetSecond().id + "/" + second.name);
				else if (second == null) Console.WriteLine(p.GetFirst().id + "/" + firstname.name + " vs. Bye");
				else Console.WriteLine(p.GetFirst().id + "/" + firstname.name + " vs. " + p.GetSecond().id + "/" + second.name);
				
			});

			results.Clear();
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(1, 4), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(2, 14), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(11, 18), Result.BLACK_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(16, 17), Result.BLACK_WIN));
			tournamentData.SetBye(player[3]);
			tournamentData.UpdateResults(results);

			#endregion third round

			#region fourth round

			tournamentData.SetPlayerActive(player[13]);
			tournamentData.SetPlayerActive(player[19]);

			Console.WriteLine("");

			matchups = PairingGenerator.GeneratePairings(tournamentData.GetAllActivePlayers(), StandardRules.GetWeightFor);
			matchups.ForEach(p =>
			{
				Player firstname = tournamentData.GetPlayersWithId(p.GetFirst()).FirstOrDefault();
				var second = tournamentData.GetPlayersWithId(p.GetSecond()).FirstOrDefault();

				if (firstname == null) Console.WriteLine("Bye vs. " + p.GetSecond().id + "/" + second.name);
				else if (second == null) Console.WriteLine(p.GetFirst().id + "/" + firstname.name + " vs. Bye");
				else Console.WriteLine(p.GetFirst().id + "/" + firstname.name + " vs. " + p.GetSecond().id + "/" + second.name);
			});

			results.Clear();
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(14, 1), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(3, 2), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(17, 19), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(18, 16), Result.WHITE_WIN));
			results.Add(new Pair<Pair<long, long>, Result>(new Pair<long, long>(13, 11), Result.WHITE_WIN));
			tournamentData.SetBye(player[4]);
			tournamentData.UpdateResults(results);

			#endregion fourth round

			tournamentData.ResetGameday();


		}



	}
}
