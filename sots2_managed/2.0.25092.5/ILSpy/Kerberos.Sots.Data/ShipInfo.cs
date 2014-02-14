using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Data
{
	internal class ShipInfo : IIDProvider
	{
		public int FleetID;
		public int DesignID;
		public int ParentID;
		public string ShipName;
		public int SerialNumber;
		public int ComissionDate;
		public ShipParams Params;
		public int RiderIndex;
		public int PsionicPower;
		public double SlavesObtained;
		public int LoaCubes;
		public Vector3? ShipFleetPosition;
		public Matrix? ShipSystemPosition;
		public int? AIFleetID;
		public DesignInfo DesignInfo;
		public int ID
		{
			get;
			set;
		}
		public static int GetMaxPsionicPower(App app, DesignInfo di, List<AdmiralInfo.TraitType> admiralTraits)
		{
			if (di.DesignSections.Any((DesignSectionInfo x) => x.ShipSectionAsset.IsSuulka))
			{
				return (int)(
					from x in di.DesignSections
					select x.ShipSectionAsset.PsionicPowerLevel).Sum();
			}
			float num = 1f;
			if (admiralTraits.Contains(AdmiralInfo.TraitType.Psion))
			{
				num += 0.2f;
			}
			else
			{
				if (admiralTraits.Contains(AdmiralInfo.TraitType.Skeptic))
				{
					num -= 0.2f;
				}
			}
			int num2 = (
				from x in di.DesignSections
				select x.ShipSectionAsset.Crew).Sum();
			return (int)Math.Floor((double)((float)num2 * app.AssetDatabase.GetFaction(app.GameDatabase.GetPlayerFactionID(di.PlayerID)).PsionicPowerPerCrew * num));
		}
		public bool IsPoliceShip()
		{
			return this.DesignInfo.IsPoliceShip();
		}
		public bool IsSDB()
		{
			return this.DesignInfo.IsSDB();
		}
		public bool IsPlatform()
		{
			return this.DesignInfo.IsPlatform();
		}
		public bool IsMinelayer()
		{
			return this.DesignInfo.IsMinelayer();
		}
		public bool IsPlaced()
		{
			return this.ShipSystemPosition.HasValue;
		}
		public override string ToString()
		{
			return string.Format("ID={0},ShipName={1},SerialNumber={2}", this.ID, this.ShipName, this.SerialNumber);
		}
	}
}
