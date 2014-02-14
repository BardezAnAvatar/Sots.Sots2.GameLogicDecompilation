using Kerberos.Sots.Framework;
using System;
using System.IO;
using System.Linq;
namespace Kerberos.Sots.PlayerFramework
{
	internal class AvailableFactionFeatures
	{
		public GrabBag<string> Avatars
		{
			get;
			private set;
		}
		public GrabBag<string> Badges
		{
			get;
			private set;
		}
		public AvailableFactionFeatures(Random random, Faction faction)
		{
			this.Avatars = new GrabBag<string>(random, 
				from x in faction.AvatarTexturePaths
				select Path.GetFileNameWithoutExtension(x));
			this.Badges = new GrabBag<string>(random, 
				from x in faction.BadgeTexturePaths
				select Path.GetFileNameWithoutExtension(x));
		}
	}
}
