using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPSYSTEM)]
	internal class StarMapSystem : StarMapObject
	{
		private bool _enabled;
		public StarMapSystem(App game, string modelName, Vector3 position, float scale, string label)
		{
			game.AddExistingObject(this, new object[]
			{
				modelName
			});
			this.PostSetProp("Label", label);
			this.PostSetScale(scale);
			this.PostSetPosition(position);
			this._enabled = true;
		}
		public void SetTradeValues(GameSession _game, TradeNode node, TradeNode historynode, int systemId)
		{
			this.PostSetProp("TradeNode", new object[]
			{
				node.Produced,
				node.ProductionCapacity,
				node.Consumption,
				node.Freighters,
				node.DockCapacity,
				node.ExportInt,
				node.ExportProv,
				node.ExportLoc,
				node.ImportInt,
				node.ImportProv,
				node.ImportLoc,
				node.Range,
				historynode.Produced,
				historynode.Freighters,
				historynode.ImportInt
			});
			List<FreighterInfo> list;
			if (_game.GameDatabase != null)
			{
				list = _game.GameDatabase.GetFreighterInfosForSystem(systemId).ToList<FreighterInfo>();
			}
			else
			{
				list = new List<FreighterInfo>();
			}
			Dictionary<FreighterInfo, int> dictionary = new Dictionary<FreighterInfo, int>();
			foreach (FreighterInfo current in list)
			{
				dictionary.Add(current, current.Design.DesignSections.Sum((DesignSectionInfo x) => x.ShipSectionAsset.FreighterSpace));
			}
			dictionary = (
				from x in dictionary
				orderby x.Value descending
				select x).ToDictionary((KeyValuePair<FreighterInfo, int> y) => y.Key, (KeyValuePair<FreighterInfo, int> y) => y.Value);
			List<object> list2 = new List<object>();
			list2.Add((
				from x in dictionary
				where x.Key.PlayerId != _game.LocalPlayer.ID || !x.Key.IsPlayerBuilt
				select x).Count<KeyValuePair<FreighterInfo, int>>());
			list2.AddRange((
				from x in dictionary
				where x.Key.PlayerId != _game.LocalPlayer.ID || !x.Key.IsPlayerBuilt
				select x.Value).Cast<object>());
			this.PostSetProp("FreighterCapacities", list2.ToArray());
			List<object> list3 = new List<object>();
			list3.Add((
				from x in dictionary
				where x.Key.PlayerId == _game.LocalPlayer.ID && x.Key.IsPlayerBuilt
				select x).Count<KeyValuePair<FreighterInfo, int>>());
			list3.AddRange((
				from x in dictionary
				where x.Key.PlayerId == _game.LocalPlayer.ID && x.Key.IsPlayerBuilt
				select x.Value).Cast<object>());
			this.PostSetProp("PlayerFreighterCapacities", list3.ToArray());
		}
		public void SetProductionValues(int prod, int maxProd)
		{
			this.PostSetProp("ProdValues", new object[]
			{
				prod,
				maxProd
			});
		}
		public void SetProvince(StarMapProvince value)
		{
			this.PostSetProp("Province", value);
		}
		public void SetPlayers(Player[] players)
		{
			this.PostSetProp("Players", players);
		}
		public void SetPlayersWithGates(Player[] players)
		{
			this.PostSetProp("PlayersWithGates", players);
		}
		public void SetPlayersWithAccelerators(Player[] players)
		{
			this.PostSetProp("PlayersWithAccelerators", players);
		}
		public void SetNavalCapacity(int capacity)
		{
			this.PostSetProp("NavalCapacity", capacity);
		}
		public void SetNavalUsage(int usage)
		{
			this.PostSetProp("NavalUsage", usage);
		}
		public void SetHasNavalStation(bool value)
		{
			this.PostSetProp("HasNavalStation", value);
		}
		public void SetHasScienceStation(bool value)
		{
			this.PostSetProp("HasScienceStation", value);
		}
		public void SetHasTradeStation(bool value)
		{
			this.PostSetProp("HasTradeStation", value);
		}
		public void SetHasDiploStation(bool value)
		{
			this.PostSetProp("HasDiploStation", value);
		}
		public void SetHasLoaGate(bool value)
		{
			this.PostSetProp("HasLoaGate", value);
		}
		public void SetStationCapacity(int value)
		{
			this.PostSetProp("StationsSupported", value);
		}
		public void SetColonyTrapped(bool value)
		{
			this.PostSetProp("ColonyTrapped", value);
		}
		public void SetSupportRange(float value)
		{
			this.PostSetProp("SupportRange", value);
		}
		public void SetPlayerBadge(string value)
		{
			this.PostSetProp("OwnerBadge", value);
		}
		public void SetOwningPlayer(Player player)
		{
			this.PostSetProp("SystemOwner", player);
		}
		public void SetIsEnabled(bool value)
		{
			this.PostSetProp("Enabled", value);
			this._enabled = value;
		}
		public bool GetIsEnabled()
		{
			return this._enabled;
		}
		public void SetIsSurveyed(bool value)
		{
			this.PostSetProp("Surveyed", value);
		}
		public void SetTerrain(StarMapTerrain value)
		{
			this.PostSetProp("Terrain", value);
		}
	}
}
