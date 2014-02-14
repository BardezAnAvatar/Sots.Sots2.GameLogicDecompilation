using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
namespace Kerberos.Sots.Console
{
	public class Console : Form
	{
		private struct TextRun
		{
			public string Category;
			public bool RegisterHit;
			public int Start;
			public int Length;
			public Color Color;
		}
		private class CategoryInfo
		{
			public string Name;
			public int HitCount;
			public ListViewItem ListViewItem;
			public bool IsEnabled
			{
				get
				{
					return this.ListViewItem.Checked;
				}
			}
		}
		private class ConsoleProfile
		{
			public string Name;
			public List<string> Categories;
			public LogLevel LogLevel;
			public ToolStripMenuItem MenuItem;
		}
		private delegate void PostTextRunsDelegate(string textAdded, List<Console.TextRun> textRuns);
		private readonly Console.ConsoleProfile[] _profiles = new Console.ConsoleProfile[]
		{
			new Console.ConsoleProfile
			{
				Name = "AI Debugging",
				Categories = new List<string>
				{
					"ai",
					"con",
					"design",
					"game",
					"log"
				},
				LogLevel = LogLevel.Verbose
			},
			new Console.ConsoleProfile
			{
				Name = "Game Flow Debugging",
				Categories = new List<string>
				{
					"con",
					"state",
					"game",
					"log"
				},
				LogLevel = LogLevel.Verbose
			}
		};
		private readonly Timer _postTextTimer;
		private readonly StringBuilder _textAdded = new StringBuilder();
		private readonly List<Console.TextRun> _textRuns = new List<Console.TextRun>();
		private readonly Dictionary<string, Console.CategoryInfo> _categoryInfos = new Dictionary<string, Console.CategoryInfo>();
		private readonly Dictionary<LogLevel, ToolStripMenuItem> _logLevelItems = new Dictionary<LogLevel, ToolStripMenuItem>();
		private int MaxSize = 10000000;
		private IContainer components;
		private ShellControl shellControl1;
		private RichTextBox richTextBox1;
		private TableLayoutPanel tableLayoutPanel1;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem logToolStripMenuItem;
		private ListView listView1;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ToolStripMenuItem verbosityToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem openLiveLogFileToolStripMenuItem;
		private ToolStripMenuItem locateLiveLogFileToolStripMenuItem;
		private TextBox logPathTextBox;
		private ToolStripMenuItem profilesToolStripMenuItem;
		public event EventCommandEntered CommandEntered
		{
			add
			{
				this.shellControl1.CommandEntered += value;
			}
			remove
			{
				this.shellControl1.CommandEntered -= value;
			}
		}
		public Console()
		{
			this.InitializeComponent();
			this._postTextTimer = new Timer();
			this._postTextTimer.Interval = 500;
			this._postTextTimer.Tick += new EventHandler(this.PostTextTimerTick);
			this._postTextTimer.Start();
			RichTextBoxHelpers.InitializeForConsole(this.richTextBox1);
			this.richTextBox1.SelectionHangingIndent = 72;
			int num = 1;
			LogLevel[] array = (LogLevel[])Enum.GetValues(typeof(LogLevel));
			for (int i = 0; i < array.Length; i++)
			{
				LogLevel logLevel = array[i];
				this._logLevelItems[logLevel] = new ToolStripMenuItem(string.Format("(&{0}) {1}", num, logLevel), null, new EventHandler(this.OnMenuClick));
				num++;
			}
			this.verbosityToolStripMenuItem.DropDownItems.AddRange(this._logLevelItems.Values.ToArray<ToolStripMenuItem>());
			this.SynchronizeWithLogLevel();
			Console.ConsoleProfile[] profiles = this._profiles;
			for (int j = 0; j < profiles.Length; j++)
			{
				Console.ConsoleProfile consoleProfile = profiles[j];
				consoleProfile.MenuItem = new ToolStripMenuItem(consoleProfile.Name, null, new EventHandler(this.OnMenuClick));
				this.profilesToolStripMenuItem.DropDownItems.Add(consoleProfile.MenuItem);
			}
			this.logPathTextBox.Text = App.Log.FilePath;
			FieldInfo[] fields = typeof(LogCategories).GetFields();
			for (int k = 0; k < fields.Length; k++)
			{
				FieldInfo fieldInfo = fields[k];
				this.RegisterCategory(fieldInfo.GetRawConstantValue() as string);
			}
		}
		private void OnMenuClick(object sender, EventArgs eventArgs)
		{
			if (sender == this.openLiveLogFileToolStripMenuItem)
			{
				ShellHelper.ShellOpen(App.Log.FilePath);
				return;
			}
			if (sender == this.locateLiveLogFileToolStripMenuItem)
			{
				ShellHelper.ShellExplore(App.Log.FilePath);
				return;
			}
			foreach (KeyValuePair<LogLevel, ToolStripMenuItem> current in this._logLevelItems)
			{
				if (sender == current.Value)
				{
					App.Log.Level = current.Key;
					this.SynchronizeWithLogLevel();
					return;
				}
			}
			Console.ConsoleProfile[] profiles = this._profiles;
			for (int i = 0; i < profiles.Length; i++)
			{
				Console.ConsoleProfile consoleProfile = profiles[i];
				if (sender == consoleProfile.MenuItem)
				{
					this.ApplyProfile(consoleProfile);
					break;
				}
			}
		}
		private void ApplyProfile(Console.ConsoleProfile profile)
		{
			foreach (string current in this._categoryInfos.Keys)
			{
				bool value = profile.Categories.Contains(current);
				this.SetCategoryEnabled(current, value);
			}
			App.Log.Level = profile.LogLevel;
		}
		private void SynchronizeWithLogLevel()
		{
			LogLevel level = App.Log.Level;
			foreach (KeyValuePair<LogLevel, ToolStripMenuItem> current in this._logLevelItems)
			{
				current.Value.Checked = (current.Key == level);
			}
		}
		private Console.CategoryInfo RegisterCategory(string category)
		{
			Console.CategoryInfo categoryInfo = new Console.CategoryInfo();
			categoryInfo.Name = category;
			categoryInfo.ListViewItem = new ListViewItem();
			categoryInfo.ListViewItem.Text = categoryInfo.Name;
			categoryInfo.ListViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(categoryInfo.ListViewItem, string.Empty));
			categoryInfo.ListViewItem.Tag = categoryInfo;
			categoryInfo.ListViewItem.Checked = true;
			this.listView1.Items.Add(categoryInfo.ListViewItem);
			this._categoryInfos.Add(categoryInfo.Name, categoryInfo);
			return categoryInfo;
		}
		private void HitCategory(string category)
		{
			Console.CategoryInfo categoryInfo;
			if (!this._categoryInfos.TryGetValue(category, out categoryInfo))
			{
				categoryInfo = this.RegisterCategory(category);
			}
			categoryInfo.HitCount++;
			categoryInfo.ListViewItem.SubItems[1].Text = categoryInfo.HitCount.ToString("N0");
		}
		private void SetCategoryEnabled(string category, bool value)
		{
			if (string.IsNullOrEmpty(category))
			{
				return;
			}
			Console.CategoryInfo categoryInfo;
			if (!this._categoryInfos.TryGetValue(category, out categoryInfo))
			{
				return;
			}
			categoryInfo.ListViewItem.Checked = value;
		}
		private bool IsCategoryEnabled(string category)
		{
			Console.CategoryInfo categoryInfo;
			return string.IsNullOrEmpty(category) || !this._categoryInfos.TryGetValue(category, out categoryInfo) || categoryInfo.IsEnabled;
		}
		private void PostTextRuns(string textAdded, List<Console.TextRun> textRuns)
		{
			if (this.richTextBox1.InvokeRequired)
			{
				this.richTextBox1.Invoke(new Console.PostTextRunsDelegate(this.PostTextRuns), new object[]
				{
					textAdded,
					textRuns
				});
				return;
			}
			if (this.richTextBox1.TextLength > this.MaxSize)
			{
				this.richTextBox1.Clear();
			}
			bool flag = false;
			foreach (Console.TextRun current in textRuns)
			{
				if (this.IsCategoryEnabled(current.Category))
				{
					if (!flag)
					{
						this.richTextBox1.Select(this.richTextBox1.TextLength, 0);
					}
					this.richTextBox1.SelectionColor = current.Color;
					this.richTextBox1.AppendText(textAdded.Substring(current.Start, current.Length));
				}
			}
			if (flag)
			{
				this.richTextBox1.ScrollToCaret();
			}
		}
		private void PostTextTimerTick(object sender, EventArgs e)
		{
			lock (this._textAdded)
			{
				if (this._textAdded.Length > 0)
				{
					foreach (Console.TextRun current in this._textRuns)
					{
						if (current.RegisterHit && !string.IsNullOrEmpty(current.Category))
						{
							this.HitCategory(current.Category);
						}
					}
					this.PostTextRuns(this._textAdded.ToString(), new List<Console.TextRun>(this._textRuns));
					this._textRuns.Clear();
					this._textAdded.Length = 0;
				}
			}
		}
		public void WriteText(string category, bool registerHit, string s, Color color)
		{
			if (s.Length == 0)
			{
				return;
			}
			lock (this._textAdded)
			{
				this._textRuns.Add(new Console.TextRun
				{
					Category = category,
					RegisterHit = registerHit,
					Start = this._textAdded.Length,
					Length = s.Length,
					Color = color
				});
				this._textAdded.Append(s);
			}
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Console));
			this.richTextBox1 = new RichTextBox();
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.shellControl1 = new ShellControl();
			this.menuStrip1 = new MenuStrip();
			this.logToolStripMenuItem = new ToolStripMenuItem();
			this.verbosityToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator1 = new ToolStripSeparator();
			this.openLiveLogFileToolStripMenuItem = new ToolStripMenuItem();
			this.locateLiveLogFileToolStripMenuItem = new ToolStripMenuItem();
			this.listView1 = new ListView();
			this.columnHeader1 = new ColumnHeader();
			this.columnHeader2 = new ColumnHeader();
			this.logPathTextBox = new TextBox();
			this.profilesToolStripMenuItem = new ToolStripMenuItem();
			this.tableLayoutPanel1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			base.SuspendLayout();
			this.richTextBox1.BackColor = Color.FromArgb(0, 0, 64);
			this.richTextBox1.Dock = DockStyle.Fill;
			this.richTextBox1.Font = new System.Drawing.Font("Lucida Console", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.richTextBox1.ForeColor = Color.LightGray;
			this.richTextBox1.Location = new Point(113, 29);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
			this.richTextBox1.Size = new System.Drawing.Size(843, 318);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = "";
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.shellControl1, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.menuStrip1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.listView1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.logPathTextBox, 1, 0);
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel1.Location = new Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 100f));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(959, 450);
			this.tableLayoutPanel1.TabIndex = 2;
			this.shellControl1.Dock = DockStyle.Fill;
			this.shellControl1.Location = new Point(113, 353);
			this.shellControl1.Name = "shellControl1";
			this.shellControl1.Prompt = "> ";
			this.shellControl1.ShellTextBackColor = Color.Black;
			this.shellControl1.ShellTextFont = new System.Drawing.Font("Lucida Console", 10f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.shellControl1.ShellTextForeColor = Color.LightGray;
			this.shellControl1.Size = new System.Drawing.Size(843, 94);
			this.shellControl1.TabIndex = 0;
			this.menuStrip1.Items.AddRange(new ToolStripItem[]
			{
				this.logToolStripMenuItem
			});
			this.menuStrip1.Location = new Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(110, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			this.logToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				this.verbosityToolStripMenuItem,
				this.profilesToolStripMenuItem,
				this.toolStripSeparator1,
				this.openLiveLogFileToolStripMenuItem,
				this.locateLiveLogFileToolStripMenuItem
			});
			this.logToolStripMenuItem.Name = "logToolStripMenuItem";
			this.logToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.logToolStripMenuItem.Text = "&Log";
			this.verbosityToolStripMenuItem.Name = "verbosityToolStripMenuItem";
			this.verbosityToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.verbosityToolStripMenuItem.Text = "&Verbosity";
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(214, 6);
			this.openLiveLogFileToolStripMenuItem.Name = "openLiveLogFileToolStripMenuItem";
			this.openLiveLogFileToolStripMenuItem.ShortcutKeys = (Keys)131151;
			this.openLiveLogFileToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.openLiveLogFileToolStripMenuItem.Text = "&Open Live Log File";
			this.openLiveLogFileToolStripMenuItem.Click += new EventHandler(this.OnMenuClick);
			this.locateLiveLogFileToolStripMenuItem.Name = "locateLiveLogFileToolStripMenuItem";
			this.locateLiveLogFileToolStripMenuItem.ShortcutKeys = (Keys)131148;
			this.locateLiveLogFileToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.locateLiveLogFileToolStripMenuItem.Text = "&Locate Live Log File";
			this.locateLiveLogFileToolStripMenuItem.Click += new EventHandler(this.OnMenuClick);
			this.listView1.BackColor = Color.FromArgb(0, 0, 64);
			this.listView1.CheckBoxes = true;
			this.listView1.Columns.AddRange(new ColumnHeader[]
			{
				this.columnHeader1,
				this.columnHeader2
			});
			this.listView1.Dock = DockStyle.Fill;
			this.listView1.ForeColor = Color.Orchid;
			this.listView1.HeaderStyle = ColumnHeaderStyle.None;
			this.listView1.Location = new Point(3, 29);
			this.listView1.Name = "listView1";
			this.tableLayoutPanel1.SetRowSpan(this.listView1, 2);
			this.listView1.Size = new System.Drawing.Size(104, 418);
			this.listView1.Sorting = SortOrder.Ascending;
			this.listView1.TabIndex = 3;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = View.Details;
			this.columnHeader1.Text = "Category";
			this.columnHeader2.Text = "Count";
			this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader2.Width = 40;
			this.logPathTextBox.Dock = DockStyle.Fill;
			this.logPathTextBox.Location = new Point(113, 3);
			this.logPathTextBox.Name = "logPathTextBox";
			this.logPathTextBox.ReadOnly = true;
			this.logPathTextBox.Size = new System.Drawing.Size(843, 20);
			this.logPathTextBox.TabIndex = 4;
			this.profilesToolStripMenuItem.Name = "profilesToolStripMenuItem";
			this.profilesToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.profilesToolStripMenuItem.Text = "&Profiles";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(959, 450);
			base.Controls.Add(this.tableLayoutPanel1);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.MainMenuStrip = this.menuStrip1;
			base.Name = "Console";
			base.ShowIcon = false;
			this.Text = "Konsole";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
