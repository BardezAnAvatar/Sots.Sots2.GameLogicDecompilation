using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.StarSystemPathing
{
	internal class StarSystemPathing
	{
		private static Dictionary<int, Dictionary<int, List<LinkNodeChild>>> _playerSystemNodes = new Dictionary<int, Dictionary<int, List<LinkNodeChild>>>();
		public static void LoadAllNodes(GameSession game, GameDatabase db)
		{
			StarSystemPathing._playerSystemNodes.Clear();
			List<PlayerInfo> list = db.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<Player> list2 = new List<Player>();
			foreach (PlayerInfo current in list)
			{
				Player playerObject = game.GetPlayerObject(current.ID);
				if (playerObject.Faction.CanUseNodeLine(null) || playerObject.Faction.CanUseAccelerators())
				{
					StarSystemPathing._playerSystemNodes.Add(playerObject.ID, new Dictionary<int, List<LinkNodeChild>>());
					list2.Add(playerObject);
				}
			}
			List<StarSystemInfo> list3 = db.GetStarSystemInfos().ToList<StarSystemInfo>();
			foreach (StarSystemInfo current2 in list3)
			{
				foreach (Player player in list2)
				{
					List<NodeLineInfo> list4 = (
						from x in db.GetExploredNodeLinesFromSystem(player.ID, current2.ID)
						where x.IsPermenant == player.Faction.CanUseNodeLine(new bool?(true)) && x.IsLoaLine == player.Faction.CanUseAccelerators()
						select x).ToList<NodeLineInfo>();
					foreach (NodeLineInfo current3 in list4)
					{
						StarSystemPathing.AddSystemNode(db, player.ID, current3.System1ID, current3.System2ID, current3.ID);
					}
				}
			}
		}
		public static void AddSystemNode(GameDatabase db, int playerId, int fromSystemId, int toSystemId, int nodeId)
		{
			if (!StarSystemPathing._playerSystemNodes.ContainsKey(playerId))
			{
				return;
			}
			if (!StarSystemPathing._playerSystemNodes[playerId].ContainsKey(fromSystemId))
			{
				StarSystemPathing._playerSystemNodes[playerId].Add(fromSystemId, new List<LinkNodeChild>());
			}
			if (!StarSystemPathing._playerSystemNodes[playerId].ContainsKey(toSystemId))
			{
				StarSystemPathing._playerSystemNodes[playerId].Add(toSystemId, new List<LinkNodeChild>());
			}
			float length = (db.GetStarSystemOrigin(fromSystemId) - db.GetStarSystemOrigin(toSystemId)).Length;
			if (!StarSystemPathing._playerSystemNodes[playerId][fromSystemId].Any((LinkNodeChild x) => x.SystemId == toSystemId))
			{
				StarSystemPathing._playerSystemNodes[playerId][fromSystemId].Add(new LinkNodeChild
				{
					SystemId = toSystemId,
					NodeId = nodeId,
					Distance = length
				});
			}
			if (!StarSystemPathing._playerSystemNodes[playerId][toSystemId].Any((LinkNodeChild x) => x.SystemId == fromSystemId))
			{
				StarSystemPathing._playerSystemNodes[playerId][toSystemId].Add(new LinkNodeChild
				{
					SystemId = fromSystemId,
					NodeId = nodeId,
					Distance = length
				});
			}
		}
		public static void RemoveNodeLine(int nodeId)
		{
			foreach (KeyValuePair<int, Dictionary<int, List<LinkNodeChild>>> current in StarSystemPathing._playerSystemNodes)
			{
				foreach (KeyValuePair<int, List<LinkNodeChild>> current2 in current.Value)
				{
					current2.Value.RemoveAll((LinkNodeChild x) => x.NodeId == nodeId);
				}
			}
		}
		public static List<int> FindClosestPath(GameDatabase db, int playerId, int fromSystemId, int toSystemId, bool nodeLinesOnly)
		{
			if (!StarSystemPathing._playerSystemNodes.ContainsKey(playerId) || fromSystemId == 0 || toSystemId == 0)
			{
				return new List<int>();
			}
			int num = StarSystemPathing.ClosestNodeSystem(db, playerId, fromSystemId);
			int toNodeSystem = StarSystemPathing.ClosestNodeSystem(db, playerId, toSystemId);
			if (num <= 0 || toNodeSystem <= 0 || (nodeLinesOnly && (num != fromSystemId || toNodeSystem != toSystemId)))
			{
				return new List<int>();
			}
			foreach (KeyValuePair<int, List<LinkNodeChild>> current in StarSystemPathing._playerSystemNodes[playerId])
			{
				foreach (LinkNodeChild current2 in current.Value)
				{
					current2.HasBeenChecked = (current2.SystemId == num);
					current2.ParentLink = null;
					current2.TotalDistance = 0f;
				}
			}
			LinkNodeChild from = new LinkNodeChild
			{
				ParentLink = null,
				HasBeenChecked = true,
				SystemId = num,
				Distance = 0f,
				TotalDistance = 0f,
				NodeId = 0
			};
			StarSystemPathing.LinkNodes(playerId, from, toNodeSystem, 0f);
			List<LinkNodeChild> list = new List<LinkNodeChild>();
			foreach (KeyValuePair<int, List<LinkNodeChild>> current3 in StarSystemPathing._playerSystemNodes[playerId])
			{
				list.AddRange((
					from x in current3.Value
					where x.SystemId == toNodeSystem
					select x).ToList<LinkNodeChild>());
			}
			LinkNodeChild linkNodeChild = null;
			float num2 = 3.40282347E+38f;
			foreach (LinkNodeChild current4 in list)
			{
				bool flag = false;
				for (LinkNodeChild linkNodeChild2 = current4; linkNodeChild2 != null; linkNodeChild2 = linkNodeChild2.ParentLink)
				{
					if (linkNodeChild2.SystemId == num)
					{
						flag = true;
						break;
					}
				}
				if (flag && current4.TotalDistance < num2)
				{
					linkNodeChild = current4;
					num2 = current4.TotalDistance;
				}
			}
			List<int> list2 = new List<int>();
			if (linkNodeChild != null)
			{
				if (linkNodeChild.SystemId != toSystemId)
				{
					list2.Add(toSystemId);
				}
				while (linkNodeChild != null)
				{
					list2.Add(linkNodeChild.SystemId);
					linkNodeChild = linkNodeChild.ParentLink;
				}
				if (list2.ElementAt(list2.Count - 1) != fromSystemId)
				{
					list2.Add(fromSystemId);
				}
			}
			list2.Reverse();
			foreach (KeyValuePair<int, List<LinkNodeChild>> current5 in StarSystemPathing._playerSystemNodes[playerId])
			{
				foreach (LinkNodeChild current6 in current5.Value)
				{
					current6.HasBeenChecked = false;
					current6.ParentLink = null;
					current6.TotalDistance = 0f;
				}
			}
			return list2;
		}
		private static void LinkNodes(int playerId, LinkNodeChild from, int toId, float totalDist)
		{
			if (from == null)
			{
				return;
			}
			foreach (LinkNodeChild current in StarSystemPathing._playerSystemNodes[playerId][from.SystemId])
			{
				float num = current.Distance + totalDist;
				if (!current.HasBeenChecked || num < current.TotalDistance)
				{
					current.TotalDistance = num;
					current.ParentLink = from;
					current.HasBeenChecked = true;
					if (current.SystemId != toId)
					{
						StarSystemPathing.LinkNodes(playerId, current, toId, num);
					}
				}
			}
		}
		private static int ClosestNodeSystem(GameDatabase db, int playerId, int systemId)
		{
			if (StarSystemPathing._playerSystemNodes[playerId].ContainsKey(systemId))
			{
				return systemId;
			}
			StarSystemInfo starSystemInfo = db.GetStarSystemInfo(systemId);
			if (starSystemInfo == null)
			{
				return systemId;
			}
			int result = -1;
			float num = 3.40282347E+38f;
			foreach (KeyValuePair<int, List<LinkNodeChild>> current in StarSystemPathing._playerSystemNodes[playerId])
			{
				StarSystemInfo starSystemInfo2 = db.GetStarSystemInfo(current.Key);
				if (!(starSystemInfo2 == null))
				{
					float lengthSquared = (starSystemInfo2.Origin - starSystemInfo.Origin).LengthSquared;
					if (lengthSquared < num)
					{
						num = lengthSquared;
						result = current.Key;
					}
				}
			}
			return result;
		}
	}
}
