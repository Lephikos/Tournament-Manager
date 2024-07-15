namespace Tournament_Manager.Forms
{
    partial class CreateTournament
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			boxTournamentName = new TextBox();
			btnConfirmName = new Button();
			label1 = new Label();
			SuspendLayout();
			// 
			// boxTournamentName
			// 
			boxTournamentName.Location = new Point(236, 75);
			boxTournamentName.Name = "boxTournamentName";
			boxTournamentName.Size = new Size(220, 23);
			boxTournamentName.TabIndex = 0;
			// 
			// btnConfirmName
			// 
			btnConfirmName.Location = new Point(501, 75);
			btnConfirmName.Name = "btnConfirmName";
			btnConfirmName.Size = new Size(148, 23);
			btnConfirmName.TabIndex = 1;
			btnConfirmName.Text = "Confirm";
			btnConfirmName.UseVisualStyleBackColor = true;
			btnConfirmName.Click += btnConfirmName_Click;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(236, 160);
			label1.Name = "label1";
			label1.Size = new Size(207, 15);
			label1.TabIndex = 2;
			label1.Text = "Tiebreaker: BHZ -> Rating -> Random";
			// 
			// CreateTournament
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(label1);
			Controls.Add(btnConfirmName);
			Controls.Add(boxTournamentName);
			Name = "CreateTournament";
			Text = "CreateTournament";
			FormClosed += CreateTournament_FormClosed;
			Load += CreateTournament_Load_1;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TextBox boxTournamentName;
		private Button btnConfirmName;
		private Label label1;
	}
}