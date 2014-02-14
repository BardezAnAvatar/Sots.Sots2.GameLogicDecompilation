using Kerberos.Sots.Combat;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.GameObjects
{
	internal class ShipTargetComparision : IComparer<Ship>
	{
		public CombatAI _ai;
		public Vector3 _formationPosition;
		public ShipTargetComparision(CombatAI ai, Vector3 formPos)
		{
			this._ai = ai;
			this._formationPosition = formPos;
		}
		public int Compare(Ship alpha, Ship beta)
		{
			int targetShipScore = this._ai.GetTargetShipScore(alpha);
			int targetShipScore2 = this._ai.GetTargetShipScore(beta);
			if (targetShipScore != targetShipScore2)
			{
				if (targetShipScore2 < targetShipScore)
				{
					return -1;
				}
				return 1;
			}
			else
			{
				float length = (alpha.Maneuvering.Position - this._formationPosition).Length;
				float length2 = (beta.Maneuvering.Position - this._formationPosition).Length;
				if (length < length2)
				{
					return -1;
				}
				return 1;
			}
		}
	}
}
