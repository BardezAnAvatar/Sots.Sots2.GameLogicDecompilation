using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class StarMapStateMode
	{
		protected App App
		{
			get;
			private set;
		}
		protected GameSession Sim
		{
			get;
			private set;
		}
		protected StarMapStateMode(GameSession sim)
		{
			this.Sim = sim;
			this.App = sim.App;
		}
		public virtual void Initialize()
		{
		}
		public virtual void Terminate()
		{
		}
		public virtual bool OnGameObjectClicked(IGameObject obj)
		{
			return false;
		}
		public virtual bool OnGameObjectMouseOver(IGameObject obj)
		{
			return false;
		}
		public virtual bool OnUIButtonPressed(string panelName)
		{
			return false;
		}
		public virtual bool OnUIDialogClosed(string panelName, string[] msgParams)
		{
			return false;
		}
	}
}
