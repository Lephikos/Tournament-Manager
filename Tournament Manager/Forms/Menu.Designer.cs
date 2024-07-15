namespace Tournament_Manager.Forms
{
    partial class Menu
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			BtnNewTournament = new Button();
			BtnLoadTournament = new Button();
			BtnExit = new Button();
			SuspendLayout();
			// 
			// BtnNewTournament
			// 
			BtnNewTournament.AutoSize = true;
			BtnNewTournament.Location = new Point(352, 89);
			BtnNewTournament.Name = "BtnNewTournament";
			BtnNewTournament.Size = new Size(137, 25);
			BtnNewTournament.TabIndex = 0;
			BtnNewTournament.Text = "Neues Turnier erstellen";
			BtnNewTournament.UseVisualStyleBackColor = true;
			BtnNewTournament.Click += BtnNewTournament_Click;
			// 
			// BtnLoadTournament
			// 
			BtnLoadTournament.AutoSize = true;
			BtnLoadTournament.Location = new Point(352, 161);
			BtnLoadTournament.Name = "BtnLoadTournament";
			BtnLoadTournament.Size = new Size(86, 25);
			BtnLoadTournament.TabIndex = 1;
			BtnLoadTournament.Text = "Turnier laden";
			BtnLoadTournament.UseVisualStyleBackColor = true;
			// 
			// BtnExit
			// 
			BtnExit.AutoSize = true;
			BtnExit.Location = new Point(352, 318);
			BtnExit.Name = "BtnExit";
			BtnExit.Size = new Size(75, 25);
			BtnExit.TabIndex = 2;
			BtnExit.Text = "Beenden";
			BtnExit.UseVisualStyleBackColor = true;
			BtnExit.Click += BtnExit_Click;
			// 
			// Menu
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(806, 477);
			Controls.Add(BtnExit);
			Controls.Add(BtnLoadTournament);
			Controls.Add(BtnNewTournament);
			Name = "Menu";
			Text = "Tournament Managers";
			Load += Menu_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button BtnNewTournament;
        private Button BtnLoadTournament;
        private Button BtnExit;
    }
}
