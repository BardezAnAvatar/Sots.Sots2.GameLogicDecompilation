using Kerberos.Sots.Launcher.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace Kerberos.Sots.Launcher
{
	public class Form1 : Form
	{
		private LogicalOptions _opt = new LogicalOptions();
		private IContainer components;
		private Button playButton;
		private Button quitButton;
		private WebBrowser webBrowser1;
		private Button optionsButton;
		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel2;
		private Panel panel1;
		private Label versionLabel;
		public Form1(string[] args)
		{
			this.InitializeComponent();
			this.versionLabel.Text = "2.0.25092.5";
			if (args.Count<string>() >= 2 && args[0] == "+connect_lobby")
			{
				this._opt.SteamConnectID = args[1];
			}
		}
		private static bool AreDevModeModifierKeysHeld()
		{
			return (Control.ModifierKeys & (Keys.Control | Keys.Alt)) == (Keys.Control | Keys.Alt);
		}
		private void playButton_Click(object sender, EventArgs e)
		{
            string path = "..\\x86\\mars.exe";
            string directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            directoryName = @"C:\Program Files (x86)\Steam\SteamApps\common\sword of the stars ii\bin\x86";
			string fullPath = Path.GetFullPath(Path.Combine(directoryName, path));
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = fullPath,
				WorkingDirectory = directoryName,
				Arguments = this._opt.ToCommandLineString(),
				ErrorDialog = true
			};
			if (Form1.AreDevModeModifierKeysHeld())
			{
				MessageBox.Show(string.Format("Launching '{0}' with the following command line arguments:\n\n{1}", processStartInfo.FileName, processStartInfo.Arguments), "Launching Game", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			Process.Start(processStartInfo);
			//Application.Exit();
		}
		private void optionsButton_Click(object sender, EventArgs e)
		{
			OptionsDialog optionsDialog = new OptionsDialog(Form1.AreDevModeModifierKeysHeld() || Settings.Default.DevMode);
			this._opt.LoadFromSettings();
			optionsDialog.CopyOptionsToDialog(this._opt);
			DialogResult dialogResult = optionsDialog.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				optionsDialog.CopyOptionsFromDialog(this._opt);
				this._opt.SaveToSettings();
			}
		}
		private void quitButton_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
			this.playButton = new Button();
			this.quitButton = new Button();
			this.webBrowser1 = new WebBrowser();
			this.optionsButton = new Button();
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.tableLayoutPanel2 = new TableLayoutPanel();
			this.panel1 = new Panel();
			this.versionLabel = new Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.playButton.Dock = DockStyle.Fill;
			this.playButton.Location = new Point(84, 18);
			this.playButton.Name = "playButton";
			this.playButton.Size = new Size(642, 24);
			this.playButton.TabIndex = 0;
			this.playButton.Text = "Play";
			this.playButton.UseVisualStyleBackColor = true;
			this.playButton.Click += new EventHandler(this.playButton_Click);
			this.quitButton.Dock = DockStyle.Fill;
			this.quitButton.Location = new Point(84, 93);
			this.quitButton.Name = "quitButton";
			this.quitButton.Size = new Size(642, 24);
			this.quitButton.TabIndex = 0;
			this.quitButton.Text = "Quit";
			this.quitButton.UseVisualStyleBackColor = true;
			this.quitButton.Click += new EventHandler(this.quitButton_Click);
			this.webBrowser1.DataBindings.Add(new Binding("Url", Settings.Default, "HomePageURL", true, DataSourceUpdateMode.OnPropertyChanged));
			this.webBrowser1.Dock = DockStyle.Fill;
			this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
			this.webBrowser1.Location = new Point(0, 0);
			this.webBrowser1.MinimumSize = new Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.ScriptErrorsSuppressed = true;
			this.webBrowser1.Size = new Size(807, 508);
			this.webBrowser1.TabIndex = 1;
			this.webBrowser1.Url = Settings.Default.HomePageURL;
			this.webBrowser1.WebBrowserShortcutsEnabled = false;
			this.optionsButton.Dock = DockStyle.Fill;
			this.optionsButton.Location = new Point(84, 48);
			this.optionsButton.Name = "optionsButton";
			this.optionsButton.Size = new Size(642, 24);
			this.optionsButton.TabIndex = 0;
			this.optionsButton.Text = "Options...";
			this.optionsButton.UseVisualStyleBackColor = true;
			this.optionsButton.Click += new EventHandler(this.optionsButton_Click);
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.versionLabel, 0, 2);
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel1.Location = new Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 125f));
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
			this.tableLayoutPanel1.Size = new Size(817, 663);
			this.tableLayoutPanel1.TabIndex = 2;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));
			this.tableLayoutPanel2.Controls.Add(this.playButton, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.optionsButton, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.quitButton, 1, 4);
			this.tableLayoutPanel2.Dock = DockStyle.Fill;
			this.tableLayoutPanel2.Location = new Point(3, 521);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 6;
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 15f));
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 15f));
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 1f));
			this.tableLayoutPanel2.Size = new Size(811, 119);
			this.tableLayoutPanel2.TabIndex = 0;
			this.panel1.BorderStyle = BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.webBrowser1);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(811, 512);
			this.panel1.TabIndex = 1;
			this.versionLabel.AutoSize = true;
			this.versionLabel.Dock = DockStyle.Fill;
			this.versionLabel.Location = new Point(3, 643);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new Size(811, 20);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.TextAlign = ContentAlignment.BottomRight;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.Window;
			base.ClientSize = new Size(817, 663);
			base.Controls.Add(this.tableLayoutPanel1);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "Form1";
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Sword of the Stars II";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
