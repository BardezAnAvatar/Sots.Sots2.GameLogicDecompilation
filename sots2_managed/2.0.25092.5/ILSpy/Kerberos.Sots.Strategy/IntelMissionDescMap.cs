using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDescMap : IEnumerable<IntelMissionDesc>, IEnumerable
	{
		private readonly IntelMissionDesc[] _items;
		private readonly Dictionary<IntelMission, IntelMissionDesc> _byID;
		private readonly Dictionary<TurnEventType, HashSet<IntelMissionDesc>> _byTurnEvent;
		private static readonly IntelMissionDesc[] NoItems = new IntelMissionDesc[0];
		public IntelMissionDesc Choose(Random random)
		{
			IntelMission value = random.Choose(this._byID.Keys);
			return this.ByID(value);
		}
		public IEnumerable<IntelMissionDesc> ByTurnEventType(TurnEventType value)
		{
			HashSet<IntelMissionDesc> result;
			if (this._byTurnEvent.TryGetValue(value, out result))
			{
				return result;
			}
			return IntelMissionDescMap.NoItems;
		}
		private IntelMissionDesc ByID(IntelMission value)
		{
			IntelMissionDesc result = null;
			this._byID.TryGetValue(value, out result);
			return result;
		}
		public IntelMissionDescMap()
		{
			this._items = new IntelMissionDesc[]
			{
				new IntelMissionDesc_RandomSystem(),
				new IntelMissionDesc_HighestTradeSystem(),
				new IntelMissionDesc_NewestColonySystem(),
				new IntelMissionDesc_CurrentResearch()
			};
			this._byID = new Dictionary<IntelMission, IntelMissionDesc>();
			this._byTurnEvent = new Dictionary<TurnEventType, HashSet<IntelMissionDesc>>();
			IntelMissionDesc[] items = this._items;
			for (int i = 0; i < items.Length; i++)
			{
				IntelMissionDesc intelMissionDesc = items[i];
				this._byID.Add(intelMissionDesc.ID, intelMissionDesc);
				foreach (TurnEventType current in intelMissionDesc.TurnEventTypes)
				{
					HashSet<IntelMissionDesc> hashSet;
					if (!this._byTurnEvent.TryGetValue(current, out hashSet))
					{
						hashSet = new HashSet<IntelMissionDesc>();
						this._byTurnEvent.Add(current, hashSet);
					}
					hashSet.Add(intelMissionDesc);
				}
			}
		}
		public IEnumerator<IntelMissionDesc> GetEnumerator()
		{
			return ((IEnumerable<IntelMissionDesc>)this._items).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._items.GetEnumerator();
		}
	}
}
