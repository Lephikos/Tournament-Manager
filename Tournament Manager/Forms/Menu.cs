namespace Tournament_Manager.Forms
{
	public partial class Menu : Form
	{
		public Menu()
		{
			InitializeComponent();
		}

		private void BtnNewTournament_Click(object sender, EventArgs e)
		{
			Form createTournament = new CreateTournament(this);
			createTournament.Show();
			this.Hide();
		}

		private void BtnExit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Menu_Load(object sender, EventArgs e)
		{

		}
	}
}
