namespace Tournament_Manager
{
    public partial class CreateTournament : Form
    {
        private readonly Form predecessor;

        public CreateTournament(Form predecessor)
        {
            InitializeComponent();

            this.predecessor = predecessor;
        }

        private void CreateTournament_Load(object sender, EventArgs e)
        {

        }

        private void CreateTournament_FormClosed(object sender, FormClosedEventArgs e)
        {
            predecessor.Close();
        }
    }
}
