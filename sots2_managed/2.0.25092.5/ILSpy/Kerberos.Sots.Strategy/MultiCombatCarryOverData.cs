using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	public class MultiCombatCarryOverData
	{
		private List<MCSystemInfo> Systems;
		public MultiCombatCarryOverData()
		{
			this.Systems = new List<MCSystemInfo>();
		}
		public void ClearData()
		{
			this.Systems.Clear();
		}
		public void AddCarryOverInfo(int systemId, int fleetId, int shipId, Matrix endShipTransform)
		{
			MCSystemInfo mCSystemInfo = this.Systems.FirstOrDefault((MCSystemInfo x) => x.SystemID == systemId);
			if (mCSystemInfo == null)
			{
				MCShipInfo item = new MCShipInfo
				{
					ShipID = shipId,
					PreviousTransform = endShipTransform
				};
				MCFleetInfo mCFleetInfo = new MCFleetInfo
				{
					FleetID = fleetId
				};
				mCFleetInfo.Ships.Add(item);
				mCSystemInfo = new MCSystemInfo
				{
					SystemID = systemId
				};
				mCSystemInfo.Fleets.Add(mCFleetInfo);
				this.Systems.Add(mCSystemInfo);
				return;
			}
			MCFleetInfo mCFleetInfo2 = mCSystemInfo.Fleets.FirstOrDefault((MCFleetInfo x) => x.FleetID == fleetId);
			if (mCFleetInfo2 == null)
			{
				MCShipInfo item2 = new MCShipInfo
				{
					ShipID = shipId,
					PreviousTransform = endShipTransform
				};
				mCFleetInfo2 = new MCFleetInfo
				{
					FleetID = fleetId
				};
				mCFleetInfo2.Ships.Add(item2);
				mCSystemInfo.Fleets.Add(mCFleetInfo2);
				return;
			}
			MCShipInfo mCShipInfo = mCFleetInfo2.Ships.FirstOrDefault((MCShipInfo x) => x.ShipID == shipId);
			if (mCShipInfo != null)
			{
				mCShipInfo.PreviousTransform = endShipTransform;
				return;
			}
			mCFleetInfo2.Ships.Add(new MCShipInfo
			{
				ShipID = shipId,
				PreviousTransform = endShipTransform
			});
		}
		public void SetRetreatFleetID(int systemId, int currFleetId, int retreatFleetId)
		{
			MCSystemInfo mCSystemInfo = this.Systems.FirstOrDefault((MCSystemInfo x) => x.SystemID == systemId);
			if (mCSystemInfo == null)
			{
				MCFleetInfo item = new MCFleetInfo
				{
					FleetID = currFleetId,
					RetreatFleetID = retreatFleetId
				};
				mCSystemInfo = new MCSystemInfo
				{
					SystemID = systemId
				};
				mCSystemInfo.Fleets.Add(item);
				this.Systems.Add(mCSystemInfo);
				return;
			}
			MCFleetInfo mCFleetInfo = mCSystemInfo.Fleets.FirstOrDefault((MCFleetInfo x) => x.FleetID == currFleetId);
			if (mCFleetInfo != null)
			{
				mCFleetInfo.RetreatFleetID = retreatFleetId;
				return;
			}
			mCFleetInfo = new MCFleetInfo
			{
				FleetID = currFleetId,
				RetreatFleetID = retreatFleetId
			};
			mCSystemInfo.Fleets.Add(mCFleetInfo);
		}
		public int GetRetreatFleetID(int systemId, int currFleetId)
		{
			MCSystemInfo mCSystemInfo = this.Systems.FirstOrDefault((MCSystemInfo x) => x.SystemID == systemId);
			if (mCSystemInfo != null)
			{
				MCFleetInfo mCFleetInfo = mCSystemInfo.Fleets.FirstOrDefault((MCFleetInfo x) => x.FleetID == currFleetId);
				if (mCFleetInfo != null)
				{
					return mCFleetInfo.RetreatFleetID;
				}
				mCFleetInfo = mCSystemInfo.Fleets.FirstOrDefault((MCFleetInfo x) => x.RetreatFleetID == currFleetId);
				if (mCFleetInfo != null)
				{
					return mCFleetInfo.RetreatFleetID;
				}
			}
			return 0;
		}
		public void AddCarryOverCombatZoneInfo(int systemId, List<int> combatZones)
		{
			MCSystemInfo mCSystemInfo = this.Systems.FirstOrDefault((MCSystemInfo x) => x.SystemID == systemId);
			if (mCSystemInfo == null)
			{
				mCSystemInfo = new MCSystemInfo
				{
					SystemID = systemId
				};
				this.Systems.Add(mCSystemInfo);
			}
			if (mCSystemInfo != null)
			{
				mCSystemInfo.ControlZones.Clear();
				foreach (int current in combatZones)
				{
					mCSystemInfo.ControlZones.Add(current);
				}
			}
		}
		public Matrix? GetPreviousShipTransform(int systemId, int fleetId, int shipId)
		{
			MCSystemInfo mCSystemInfo = this.Systems.FirstOrDefault((MCSystemInfo x) => x.SystemID == systemId);
			if (mCSystemInfo != null)
			{
				MCFleetInfo mCFleetInfo = mCSystemInfo.Fleets.FirstOrDefault((MCFleetInfo x) => x.FleetID == fleetId);
				if (mCFleetInfo != null)
				{
					MCShipInfo mCShipInfo = mCFleetInfo.Ships.FirstOrDefault((MCShipInfo x) => x.ShipID == shipId);
					if (mCShipInfo != null)
					{
						return new Matrix?(mCShipInfo.PreviousTransform);
					}
				}
			}
			return null;
		}
		public List<int> GetPreviousControlZones(int systemId)
		{
			MCSystemInfo mCSystemInfo = this.Systems.FirstOrDefault((MCSystemInfo x) => x.SystemID == systemId);
			if (mCSystemInfo != null)
			{
				return mCSystemInfo.ControlZones;
			}
			return new List<int>();
		}
		public List<object> GetCarryOverDataList(int systemID)
		{
			List<object> list = new List<object>();
			MCSystemInfo mCSystemInfo = this.Systems.FirstOrDefault((MCSystemInfo x) => x.SystemID == systemID);
			if (mCSystemInfo == null)
			{
				return list;
			}
			list.Add(systemID);
			list.Add(mCSystemInfo.ControlZones.Count);
			foreach (int current in mCSystemInfo.ControlZones)
			{
				list.Add(current);
			}
			list.Add(mCSystemInfo.Fleets.Count);
			foreach (MCFleetInfo current2 in mCSystemInfo.Fleets)
			{
				list.Add(current2.FleetID);
				list.Add(current2.RetreatFleetID);
				list.Add(current2.Ships.Count);
				foreach (MCShipInfo current3 in current2.Ships)
				{
					list.Add(current3.ShipID);
					list.Add(current3.PreviousTransform.Position.X);
					list.Add(current3.PreviousTransform.Position.Y);
					list.Add(current3.PreviousTransform.Position.Z);
					list.Add(current3.PreviousTransform.EulerAngles.X);
					list.Add(current3.PreviousTransform.EulerAngles.Y);
					list.Add(current3.PreviousTransform.EulerAngles.Z);
				}
			}
			return list;
		}
	}
}
