using System;
namespace Kerberos.Sots.Data
{
	internal class FleetInfo : IIDProvider
	{
		public int PlayerID;
		public int AdmiralID;
		public int SystemID;
		public int? PreviousSystemID;
		public int SupportingSystemID;
		public string Name;
		public int TurnsAway;
		public float SupplyRemaining;
		public bool Preferred;
		public int LastTurnAccelerated;
		public int? FleetConfigID;
		public FleetType Type;
		public int ID
		{
			get;
			set;
		}
		public bool IsNormalFleet
		{
			get
			{
				return this.Type == FleetType.FL_NORMAL;
			}
		}
		public bool IsReserveFleet
		{
			get
			{
				return this.Type == FleetType.FL_RESERVE;
			}
		}
		public bool IsLimboFleet
		{
			get
			{
				return this.Type == FleetType.FL_LIMBOFLEET;
			}
		}
		public bool IsDefenseFleet
		{
			get
			{
				return this.Type == FleetType.FL_DEFENSE;
			}
		}
		public bool IsGateFleet
		{
			get
			{
				return this.Type == FleetType.FL_GATE;
			}
		}
		public bool IsAcceleratorFleet
		{
			get
			{
				return this.Type == FleetType.FL_ACCELERATOR;
			}
		}
		public bool IsTrapFleet
		{
			get
			{
				return this.Type == FleetType.FL_TRAP;
			}
		}
		public override string ToString()
		{
			return string.Format("ID={0},Name={1}", this.ID, this.Name ?? string.Empty);
		}
		public void CopyFrom(FleetInfo value)
		{
			this.PlayerID = value.PlayerID;
			this.AdmiralID = value.AdmiralID;
			this.SystemID = value.SystemID;
			this.PreviousSystemID = value.PreviousSystemID;
			this.SupportingSystemID = value.SupportingSystemID;
			this.Name = value.Name;
			this.TurnsAway = value.TurnsAway;
			this.SupplyRemaining = value.SupplyRemaining;
			this.Preferred = value.Preferred;
			this.Type = value.Type;
			this.LastTurnAccelerated = value.LastTurnAccelerated;
			this.FleetConfigID = value.FleetConfigID;
		}
	}
}
