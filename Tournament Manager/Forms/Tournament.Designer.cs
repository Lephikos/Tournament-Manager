namespace Tournament_Manager.Forms
{
	partial class Tournament
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
			label1 = new Label();
			listBoxPlayers = new ListBox();
			txtBoxNamePlayer = new TextBox();
			txtBoxRatingPlayer = new TextBox();
			btnAddPlayer = new Button();
			label2 = new Label();
			label3 = new Label();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(25, 25);
			label1.Name = "label1";
			label1.Size = new Size(44, 15);
			label1.TabIndex = 0;
			label1.Text = "Players";
			// 
			// listBoxPlayers
			// 
			listBoxPlayers.FormattingEnabled = true;
			listBoxPlayers.ItemHeight = 15;
			listBoxPlayers.Location = new Point(25, 55);
			listBoxPlayers.Name = "listBoxPlayers";
			listBoxPlayers.Size = new Size(242, 244);
			listBoxPlayers.TabIndex = 1;
			// 
			// txtBoxNamePlayer
			// 
			txtBoxNamePlayer.Location = new Point(35, 332);
			txtBoxNamePlayer.Name = "txtBoxNamePlayer";
			txtBoxNamePlayer.Size = new Size(154, 23);
			txtBoxNamePlayer.TabIndex = 2;
			// 
			// txtBoxRatingPlayer
			// 
			txtBoxRatingPlayer.Location = new Point(36, 371);
			txtBoxRatingPlayer.Name = "txtBoxRatingPlayer";
			txtBoxRatingPlayer.Size = new Size(153, 23);
			txtBoxRatingPlayer.TabIndex = 3;
			// 
			// btnAddPlayer
			// 
			btnAddPlayer.Location = new Point(36, 413);
			btnAddPlayer.Name = "btnAddPlayer";
			btnAddPlayer.Size = new Size(75, 23);
			btnAddPlayer.TabIndex = 4;
			btnAddPlayer.Text = "Add Player";
			btnAddPlayer.UseVisualStyleBackColor = true;
			btnAddPlayer.Click += btnAddPlayer_Click;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(195, 335);
			label2.Name = "label2";
			label2.Size = new Size(72, 15);
			label2.TabIndex = 5;
			label2.Text = "Player name";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(195, 379);
			label3.Name = "label3";
			label3.Size = new Size(73, 15);
			label3.TabIndex = 6;
			label3.Text = "Player rating";
			// 
			// Tournament
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(btnAddPlayer);
			Controls.Add(txtBoxRatingPlayer);
			Controls.Add(txtBoxNamePlayer);
			Controls.Add(listBoxPlayers);
			Controls.Add(label1);
			Name = "Tournament";
			Text = "Tournament";
			Load += Tournament_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label label1;
		private ListBox listBoxPlayers;
		private TextBox txtBoxNamePlayer;
		private TextBox txtBoxRatingPlayer;
		private Button btnAddPlayer;
		private Label label2;
		private Label label3;
	}
}