using Kerberos.Sots.Launcher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace Kerberos.Sots.Launcher
{
	public class OptionsDialog : Form
	{
		private class LanguageItem
		{
			public CultureInfo CultureInfo
			{
				get;
				private set;
			}
			public LanguageItem(CultureInfo cultureInfo)
			{
				this.CultureInfo = cultureInfo;
			}
			public override string ToString()
			{
				return this.CultureInfo.DisplayName;
			}
		}
		private IContainer components;
		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel2;
		private Button okButton;
		private Button cancelButton;
		private Panel panel1;
		private Label label2;
		private Label label1;
		private TextBox screenHeightBox;
		private TextBox screenWidthBox;
		private CheckBox focalBlurCheckBox;
		private CheckBox refractionCheckBox;
		private CheckBox bloomCheckBox;
		private CheckBox windowedCheckBox;
		private CheckBox ambientOcclusionCheckBox;
		private TrackBar textureQualityTrackBar;
		private Label label3;
		private Label label6;
		private Label label5;
		private CheckBox allowConsole;
		private CheckBox proceduralPlanetsCheckBox;
		private CheckBox decals;
		private Button copyCommandLineButton;
		private CheckBox fxaaCheckBox;
		private Label languageLabel;
		private ComboBox language;
		private GroupBox netGroup;
		private TextBox netListen;
		private TextBox netMTU;
		private Label netMTUSuffix;
		private Label netListenLabel;
		private Label netMTULabel;
		private CheckBox customNetworkSettings;
		private CheckBox windowedFullscreenCheckbox;
		private bool IsDevModeEnabled
		{
			get;
			set;
		}
		public void SyncUI()
		{
			if (!this.IsDevModeEnabled)
			{
				this.allowConsole.Hide();
				this.copyCommandLineButton.Hide();
			}
			if (this.customNetworkSettings.Checked)
			{
				this.netGroup.Visible = true;
				return;
			}
			this.netGroup.Visible = false;
		}
		public OptionsDialog(bool devModeEnabled)
		{
			this.IsDevModeEnabled = devModeEnabled;
			this.InitializeComponent();
			this.SyncUI();
		}
		private static List<CultureInfo> GetAvailableCultureInfos()
		{
			List<CultureInfo> list = new List<CultureInfo>();
			string fullPath = Path.GetFullPath(Path.Combine(string.Format(Settings.Default.AssetRoot, "base"), "locale"));
			if (Directory.Exists(fullPath))
			{
				foreach (string current in Directory.EnumerateDirectories(fullPath, "*", SearchOption.TopDirectoryOnly))
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(current);
					CultureInfo cultureInfo;
					try
					{
						cultureInfo = CultureInfo.GetCultureInfo(fileNameWithoutExtension);
					}
					catch (CultureNotFoundException)
					{
						continue;
					}
					if (cultureInfo.TwoLetterISOLanguageName.Equals(fileNameWithoutExtension, StringComparison.InvariantCultureIgnoreCase))
					{
						list.Add(cultureInfo);
					}
				}
			}
			return list;
		}
		private void CopyLanguageToDialog(string defaultISOTwoLetterName)
		{
			this.language.Items.Clear();
			this.language.Items.Add("Automatic");
			object selectedItem = this.language.Items[0];
			this.language.Items.AddRange((
				from x in OptionsDialog.GetAvailableCultureInfos()
				select new OptionsDialog.LanguageItem(x)).Cast<object>().ToArray<object>());
			if (!string.IsNullOrEmpty(defaultISOTwoLetterName))
			{
				foreach (OptionsDialog.LanguageItem current in this.language.Items.OfType<OptionsDialog.LanguageItem>())
				{
					if (current.CultureInfo.TwoLetterISOLanguageName.Equals(defaultISOTwoLetterName, StringComparison.InvariantCultureIgnoreCase))
					{
						selectedItem = current;
						break;
					}
				}
			}
			this.language.SelectedItem = selectedItem;
		}
		public void CopyOptionsToDialog(LogicalOptions value)
		{
			this.screenWidthBox.Text = value.Width.ToString();
			this.screenHeightBox.Text = value.Height.ToString();
			this.windowedCheckBox.Checked = value.Windowed;
			this.windowedFullscreenCheckbox.Checked = value.WindowedFullscreen;
			this.fxaaCheckBox.Checked = value.FXAA;
			this.bloomCheckBox.Checked = value.Bloom;
			this.focalBlurCheckBox.Checked = value.FocalBlur;
			this.refractionCheckBox.Checked = value.Refraction;
			this.ambientOcclusionCheckBox.Checked = value.AmbientOcclusion;
			this.proceduralPlanetsCheckBox.Checked = value.ProceduralPlanets;
			this.decals.Checked = value.Decals;
			this.allowConsole.Checked = value.AllowConsole;
			this.textureQualityTrackBar.Value = value.TextureQuality;
			this.customNetworkSettings.Checked = value.CustomNetworkSettings;
			this.netMTU.Text = value.NetworkMTUAffinity.ToString();
			this.netListen.Text = value.NetworkListenPort.ToString();
			this.CopyLanguageToDialog(value.TwoLetterISOLanguageName);
		}
		public void CopyOptionsFromDialog(LogicalOptions value)
		{
			value.Width = int.Parse(this.screenWidthBox.Text);
			value.Height = int.Parse(this.screenHeightBox.Text);
			value.Windowed = this.windowedCheckBox.Checked;
			value.WindowedFullscreen = this.windowedFullscreenCheckbox.Checked;
			value.FXAA = this.fxaaCheckBox.Checked;
			value.Bloom = this.bloomCheckBox.Checked;
			value.FocalBlur = this.focalBlurCheckBox.Checked;
			value.Refraction = this.refractionCheckBox.Checked;
			value.AmbientOcclusion = this.ambientOcclusionCheckBox.Checked;
			value.ProceduralPlanets = this.proceduralPlanetsCheckBox.Checked;
			value.Decals = this.decals.Checked;
			value.AllowConsole = this.allowConsole.Checked;
			value.TextureQuality = this.textureQualityTrackBar.Value;
			value.CustomNetworkSettings = this.customNetworkSettings.Checked;
			value.NetworkMTUAffinity = uint.Parse(this.netMTU.Text);
			value.NetworkListenPort = ushort.Parse(this.netListen.Text);
			value.TwoLetterISOLanguageName = ((this.language.SelectedItem is OptionsDialog.LanguageItem) ? (this.language.SelectedItem as OptionsDialog.LanguageItem).CultureInfo.TwoLetterISOLanguageName : string.Empty);
		}
		private bool IsValidWidthHeightText(string value)
		{
			int num;
			return !string.IsNullOrEmpty(value) && int.TryParse(value, out num) && num > 8 && num <= 65536;
		}
		private bool IsValidPortText(string value)
		{
			ushort num;
			return !string.IsNullOrEmpty(value) && ushort.TryParse(value, out num) && num >= 1024;
		}
		private bool IsValidMTUSizeText(string value)
		{
			uint num;
			return !string.IsNullOrEmpty(value) && uint.TryParse(value, out num) && num >= 128u && num <= 65536u;
		}
		private void screenWidthBox_Validating(object sender, CancelEventArgs e)
		{
			e.Cancel = !this.IsValidWidthHeightText(this.screenWidthBox.Text);
		}
		private void screenHeightBox_Validating(object sender, CancelEventArgs e)
		{
			e.Cancel = !this.IsValidWidthHeightText(this.screenHeightBox.Text);
		}
		private void netMTU_Validating(object sender, CancelEventArgs e)
		{
			e.Cancel = !this.IsValidMTUSizeText(this.netMTU.Text);
		}
		private void netListen_Validating(object sender, CancelEventArgs e)
		{
			e.Cancel = !this.IsValidPortText(this.netListen.Text);
		}
		private void copyCommandLineButton_Click(object sender, EventArgs e)
		{
			LogicalOptions logicalOptions = new LogicalOptions();
			this.CopyOptionsFromDialog(logicalOptions);
			string text = logicalOptions.ToCommandLineString();
			Clipboard.SetText(text);
			MessageBox.Show("The following command line has been copied to the clipboard:\n\n" + text, "Command Line Copied to Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		private void customNetworkSettings_CheckedChanged(object sender, EventArgs e)
		{
			this.SyncUI();
		}
		private void OnWindowedChanged(object sender, EventArgs e)
		{
			this.windowedFullscreenCheckbox.Enabled = this.windowedCheckBox.Checked;
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(OptionsDialog));
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.tableLayoutPanel2 = new TableLayoutPanel();
			this.okButton = new Button();
			this.cancelButton = new Button();
			this.panel1 = new Panel();
			this.windowedFullscreenCheckbox = new CheckBox();
			this.netGroup = new GroupBox();
			this.netListen = new TextBox();
			this.netMTU = new TextBox();
			this.netMTUSuffix = new Label();
			this.netListenLabel = new Label();
			this.netMTULabel = new Label();
			this.languageLabel = new Label();
			this.language = new ComboBox();
			this.copyCommandLineButton = new Button();
			this.textureQualityTrackBar = new TrackBar();
			this.label2 = new Label();
			this.label6 = new Label();
			this.label5 = new Label();
			this.label3 = new Label();
			this.label1 = new Label();
			this.screenHeightBox = new TextBox();
			this.screenWidthBox = new TextBox();
			this.customNetworkSettings = new CheckBox();
			this.allowConsole = new CheckBox();
			this.decals = new CheckBox();
			this.proceduralPlanetsCheckBox = new CheckBox();
			this.ambientOcclusionCheckBox = new CheckBox();
			this.focalBlurCheckBox = new CheckBox();
			this.refractionCheckBox = new CheckBox();
			this.fxaaCheckBox = new CheckBox();
			this.bloomCheckBox = new CheckBox();
			this.windowedCheckBox = new CheckBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.netGroup.SuspendLayout();
			((ISupportInitialize)this.textureQualityTrackBar).BeginInit();
			base.SuspendLayout();
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel1.Location = new Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
			this.tableLayoutPanel1.Size = new Size(549, 315);
			this.tableLayoutPanel1.TabIndex = 0;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333f));
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333f));
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333f));
			this.tableLayoutPanel2.Controls.Add(this.okButton, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.cancelButton, 2, 0);
			this.tableLayoutPanel2.Dock = DockStyle.Fill;
			this.tableLayoutPanel2.Location = new Point(3, 276);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel2.Size = new Size(543, 36);
			this.tableLayoutPanel2.TabIndex = 1;
			this.okButton.DialogResult = DialogResult.OK;
			this.okButton.Dock = DockStyle.Fill;
			this.okButton.Location = new Point(184, 3);
			this.okButton.Name = "okButton";
			this.okButton.Size = new Size(175, 30);
			this.okButton.TabIndex = 5001;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.cancelButton.Dock = DockStyle.Fill;
			this.cancelButton.Location = new Point(365, 3);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(175, 30);
			this.cancelButton.TabIndex = 5002;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.panel1.Controls.Add(this.windowedFullscreenCheckbox);
			this.panel1.Controls.Add(this.netGroup);
			this.panel1.Controls.Add(this.languageLabel);
			this.panel1.Controls.Add(this.language);
			this.panel1.Controls.Add(this.copyCommandLineButton);
			this.panel1.Controls.Add(this.textureQualityTrackBar);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label6);
			this.panel1.Controls.Add(this.label5);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.screenHeightBox);
			this.panel1.Controls.Add(this.screenWidthBox);
			this.panel1.Controls.Add(this.customNetworkSettings);
			this.panel1.Controls.Add(this.allowConsole);
			this.panel1.Controls.Add(this.decals);
			this.panel1.Controls.Add(this.proceduralPlanetsCheckBox);
			this.panel1.Controls.Add(this.ambientOcclusionCheckBox);
			this.panel1.Controls.Add(this.focalBlurCheckBox);
			this.panel1.Controls.Add(this.refractionCheckBox);
			this.panel1.Controls.Add(this.fxaaCheckBox);
			this.panel1.Controls.Add(this.bloomCheckBox);
			this.panel1.Controls.Add(this.windowedCheckBox);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(543, 267);
			this.panel1.TabIndex = 2;
			this.windowedFullscreenCheckbox.AutoSize = true;
			this.windowedFullscreenCheckbox.Enabled = false;
			this.windowedFullscreenCheckbox.Location = new Point(9, 29);
			this.windowedFullscreenCheckbox.Name = "windowedFullscreenCheckbox";
			this.windowedFullscreenCheckbox.Size = new Size(128, 17);
			this.windowedFullscreenCheckbox.TabIndex = 3101;
			this.windowedFullscreenCheckbox.Text = "W&indowed Fullscreen";
			this.windowedFullscreenCheckbox.UseVisualStyleBackColor = true;
			this.netGroup.Controls.Add(this.netListen);
			this.netGroup.Controls.Add(this.netMTU);
			this.netGroup.Controls.Add(this.netMTUSuffix);
			this.netGroup.Controls.Add(this.netListenLabel);
			this.netGroup.Controls.Add(this.netMTULabel);
			this.netGroup.Location = new Point(297, 102);
			this.netGroup.Name = "netGroup";
			this.netGroup.Size = new Size(237, 75);
			this.netGroup.TabIndex = 2500;
			this.netGroup.TabStop = false;
			this.netGroup.Text = "Advanced Network Settings";
			this.netListen.Location = new Point(105, 45);
			this.netListen.Name = "netListen";
			this.netListen.Size = new Size(91, 20);
			this.netListen.TabIndex = 2521;
			this.netListen.Validating += new CancelEventHandler(this.netListen_Validating);
			this.netMTU.Location = new Point(105, 22);
			this.netMTU.Name = "netMTU";
			this.netMTU.Size = new Size(91, 20);
			this.netMTU.TabIndex = 2511;
			this.netMTU.Validating += new CancelEventHandler(this.netMTU_Validating);
			this.netMTUSuffix.AutoSize = true;
			this.netMTUSuffix.Location = new Point(199, 25);
			this.netMTUSuffix.Name = "netMTUSuffix";
			this.netMTUSuffix.Size = new Size(32, 13);
			this.netMTUSuffix.TabIndex = 2512;
			this.netMTUSuffix.Text = "bytes";
			this.netListenLabel.AutoSize = true;
			this.netListenLabel.Location = new Point(39, 48);
			this.netListenLabel.Name = "netListenLabel";
			this.netListenLabel.Size = new Size(60, 13);
			this.netListenLabel.TabIndex = 2520;
			this.netListenLabel.Text = "&Listen Port:";
			this.netMTULabel.AutoSize = true;
			this.netMTULabel.Location = new Point(31, 25);
			this.netMTULabel.Name = "netMTULabel";
			this.netMTULabel.Size = new Size(68, 13);
			this.netMTULabel.TabIndex = 2510;
			this.netMTULabel.Text = "&MTU Affinity:";
			this.languageLabel.AutoSize = true;
			this.languageLabel.Location = new Point(248, 197);
			this.languageLabel.Name = "languageLabel";
			this.languageLabel.Size = new Size(58, 13);
			this.languageLabel.TabIndex = 3001;
			this.languageLabel.Text = "&Language:";
			this.language.DropDownStyle = ComboBoxStyle.DropDownList;
			this.language.FormattingEnabled = true;
			this.language.Location = new Point(312, 193);
			this.language.Name = "language";
			this.language.Size = new Size(222, 21);
			this.language.TabIndex = 3002;
			this.copyCommandLineButton.BackColor = Color.FromArgb(255, 192, 192);
			this.copyCommandLineButton.Location = new Point(245, 226);
			this.copyCommandLineButton.Name = "copyCommandLineButton";
			this.copyCommandLineButton.Size = new Size(289, 30);
			this.copyCommandLineButton.TabIndex = 3100;
			this.copyCommandLineButton.Text = "Copy Command &Line to Clipboard";
			this.copyCommandLineButton.UseVisualStyleBackColor = false;
			this.copyCommandLineButton.Click += new EventHandler(this.copyCommandLineButton_Click);
			this.textureQualityTrackBar.Location = new Point(335, 60);
			this.textureQualityTrackBar.Maximum = 5;
			this.textureQualityTrackBar.Name = "textureQualityTrackBar";
			this.textureQualityTrackBar.Size = new Size(199, 42);
			this.textureQualityTrackBar.TabIndex = 2011;
			this.textureQualityTrackBar.Tag = "";
			this.textureQualityTrackBar.Value = 5;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(429, 11);
			this.label2.Name = "label2";
			this.label2.Size = new Size(12, 13);
			this.label2.TabIndex = 1002;
			this.label2.Text = "x";
			this.label6.AutoSize = true;
			this.label6.Location = new Point(332, 44);
			this.label6.Name = "label6";
			this.label6.Size = new Size(27, 13);
			this.label6.TabIndex = 2001;
			this.label6.Text = "Low";
			this.label5.AutoSize = true;
			this.label5.Location = new Point(505, 44);
			this.label5.Name = "label5";
			this.label5.Size = new Size(29, 13);
			this.label5.TabIndex = 2002;
			this.label5.Text = "High";
			this.label3.AutoSize = true;
			this.label3.Location = new Point(242, 60);
			this.label3.Name = "label3";
			this.label3.Size = new Size(81, 13);
			this.label3.TabIndex = 2010;
			this.label3.Text = "Texture Quality:";
			this.label1.AutoSize = true;
			this.label1.Location = new Point(227, 10);
			this.label1.Name = "label1";
			this.label1.Size = new Size(102, 13);
			this.label1.TabIndex = 1000;
			this.label1.Text = "Window Resolution:";
			this.screenHeightBox.Location = new Point(443, 7);
			this.screenHeightBox.Name = "screenHeightBox";
			this.screenHeightBox.Size = new Size(91, 20);
			this.screenHeightBox.TabIndex = 1003;
			this.screenHeightBox.Validating += new CancelEventHandler(this.screenHeightBox_Validating);
			this.screenWidthBox.Location = new Point(335, 7);
			this.screenWidthBox.Name = "screenWidthBox";
			this.screenWidthBox.Size = new Size(91, 20);
			this.screenWidthBox.TabIndex = 1001;
			this.screenWidthBox.Validating += new CancelEventHandler(this.screenWidthBox_Validating);
			this.customNetworkSettings.AutoSize = true;
			this.customNetworkSettings.BackColor = SystemColors.Control;
			this.customNetworkSettings.Enabled = false;
			this.customNetworkSettings.Location = new Point(9, 209);
			this.customNetworkSettings.Name = "customNetworkSettings";
			this.customNetworkSettings.Size = new Size(181, 17);
			this.customNetworkSettings.TabIndex = 10;
			this.customNetworkSettings.Text = "Use Advanced Net&work Settings";
			this.customNetworkSettings.UseVisualStyleBackColor = false;
			this.customNetworkSettings.CheckedChanged += new EventHandler(this.customNetworkSettings_CheckedChanged);
			this.allowConsole.AutoSize = true;
			this.allowConsole.BackColor = Color.FromArgb(255, 192, 192);
			this.allowConsole.Location = new Point(9, 231);
			this.allowConsole.Name = "allowConsole";
			this.allowConsole.Size = new Size(92, 17);
			this.allowConsole.TabIndex = 11;
			this.allowConsole.Text = "Allow &Console";
			this.allowConsole.UseVisualStyleBackColor = false;
			this.decals.AutoSize = true;
			this.decals.Location = new Point(9, 187);
			this.decals.Name = "decals";
			this.decals.Size = new Size(59, 17);
			this.decals.TabIndex = 8;
			this.decals.Text = "&Decals";
			this.decals.UseVisualStyleBackColor = true;
			this.proceduralPlanetsCheckBox.AutoSize = true;
			this.proceduralPlanetsCheckBox.Location = new Point(9, 164);
			this.proceduralPlanetsCheckBox.Name = "proceduralPlanetsCheckBox";
			this.proceduralPlanetsCheckBox.Size = new Size(115, 17);
			this.proceduralPlanetsCheckBox.TabIndex = 7;
			this.proceduralPlanetsCheckBox.Text = "&Procedural Planets";
			this.proceduralPlanetsCheckBox.UseVisualStyleBackColor = true;
			this.ambientOcclusionCheckBox.AutoSize = true;
			this.ambientOcclusionCheckBox.Location = new Point(9, 141);
			this.ambientOcclusionCheckBox.Name = "ambientOcclusionCheckBox";
			this.ambientOcclusionCheckBox.Size = new Size(114, 17);
			this.ambientOcclusionCheckBox.TabIndex = 6;
			this.ambientOcclusionCheckBox.Text = "&Ambient Occlusion";
			this.ambientOcclusionCheckBox.UseVisualStyleBackColor = true;
			this.focalBlurCheckBox.AutoSize = true;
			this.focalBlurCheckBox.Location = new Point(9, 118);
			this.focalBlurCheckBox.Name = "focalBlurCheckBox";
			this.focalBlurCheckBox.Size = new Size(73, 17);
			this.focalBlurCheckBox.TabIndex = 5;
			this.focalBlurCheckBox.Text = "&Focal Blur";
			this.focalBlurCheckBox.UseVisualStyleBackColor = true;
			this.refractionCheckBox.AutoSize = true;
			this.refractionCheckBox.Location = new Point(9, 95);
			this.refractionCheckBox.Name = "refractionCheckBox";
			this.refractionCheckBox.Size = new Size(75, 17);
			this.refractionCheckBox.TabIndex = 4;
			this.refractionCheckBox.Text = "&Refraction";
			this.refractionCheckBox.UseVisualStyleBackColor = true;
			this.fxaaCheckBox.AutoSize = true;
			this.fxaaCheckBox.Location = new Point(9, 49);
			this.fxaaCheckBox.Name = "fxaaCheckBox";
			this.fxaaCheckBox.Size = new Size(115, 17);
			this.fxaaCheckBox.TabIndex = 2;
			this.fxaaCheckBox.Text = "A&ntialiasing (FXAA)";
			this.fxaaCheckBox.UseVisualStyleBackColor = true;
			this.bloomCheckBox.AutoSize = true;
			this.bloomCheckBox.Location = new Point(9, 72);
			this.bloomCheckBox.Name = "bloomCheckBox";
			this.bloomCheckBox.Size = new Size(55, 17);
			this.bloomCheckBox.TabIndex = 3;
			this.bloomCheckBox.Text = "&Bloom";
			this.bloomCheckBox.UseVisualStyleBackColor = true;
			this.windowedCheckBox.AutoSize = true;
			this.windowedCheckBox.Location = new Point(9, 9);
			this.windowedCheckBox.Name = "windowedCheckBox";
			this.windowedCheckBox.Size = new Size(77, 17);
			this.windowedCheckBox.TabIndex = 1;
			this.windowedCheckBox.Text = "&Windowed";
			this.windowedCheckBox.UseVisualStyleBackColor = true;
			this.windowedCheckBox.CheckedChanged += new EventHandler(this.OnWindowedChanged);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(549, 315);
			base.Controls.Add(this.tableLayoutPanel1);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "OptionsDialog";
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Sword of the Stars II Options";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.netGroup.ResumeLayout(false);
			this.netGroup.PerformLayout();
			((ISupportInitialize)this.textureQualityTrackBar).EndInit();
			base.ResumeLayout(false);
		}
	}
}
