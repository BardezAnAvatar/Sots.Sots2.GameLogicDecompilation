using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.Combat
{
	internal abstract class CombatAIController
	{
		public abstract void Initialize();
		public abstract void Terminate();
		public abstract void ObjectRemoved(IGameObject obj);
		public abstract Ship GetShip();
		public abstract bool RequestingNewTarget();
		public abstract void FindNewTarget(IEnumerable<IGameObject> objs);
		public abstract bool NeedsAParent();
		public abstract void FindParent(IEnumerable<CombatAIController> controllers);
		public abstract void SetTarget(IGameObject target);
		public abstract IGameObject GetTarget();
		public abstract void OnThink();
		public abstract void ForceFlee();
		public abstract bool VictoryConditionIsMet();
		public static Ship CreateNewShip(GameSession strategySim, Matrix worldMat, int designId, int parentId, int inputId, int playerId)
		{
			ShipInfo shipInfo = new ShipInfo();
			shipInfo.FleetID = 0;
			shipInfo.DesignID = designId;
			shipInfo.DesignInfo = strategySim.GameDatabase.GetDesignInfo(designId);
			shipInfo.ParentID = 0;
			shipInfo.SerialNumber = 0;
			shipInfo.ShipName = string.Empty;
			if (shipInfo.DesignID != 0)
			{
				return Ship.CreateShip(strategySim, worldMat, shipInfo, parentId, inputId, playerId, false, null);
			}
			return null;
		}
	}
}
