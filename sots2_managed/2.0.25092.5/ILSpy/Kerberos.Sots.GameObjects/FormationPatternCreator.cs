using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameObjects
{
	internal static class FormationPatternCreator
	{
		public static Ship FindBestFitShipBySize(List<Ship> remainingShips, Vector3 formationPosition, Matrix formationMatrix, ShipClass requestedClass)
		{
			if (remainingShips.Count == 0)
			{
				return null;
			}
			Vector3 v = Vector3.Transform(formationPosition, formationMatrix);
			ShipClass shipClass = requestedClass;
			remainingShips.Sort((Ship x, Ship y) => Ship.IsShipClassBigger(y.ShipClass, x.ShipClass, false).CompareTo(Ship.IsShipClassBigger(x.ShipClass, y.ShipClass, false)));
			Ship ship = null;
			int num = 30;
			while (ship == null && shipClass != ShipClass.BattleRider)
			{
				ShipClass shipClass2 = ShipClass.BattleRider;
				float num2 = 3.40282347E+38f;
				foreach (Ship current in remainingShips)
				{
					if (Ship.IsShipClassBigger(current.ShipClass, shipClass2, false))
					{
						shipClass2 = current.ShipClass;
					}
					if (current.ShipClass != shipClass)
					{
						break;
					}
					float lengthSquared = (current.Maneuvering.Position - v).LengthSquared;
					if (lengthSquared < num2)
					{
						num2 = lengthSquared;
						ship = current;
					}
				}
				if (ship != null)
				{
					break;
				}
				shipClass = shipClass2;
				num--;
				if (num <= 0)
				{
					ship = remainingShips.FirstOrDefault<Ship>();
					break;
				}
			}
			return ship;
		}
		public static Vector3 GetLineAbreastPositionAtIndex(int index)
		{
			int num = index / 5;
			int num2 = (index % 5 + 1) / 2;
			int num3 = (index % 2 == 0) ? 1 : -1;
			return new Vector3
			{
				X = (float)num3 * 400f * (float)num2,
				Y = 0f,
				Z = 500f * (float)num
			};
		}
		public static Vector3 GetVFormationPositionAtIndex(int index)
		{
			int num = (index + 1) / 2;
			int num2 = (index + 1) / 2;
			int num3 = (index % 2 == 0) ? 1 : -1;
			return new Vector3
			{
				X = (float)num3 * 400f * (float)num2,
				Y = 0f,
				Z = 500f * (float)num
			};
		}
		public static Vector3 GetCubeFormationPositionAtIndex(int index)
		{
			switch (index)
			{
			case 0:
				return new Vector3(0f, 0f, 0f);
			case 1:
				return new Vector3(-750f, 0f, 0f);
			case 2:
				return new Vector3(750f, 0f, 0f);
			case 3:
				return new Vector3(0f, 0f, -750f);
			case 4:
				return new Vector3(0f, 0f, 750f);
			case 5:
				return new Vector3(-750f, 0f, -750f);
			case 6:
				return new Vector3(750f, 0f, -750f);
			case 7:
				return new Vector3(-750f, 0f, 750f);
			case 8:
				return new Vector3(750f, 0f, 750f);
			case 9:
				return new Vector3(0f, 300f, 0f);
			case 10:
				return new Vector3(0f, -300f, 0f);
			case 11:
				return new Vector3(0f, 300f, -750f);
			case 12:
				return new Vector3(0f, -300f, -750f);
			case 13:
				return new Vector3(0f, 300f, 750f);
			case 14:
				return new Vector3(0f, -300f, 750f);
			case 15:
				return new Vector3(-750f, 300f, 0f);
			case 16:
				return new Vector3(-750f, -300f, 0f);
			case 17:
				return new Vector3(750f, 300f, 0f);
			case 18:
				return new Vector3(750f, -300f, 0f);
			case 19:
				return new Vector3(-750f, 300f, -750f);
			case 20:
				return new Vector3(-750f, -300f, -750f);
			case 21:
				return new Vector3(750f, 300f, -750f);
			case 22:
				return new Vector3(750f, -300f, -750f);
			case 23:
				return new Vector3(-750f, 300f, 750f);
			case 24:
				return new Vector3(-750f, -300f, 750f);
			case 25:
				return new Vector3(750f, 300f, 750f);
			case 26:
				return new Vector3(750f, -300f, 750f);
			default:
				return Vector3.Zero;
			}
		}
		public static List<FormationPatternData> CreateLineAbreastPattern(List<Ship> ships, Matrix formationMat)
		{
			List<FormationPatternData> list = new List<FormationPatternData>();
			int count = ships.Count;
			Vector3 vector = Vector3.Zero;
			for (int i = 0; i < count; i++)
			{
				FormationPatternData formationPatternData = new FormationPatternData();
				formationPatternData.Position = FormationPatternCreator.GetLineAbreastPositionAtIndex(i);
				formationPatternData.Ship = null;
				formationPatternData.IsLead = (i == 0);
				vector += formationPatternData.Position;
				list.Add(formationPatternData);
			}
			if (list.Count > 0)
			{
				vector /= (float)list.Count;
			}
			List<Ship> list2 = new List<Ship>();
			list2.AddRange(ships);
			for (int j = 0; j < count; j++)
			{
				list[j].Ship = FormationPatternCreator.FindBestFitShipBySize(list2, list[j].Position - vector, formationMat, ShipClass.Leviathan);
				list2.Remove(list[j].Ship);
			}
			return list;
		}
		public static List<FormationPatternData> CreateVFormationPattern(List<Ship> ships, Matrix formationMat)
		{
			List<FormationPatternData> list = new List<FormationPatternData>();
			int count = ships.Count;
			for (int i = 0; i < count; i++)
			{
				list.Add(new FormationPatternData
				{
					Position = FormationPatternCreator.GetVFormationPositionAtIndex(i),
					Ship = null,
					IsLead = i == 0
				});
			}
			List<Ship> list2 = new List<Ship>();
			list2.AddRange(ships);
			for (int j = 0; j < count; j++)
			{
				list[j].Ship = FormationPatternCreator.FindBestFitShipBySize(list2, list[j].Position, formationMat, ShipClass.Leviathan);
				list2.Remove(list[j].Ship);
			}
			return list;
		}
		public static List<FormationPatternData> CreateCubeFormationPattern(List<Ship> ships, Matrix formationMat)
		{
			List<FormationPatternData> list = new List<FormationPatternData>();
			int count = ships.Count;
			for (int i = 0; i < count; i++)
			{
				list.Add(new FormationPatternData
				{
					Position = FormationPatternCreator.GetCubeFormationPositionAtIndex(i),
					Ship = null,
					IsLead = i == 0
				});
			}
			List<Ship> list2 = new List<Ship>();
			list2.AddRange(ships);
			for (int j = 0; j < count; j++)
			{
				list[j].Ship = FormationPatternCreator.FindBestFitShipBySize(list2, list[j].Position, formationMat, ShipClass.Leviathan);
				list2.Remove(list[j].Ship);
			}
			return list;
		}
	}
}
