using Kerberos.Sots.Framework;
using System;
namespace Kerberos.Sots
{
	internal class DefaultStarModelParameters
	{
		public static readonly float RadiusScale = 1E-06f;
		public static readonly string ImposterMaterial = "StarCorona";
		public static readonly Vector2 ImposterSpriteScale = new Vector2(0.25f, 0.25f);
		public static readonly Vector2 ImposterRange = new Vector2(0f, 10f);
		public static readonly Vector3 ImposterColor = new Vector3(0.6f, 0.8f, 1f);
	}
}
