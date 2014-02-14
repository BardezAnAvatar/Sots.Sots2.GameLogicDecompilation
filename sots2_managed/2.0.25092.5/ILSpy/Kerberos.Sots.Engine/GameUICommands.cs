using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Kerberos.Sots.Engine
{
	internal class GameUICommands
	{
		public readonly UICommand EndTurn;
		public readonly UICommand Exit;
		private readonly List<UICommand> _commands = new List<UICommand>();
        public GameUICommands(App game)
        {
            Action trigger = null;
            Action<IUIPollCommandState> poll = null;
            Action action3 = null;
            Func<FieldInfo, UICommand> selector = null;
            this._commands = new List<UICommand>();
            if (trigger == null)
            {
                trigger = () => game.EndTurn();
            }
            if (poll == null)
            {
                poll = cmd => cmd.IsEnabled = game.CanEndTurn();
            }
            this.EndTurn = new UICommand("EndTurn", trigger, poll);
            if (action3 == null)
            {
                action3 = () => game.RequestExit();
            }
            this.Exit = new UICommand("Exit", action3, cmd => cmd.IsEnabled = true);
            if (selector == null)
            {
                selector = x => x.GetValue(this) as UICommand;
            }
            this._commands.AddRange(from x in base.GetType().GetFields().Select<FieldInfo, UICommand>(selector)
                                    where x != null
                                    select x);
        }
        public void Poll()
		{
			this._commands.Poll();
		}
	}
}
