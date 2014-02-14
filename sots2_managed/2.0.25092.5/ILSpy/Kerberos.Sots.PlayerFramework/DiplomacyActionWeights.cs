using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace Kerberos.Sots.PlayerFramework
{
	public class DiplomacyActionWeights
	{
		public class DiplomacyActionWeight
		{
			public DiplomaticMood Mood;
			public DiplomacyAction DiplomacyAction;
			public float Value;
			public object Type;
		}
		private List<DiplomacyActionWeights.DiplomacyActionWeight> Weights = new List<DiplomacyActionWeights.DiplomacyActionWeight>();
		public DiplomacyActionWeights()
		{
		}
		public DiplomacyActionWeights(XmlDocument doc)
		{
			IEnumerable<XmlElement> enumerable = 
				from x in doc["DiplomacyActionWeights"].OfType<XmlElement>()
				where x.Name == "DiplomacyReaction"
				select x;
			foreach (XmlElement current in enumerable)
			{
				DiplomaticMood mood = (DiplomaticMood)Enum.Parse(typeof(DiplomaticMood), current.GetAttribute("value"));
				IEnumerable<XmlElement> enumerable2 = 
					from x in current.OfType<XmlElement>()
					where x.Name == "Action"
					select x;
				foreach (XmlElement current2 in enumerable2)
				{
					DiplomacyActionWeights.DiplomacyActionWeight diplomacyActionWeight = new DiplomacyActionWeights.DiplomacyActionWeight();
					diplomacyActionWeight.Mood = mood;
					diplomacyActionWeight.DiplomacyAction = (DiplomacyAction)Enum.Parse(typeof(DiplomacyAction), current2.GetAttribute("id"));
					diplomacyActionWeight.Value = float.Parse(current2.GetAttribute("value"));
					if (DiplomacyActionWeights.RequiresType(diplomacyActionWeight.DiplomacyAction) && !current2.HasAttribute("type"))
					{
						throw new Exception(string.Format("XML node for diplomatic action type: {0} requires a type.", diplomacyActionWeight.DiplomacyAction.ToString()));
					}
					switch (diplomacyActionWeight.DiplomacyAction)
					{
					case DiplomacyAction.REQUEST:
						this.ProcessRequest(current2, diplomacyActionWeight);
						break;
					case DiplomacyAction.DEMAND:
						this.ProcessDemand(current2, diplomacyActionWeight);
						break;
					case DiplomacyAction.TREATY:
						this.ProcessTreaty(current2, diplomacyActionWeight);
						break;
					case DiplomacyAction.LOBBY:
						this.ProcessLobby(current2, diplomacyActionWeight);
						break;
					}
					this.Weights.Add(diplomacyActionWeight);
				}
			}
		}
		public Dictionary<DiplomacyAction, float> GetCumulativeWeights(DiplomaticMood mood)
		{
			Dictionary<DiplomacyAction, float> dictionary = new Dictionary<DiplomacyAction, float>();
			Array values = Enum.GetValues(typeof(DiplomacyAction));
			foreach (object current in values)
			{
				float cumulativeWeight = this.GetCumulativeWeight((DiplomacyAction)current, mood);
				dictionary.Add((DiplomacyAction)current, cumulativeWeight);
			}
			return dictionary;
		}
		public float GetCumulativeWeight(DiplomacyAction action, DiplomaticMood mood)
		{
			float num = 0f;
			switch (action)
			{
			case DiplomacyAction.REQUEST:
			{
				Array values = Enum.GetValues(typeof(RequestType));
				foreach (object current in values)
				{
					num += this.GetWeight(action, mood, current);
				}
				num /= (float)values.Length;
				break;
			}
			case DiplomacyAction.DEMAND:
			{
				Array values2 = Enum.GetValues(typeof(DemandType));
				foreach (object current2 in values2)
				{
					num += this.GetWeight(action, mood, current2);
				}
				num /= (float)values2.Length;
				break;
			}
			case DiplomacyAction.TREATY:
			{
				Array values3 = Enum.GetValues(typeof(TreatyType));
				Array values4 = Enum.GetValues(typeof(LimitationTreatyType));
				foreach (object current3 in values3)
				{
					if ((TreatyType)current3 != TreatyType.Limitation)
					{
						num += this.GetWeight(action, mood, current3);
					}
				}
				foreach (object current4 in values4)
				{
					num += this.GetWeight(action, mood, current4);
				}
				num /= (float)(values3.Length + values4.Length);
				break;
			}
			case DiplomacyAction.LOBBY:
			{
				Array values5 = Enum.GetValues(typeof(LobbyType));
				foreach (object current5 in values5)
				{
					num += this.GetWeight(action, mood, current5);
				}
				num /= (float)values5.Length;
				break;
			}
			default:
				num += this.GetWeight(action, mood, null);
				break;
			}
			return num;
		}
		public Dictionary<object, float> GetWeights(DiplomacyAction action, DiplomaticMood mood)
		{
			Dictionary<object, float> dictionary = new Dictionary<object, float>();
			List<Type> types = DiplomacyActionWeights.GetTypes(action);
			if (types != null)
			{
				using (List<Type>.Enumerator enumerator = types.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Type current = enumerator.Current;
						Array values = Enum.GetValues(current);
						foreach (object current2 in values)
						{
							dictionary.Add(current2, this.GetWeight(action, mood, current2));
						}
					}
					return dictionary;
				}
			}
			float weight = this.GetWeight(action, mood, null);
			dictionary.Add(action, weight);
			return dictionary;
		}
		public float GetWeight(DiplomacyAction action, DiplomaticMood mood, object type = null)
		{
			if (DiplomacyActionWeights.RequiresType(action) && type == null)
			{
				throw new Exception("Action requires a type.");
			}
			DiplomacyActionWeights.DiplomacyActionWeight diplomacyActionWeight;
			if (type == null)
			{
				diplomacyActionWeight = this.Weights.FirstOrDefault((DiplomacyActionWeights.DiplomacyActionWeight x) => x.DiplomacyAction == action && x.Mood == mood);
			}
			else
			{
				diplomacyActionWeight = this.Weights.FirstOrDefault((DiplomacyActionWeights.DiplomacyActionWeight x) => x.DiplomacyAction == action && x.Mood == mood && x.Type.Equals(type));
			}
			if (diplomacyActionWeight == null)
			{
				return 1f;
			}
			return diplomacyActionWeight.Value;
		}
		private static List<Type> GetTypes(DiplomacyAction action)
		{
			List<Type> list = new List<Type>();
			switch (action)
			{
			case DiplomacyAction.REQUEST:
				list.Add(typeof(RequestType));
				break;
			case DiplomacyAction.DEMAND:
				list.Add(typeof(DemandType));
				break;
			case DiplomacyAction.TREATY:
				list.Add(typeof(TreatyType));
				list.Add(typeof(LimitationTreatyType));
				break;
			case DiplomacyAction.LOBBY:
				list.Add(typeof(LobbyType));
				break;
			default:
				return null;
			}
			return list;
		}
		public static DiplomacyAction GetActionFromType(Type type)
		{
			if (type == typeof(LobbyType))
			{
				return DiplomacyAction.LOBBY;
			}
			if (type == typeof(DemandType))
			{
				return DiplomacyAction.DEMAND;
			}
			if (type == typeof(RequestType))
			{
				return DiplomacyAction.REQUEST;
			}
			if (type == typeof(TreatyType) || type == typeof(LimitationTreatyType))
			{
				return DiplomacyAction.TREATY;
			}
			throw new Exception("Unable to determine action for type.");
		}
		public static bool RequiresType(DiplomacyAction action)
		{
			return action == DiplomacyAction.DEMAND || action == DiplomacyAction.LOBBY || action == DiplomacyAction.REQUEST || action == DiplomacyAction.TREATY;
		}
		private void ProcessDemand(XmlElement element, DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			DemandType demandType = (DemandType)Enum.Parse(typeof(DemandType), element.GetAttribute("type"));
			weight.Type = demandType;
		}
		private void ProcessLobby(XmlElement element, DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			LobbyType lobbyType = (LobbyType)Enum.Parse(typeof(LobbyType), element.GetAttribute("type"));
			weight.Type = lobbyType;
		}
		private void ProcessRequest(XmlElement element, DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			RequestType requestType = (RequestType)Enum.Parse(typeof(RequestType), element.GetAttribute("type"));
			weight.Type = requestType;
		}
		private void ProcessTreaty(XmlElement element, DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			string text = element.GetAttribute("type");
			if (text.Contains("Limitation"))
			{
				text = text.Replace("Limitation", "");
				LimitationTreatyType limitationTreatyType = (LimitationTreatyType)Enum.Parse(typeof(LimitationTreatyType), text);
				weight.Type = limitationTreatyType;
				return;
			}
			TreatyType arg_62_0 = (TreatyType)Enum.Parse(typeof(TreatyType), text);
		}
	}
}
