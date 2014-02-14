using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
namespace Kerberos.Sots.PlayerFramework
{
	internal class AvailablePlayerFeatures
	{
		private readonly Random _random;
		private readonly Dictionary<Faction, AvailableFactionFeatures> _factions;
		public ReadOnlyDictionary<Faction, AvailableFactionFeatures> Factions
		{
			get;
			private set;
		}
		public GrabBag<int> EmpireColors
		{
			get;
			private set;
		}
		public AvailablePlayerFeatures(AssetDatabase assetdb, Random random, IEnumerable<Faction> factions)
		{
			this._random = random;
			this._factions = new Dictionary<Faction, AvailableFactionFeatures>();
			this.ReplaceFactions(factions);
			this.Factions = new ReadOnlyDictionary<Faction, AvailableFactionFeatures>(this._factions);
			this.EmpireColors = new GrabBag<int>(random, Generators.Sequence(0, Player.DefaultPrimaryPlayerColors.Count, 1));
		}
		public void ReplaceFactions(IEnumerable<Faction> factions)
		{
			this._factions.Clear();
			foreach (Faction current in factions)
			{
				this.TryAddFaction(current);
			}
		}
		public bool TryAddFaction(Faction faction)
		{
			if (faction == null)
			{
				return false;
			}
			if (!faction.IsPlayable)
			{
				return false;
			}
			if (this._factions.ContainsKey(faction))
			{
				return false;
			}
			this._factions[faction] = new AvailableFactionFeatures(this._random, faction);
			return true;
		}
		public bool TryRemoveFaction(Faction faction)
		{
			if (faction == null)
			{
				return false;
			}
			if (!this._factions.ContainsKey(faction))
			{
				return false;
			}
			this._factions.Remove(faction);
			return true;
		}
	}
}
