using Tournament_Manager.Logic.Tiebreaks;

namespace Tournament_Manager.Forms;

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

	private void btnConfirmName_Click(object sender, EventArgs e)
	{
		string name = boxTournamentName.Text;

		if (!(name == null || name.Length == 0))
		{
			Form tournament =
				new Tournament(name, new List<Tiebreaks>() { Tiebreaks.BHZ, Tiebreaks.RATING }, predecessor);
			tournament.Show();
			this.Hide();
		}
	}

	private void CreateTournament_Load_1(object sender, EventArgs e)
	{

	}
}
