using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace Kerberos.Sots.Console
{
	internal class ShellTextBox : TextBox
	{
		private string prompt = "> ";
		private CommandHistory commandHistory = new CommandHistory();
		private Container components;
		public string Prompt
		{
			get
			{
				return this.prompt;
			}
			set
			{
				this.SetPromptText(value);
			}
		}
		internal ShellTextBox()
		{
			this.InitializeComponent();
			this.printPrompt();
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg != 12)
			{
				switch (msg)
				{
				case 768:
				case 770:
					break;
				case 769:
					goto IL_3B;
				case 771:
					return;
				default:
					goto IL_3B;
				}
			}
			if (!this.IsCaretAtWritablePosition())
			{
				this.MoveCaretToEndOfText();
			}
			IL_3B:
			base.WndProc(ref m);
		}
		private void InitializeComponent()
		{
			base.SuspendLayout();
			this.BackColor = Color.Black;
			this.Dock = DockStyle.Fill;
			this.ForeColor = Color.Gray;
			base.Location = new Point(0, 0);
			this.MaxLength = 0;
			this.Multiline = true;
			base.Name = "shellTextBox";
			base.AcceptsTab = true;
			base.AcceptsReturn = true;
			base.ScrollBars = ScrollBars.Both;
			base.Size = new Size(400, 176);
			base.TabIndex = 0;
			this.Text = "";
			base.KeyPress += new KeyPressEventHandler(this.shellTextBox_KeyPress);
			base.KeyDown += new KeyEventHandler(this.ShellControl_KeyDown);
			base.Name = "ShellTextBox";
			base.Size = new Size(400, 176);
			base.ResumeLayout(false);
		}
		private void printPrompt()
		{
			string text = this.Text;
			if (text.Length != 0 && text[text.Length - 1] != '\n')
			{
				this.printLine();
			}
			this.AddText(this.prompt);
		}
		private void printLine()
		{
			this.AddText(Environment.NewLine);
		}
		private void shellTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\b' && this.IsCaretJustBeforePrompt())
			{
				e.Handled = true;
				return;
			}
			if (this.IsTerminatorKey(e.KeyChar))
			{
				e.Handled = true;
				string textAtPrompt = this.GetTextAtPrompt();
				if (textAtPrompt.Length != 0)
				{
					this.printLine();
					((ShellControl)base.Parent).FireCommandEntered(textAtPrompt);
					this.commandHistory.Add(textAtPrompt);
				}
				this.printPrompt();
			}
		}
		private void ShellControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (!this.IsCaretAtWritablePosition() && !e.Control && !this.IsTerminatorKey(e.KeyCode))
			{
				this.MoveCaretToEndOfText();
			}
			if (e.KeyCode == Keys.Left && this.IsCaretJustBeforePrompt())
			{
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.Down)
			{
				if (this.commandHistory.DoesNextCommandExist())
				{
					this.ReplaceTextAtPrompt(this.commandHistory.GetNextCommand());
				}
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.Up)
			{
				if (this.commandHistory.DoesPreviousCommandExist())
				{
					this.ReplaceTextAtPrompt(this.commandHistory.GetPreviousCommand());
				}
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.Right)
			{
				string textAtPrompt = this.GetTextAtPrompt();
				string lastCommand = this.commandHistory.LastCommand;
				if (lastCommand != null && (textAtPrompt.Length == 0 || lastCommand.StartsWith(textAtPrompt)) && lastCommand.Length > textAtPrompt.Length)
				{
					this.AddText(lastCommand[textAtPrompt.Length].ToString());
				}
			}
		}
		private string GetCurrentLine()
		{
			if (base.Lines.Length > 0)
			{
				return (string)base.Lines.GetValue(base.Lines.GetLength(0) - 1);
			}
			return "";
		}
		private string GetTextAtPrompt()
		{
			string currentLine = this.GetCurrentLine();
			if (currentLine.Length < this.prompt.Length)
			{
				return string.Empty;
			}
			return currentLine.Substring(this.prompt.Length);
		}
		private void ReplaceTextAtPrompt(string text)
		{
			string currentLine = this.GetCurrentLine();
			int num = currentLine.Length - this.prompt.Length;
			if (num == 0)
			{
				this.AddText(text);
				return;
			}
			base.Select(this.TextLength - num, num);
			this.SelectedText = text;
		}
		private bool IsCaretAtCurrentLine()
		{
			return this.TextLength - base.SelectionStart <= this.GetCurrentLine().Length;
		}
		private void MoveCaretToEndOfText()
		{
			base.SelectionStart = this.TextLength;
			base.ScrollToCaret();
		}
		private bool IsCaretJustBeforePrompt()
		{
			return this.IsCaretAtCurrentLine() && this.GetCurrentCaretColumnPosition() == this.prompt.Length;
		}
		private int GetCurrentCaretColumnPosition()
		{
			string currentLine = this.GetCurrentLine();
			int selectionStart = base.SelectionStart;
			return selectionStart - this.TextLength + currentLine.Length;
		}
		private bool IsCaretAtWritablePosition()
		{
			return this.IsCaretAtCurrentLine() && this.GetCurrentCaretColumnPosition() >= this.prompt.Length;
		}
		private void SetPromptText(string val)
		{
			this.GetCurrentLine();
			base.Select(0, this.prompt.Length);
			this.SelectedText = val;
			this.prompt = val;
		}
		public string[] GetCommandHistory()
		{
			return this.commandHistory.GetCommandHistory();
		}
		public void WriteText(string text)
		{
			this.AddText(text);
		}
		private bool IsTerminatorKey(Keys key)
		{
			return key == Keys.Return;
		}
		private bool IsTerminatorKey(char keyChar)
		{
			return keyChar == '\r';
		}
		private void AddText(string text)
		{
			this.Text += text;
			this.MoveCaretToEndOfText();
		}
	}
}
