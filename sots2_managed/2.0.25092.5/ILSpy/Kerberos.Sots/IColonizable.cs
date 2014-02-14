using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
namespace Kerberos.Sots
{
	internal interface IColonizable
	{
		Colony Colony
		{
			get;
		}
		int PopSize
		{
			get;
		}
		bool HasColony();
		bool Colonize(Faction faction, long count);
	}
}
