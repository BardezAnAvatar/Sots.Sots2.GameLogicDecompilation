using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Strategy
{
	internal class ShipSectionCollection
	{
		private struct Key
		{
			public ShipSectionType Type;
			public ShipClass Class;
		}
		private readonly Dictionary<ShipSectionCollection.Key, HashSet<ShipSectionAsset>> _bykey;
		private readonly HashSet<ShipSectionAsset> _all;
		private static readonly HashSet<ShipSectionAsset> Empty = new HashSet<ShipSectionAsset>();
        public ShipSectionCollection(GameDatabase gameDatabase, AssetDatabase assetDatabase, Player player, string[] availableSectionIds)
        {
            this._bykey = new Dictionary<Key, HashSet<ShipSectionAsset>>();
            this._all = new HashSet<ShipSectionAsset>();
            int playerFactionID = gameDatabase.GetPlayerFactionID(player.ID);
            string playerFaction = gameDatabase.GetFactionName(playerFactionID);
            this._bykey.Clear();
            this._all.Clear();
            foreach (ShipSectionAsset asset in from x in assetDatabase.ShipSections
                                               where (availableSectionIds.Contains<string>(x.FileName) && (x.Faction == playerFaction)) && !x.IsSuulka
                                               select x)
            {
                this._all.Add(asset);
            }
            foreach (ShipSectionAsset asset2 in this._all)
            {
                HashSet<ShipSectionAsset> set;
                Key key = new Key
                {
                    Class = asset2.Class,
                    Type = asset2.Type
                };
                if (!this._bykey.TryGetValue(key, out set))
                {
                    set = new HashSet<ShipSectionAsset>();
                    this._bykey.Add(key, set);
                }
                set.Add(asset2);
            }
        }
        public HashSet<ShipSectionAsset> GetSectionsByType(ShipClass shipClass, ShipSectionType type)
		{
			ShipSectionCollection.Key key = new ShipSectionCollection.Key
			{
				Type = type,
				Class = shipClass
			};
			HashSet<ShipSectionAsset> result;
			if (!this._bykey.TryGetValue(key, out result))
			{
				return ShipSectionCollection.Empty;
			}
			return result;
		}
		public HashSet<ShipSectionAsset> GetAllSections()
		{
			return this._all;
		}
	}
}
