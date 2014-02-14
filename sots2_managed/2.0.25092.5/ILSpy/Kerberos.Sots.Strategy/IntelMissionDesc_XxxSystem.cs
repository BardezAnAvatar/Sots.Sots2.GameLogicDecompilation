using System;
namespace Kerberos.Sots.Strategy
{
	internal abstract class IntelMissionDesc_XxxSystem : IntelMissionDesc
	{
		public sealed override void OnProcessTurnEvent(GameSession game, TurnEvent e)
		{
			base.OnProcessTurnEvent(game, e);
			if (e.SystemID != 0)
			{
				game.GameDatabase.UpdatePlayerViewWithStarSystem(game.LocalPlayer.ID, e.SystemID);
				int turnCount = game.GameDatabase.GetTurnCount();
				game.GameDatabase.InsertExploreRecord(e.SystemID, game.LocalPlayer.ID, turnCount, true, true);
			}
		}
	}
}
