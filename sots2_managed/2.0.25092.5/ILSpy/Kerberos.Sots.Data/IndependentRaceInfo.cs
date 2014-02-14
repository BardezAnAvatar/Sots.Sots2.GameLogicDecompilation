using System;
namespace Kerberos.Sots.Data
{
	internal class IndependentRaceInfo : IIDProvider
	{
		public string Name;
		public int OrbitalObjectID;
		public double Population;
		public int TechLevel;
		public int ReactionHuman;
		public int ReactionTarka;
		public int ReactionLiir;
		public int ReactionZuul;
		public int ReactionMorrigi;
		public int ReactionHiver;
		public int ID
		{
			get;
			set;
		}
	}
}
