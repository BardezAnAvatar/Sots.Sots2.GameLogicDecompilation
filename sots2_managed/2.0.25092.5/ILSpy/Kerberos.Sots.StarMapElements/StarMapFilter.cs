using Kerberos.Sots.Engine;
using System;
namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPFILTER)]
	internal class StarMapFilter : StarMapObject
	{
		public StarMapFilter(App game)
		{
			game.AddExistingObject(this, new object[0]);
		}
		public void SetLabel(string value)
		{
			this.PostSetProp("Label", value);
		}
		public void SetFilterType(StarMapViewFilter type)
		{
			this.PostSetProp("FilterType", new object[]
			{
				type
			});
		}
	}
}
