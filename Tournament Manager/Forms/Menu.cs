namespace Tournament_Manager
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
    }
}
