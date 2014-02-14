using System;
namespace Kerberos.Sots.Data
{
	internal class PlanetInfo : IIDProvider
	{
		public string Type;
		public int? RingID;
		public float Suitability;
		public int Biosphere;
		public int Resources;
		public int MaxResources;
		public float Infrastructure;
		public float Size;
		public int ID
		{
			get;
			set;
		}
		public static bool AreSame(PlanetInfo s1, PlanetInfo s2)
		{
			return s1.ID == s2.ID && s1.Type == s2.Type && !s1.RingID.HasValue == !s2.RingID.HasValue && (!s1.RingID.HasValue || s1.RingID.Value == s2.RingID.Value) && s1.Suitability == s2.Suitability && s1.Biosphere == s2.Biosphere && s1.Resources == s2.Resources && s1.Infrastructure == s2.Infrastructure && s1.Size == s2.Size;
		}
	}
}
