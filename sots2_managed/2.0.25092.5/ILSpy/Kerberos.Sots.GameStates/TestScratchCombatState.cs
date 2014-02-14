using System;
using System.IO;
using System.Xml;
namespace Kerberos.Sots.GameStates
{
	internal class TestScratchCombatState : CombatState
	{
		public TestScratchCombatState(App game) : base(game)
		{
		}
		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (base.App.GameDatabase == null)
			{
				base.App.NewGame();
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(base.App.GameRoot, "data/scratch_combat.xml"));
			base.OnPrepare(prev, new object[]
			{
				0,
				xmlDocument,
				true
			});
		}
	}
}
