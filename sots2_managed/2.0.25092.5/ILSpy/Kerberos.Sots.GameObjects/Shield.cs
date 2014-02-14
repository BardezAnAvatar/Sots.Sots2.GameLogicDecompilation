using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIELD)]
	internal class Shield : GameObject, IActive
	{
		private bool _isShipAssigned;
		private bool _active;
		public bool IsShipAssigned
		{
			get
			{
				return this._isShipAssigned;
			}
		}
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				this._active = value;
				this.PostSetActive(this._active);
			}
		}
		public Shield(App game, Ship ship, LogicalShield logShield, Section sectWithShield, List<PlayerTechInfo> playerTechs, bool isShipAssigned = true)
		{
			ShieldData shieldData = (ship.ShipClass == ShipClass.Dreadnought) ? logShield.DNShieldData : logShield.CRShieldData;
			game.AddExistingObject(this, new List<object>
			{
				(int)logShield.Type,
				Shield.GetTotalStructure(game.AssetDatabase, shieldData.Structure, playerTechs),
				shieldData.RechargeTime,
				(logShield.Type == LogicalShield.ShieldType.PSI_SHIELD) ? 0f : Shield.GetShieldRegenPerSec(playerTechs),
				ship.ObjectID,
				sectWithShield.ObjectID,
				Path.Combine("props\\models\\Shields", Path.GetFileNameWithoutExtension(shieldData.ModelFileName) + "_convex.obj"),
				Path.Combine("props\\models\\Shields", shieldData.ModelFileName),
				Path.Combine(new string[]
				{
					shieldData.ImpactEffectName
				}),
				1000000f,
				true,
				isShipAssigned
			}.ToArray());
			this._isShipAssigned = isShipAssigned;
		}
		public static float GetTotalStructure(AssetDatabase ab, float baseStruct, List<PlayerTechInfo> playerTechs)
		{
			PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_Quantum_Capacitors");
			PlayerTechInfo playerTechInfo2 = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_Shield_Magnifier");
			float num = 1f;
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
			{
				num += ab.GetTechBonus<float>(playerTechInfo.TechFileID, "shieldstructure");
			}
			if (playerTechInfo2 != null && playerTechInfo2.State == TechStates.Researched)
			{
				num += ab.GetTechBonus<float>(playerTechInfo2.TechFileID, "shieldstructure");
			}
			return baseStruct * num;
		}
		public static float GetShieldRegenPerSec(List<PlayerTechInfo> playerTechs)
		{
			PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "SLD_Shields_Rechargers");
			if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
			{
				return 50f;
			}
			return 0f;
		}
	}
}
