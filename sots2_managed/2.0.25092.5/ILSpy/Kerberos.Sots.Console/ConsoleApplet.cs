using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
namespace Kerberos.Sots.Console
{
	internal class ConsoleApplet
	{
		private bool _started;
		private Thread _thread;
		private readonly List<string> _commands = new List<string>();
		private Console _con;
		private void Worker(object param)
		{
			this._con = new Console();
			this._con.Show();
			this._con.CommandEntered += new EventCommandEntered(this.ShellControl_CommandEntered);
			Application.Run(this._con);
			this._con.CommandEntered -= new EventCommandEntered(this.ShellControl_CommandEntered);
			this._con.Dispose();
			this._con = null;
		}
		private void ShellControl_CommandEntered(object sender, CommandEnteredEventArgs e)
		{
			lock (this._commands)
			{
				this._commands.Add(e.Command);
			}
		}
		public void Start()
		{
			if (this._started)
			{
				throw new InvalidOperationException("Already started.");
			}
			this._started = true;
			this._thread = ScriptHost.CreateThread(new ParameterizedThreadStart(this.Worker));
			this._thread.Priority = ThreadPriority.BelowNormal;
			this._thread.SetApartmentState(ApartmentState.STA);
			this._thread.Start();
		}
		public void Stop()
		{
			Application.Exit();
			this._commands.Clear();
			this._started = false;
		}
		public string[] FlushCommands()
		{
			string[] result;
			lock (this._commands)
			{
				string[] array = this._commands.ToArray();
				this._commands.Clear();
				result = array;
			}
			return result;
		}
		public void WriteText(string category, bool registerHit, string s, Color color)
		{
			if (this._con == null || s.Length == 0)
			{
				return;
			}
			lock (this._con)
			{
				this._con.WriteText(category, registerHit, s, color);
			}
		}
	}
}
