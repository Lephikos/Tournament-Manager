using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tournament_Manager.Data;
using Tournament_Manager.Logic.Tiebreaks;

namespace Tournament_Manager.Forms
{
	public partial class Tournament : Form
	{

		TournamentData tournamentData;

		Form predecessor;

		public Tournament(string name, List<Tiebreaks> tiebreaks, Form predecessor)
		{
			InitializeComponent();

			tournamentData = new TournamentData(name, tiebreaks);
			this.predecessor = predecessor;
		}









		private void Tournament_Load(object sender, EventArgs e)
		{

		}

		private void btnAddPlayer_Click(object sender, EventArgs e)
		{
			string playerName;
			int playerRating = 0;
			Player player;

			if (txtBoxNamePlayer != null && txtBoxNamePlayer.Text.Length > 0)
			{
				playerName = txtBoxNamePlayer.Text;

				try
				{
					playerRating = int.Parse(txtBoxRatingPlayer.Text);
				}
				catch (Exception unused) { }

				player = new Player(playerName, playerRating);
				tournamentData.AddPlayer(player);

			}

			
		}
	}
}
