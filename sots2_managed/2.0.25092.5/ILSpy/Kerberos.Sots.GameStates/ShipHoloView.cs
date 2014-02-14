using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIPHOLOVIEW)]
	internal class ShipHoloView : GameObject, IDisposable
	{
		public ShipHoloView(App game, OrbitCameraController cameraController)
		{
			game.AddExistingObject(this, new object[]
			{
				cameraController.GetObjectID()
			});
		}
		public void SetUseViewport(bool value)
		{
			this.PostSetProp("UseViewport", value);
		}
		public void HideViewport(bool value)
		{
			this.PostSetProp("HideViewport", value);
		}
		public void SetShip(Ship value)
		{
			this.PostSetProp("Ship", value.GetObjectID());
		}
		public void AddWeaponGroupIcon(WeaponBank weaponBank)
		{
			this.PostSetProp("AddWeaponGroupIcon", weaponBank.GetObjectID());
		}
		public void AddModuleIcon(Module selectedModule, Section defaultShipSection, string defaultModelNodeName, string iconSpriteName)
		{
			this.PostSetProp("AddModuleIcon", new object[]
			{
				selectedModule.GetObjectID(),
				defaultShipSection.GetObjectID(),
				defaultModelNodeName,
				iconSpriteName
			});
		}
		public void AddPsionicIcon(Module selectedModule, int psionicid, int elementid)
		{
			this.PostSetProp("AddPsionicIcon", new object[]
			{
				selectedModule.GetObjectID(),
				psionicid,
				elementid
			});
		}
		public void ClearSelection()
		{
			this.PostSetProp("ClearSelection", new object[0]);
		}
		public void Dispose()
		{
			if (base.App != null)
			{
				base.App.ReleaseObject(this);
			}
		}
	}
}
