using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
namespace Kerberos.Sots.GameStates
{
	internal class TestPlanetState : GameState
	{
		private GameObjectSet _crits;
		private Sky _sky;
		private OrbitCameraController _camera;
		private StellarBody _planet;
		private bool _planetDirty = true;
		private bool _planetReady;
		private string[] _types;
		private string[] _factions;
		private string _faction;
		private string _type;
		private int _hazard;
		private double _population;
		private float _biosphere;
		private int _typeVariant;
		private int _seed;
		private int _rotation;
		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._crits = new GameObjectSet(base.App);
			this._sky = new Sky(base.App, SkyUsage.InSystem, 0);
			this._crits.Add(this._sky);
			this._camera = this._crits.Add<OrbitCameraController>(new object[0]);
			this._types = base.App.AssetDatabase.PlanetGenerationRules.GetStellarBodyTypes();
			this._factions = base.App.AssetDatabase.PlanetGenerationRules.GetFactions();
			base.App.UI.LoadScreen("TestPlanet");
		}
		protected override void OnEnter()
		{
			base.App.UI.SetScreen("TestPlanet");
			base.App.UI.PanelMessage += new UIEventPanelMessage(this.UI_PanelMessage);
			this._camera.MaxDistance = 200000f;
			this._camera.DesiredDistance = 50000f;
			base.App.UI.ClearItems("factionList");
			for (int i = 0; i < this._factions.Length; i++)
			{
				base.App.UI.AddItem("factionList", string.Empty, i, this._factions[i]);
			}
			for (int j = 0; j < this._types.Length; j++)
			{
				base.App.UI.AddItem("typeList", string.Empty, j, this._types[j]);
			}
			base.App.UI.SetSelection("factionList", 0);
			base.App.UI.SetSelection("typeList", 0);
			base.App.UI.InitializeSlider("partRotationSlider", 0, 360, 0);
			base.App.UI.InitializeSlider("partHazardSlider", 0, 1500, 0);
			base.App.UI.InitializeSlider("partPopulationSlider", 0, 1000, 0);
			base.App.UI.InitializeSlider("partBiosphereSlider", 0, 1000, 0);
			base.App.UI.InitializeSlider("partRandomSeedSlider", 0, 1000, 0);
			base.App.UI.InitializeSlider("partTypeVariantSlider", 0, 20, 0);
			this._type = this._types[0];
			this._faction = this._factions[0];
			this._crits.Activate();
		}
		private void Regenerate()
		{
			if (this._planet != null)
			{
				base.App.ReleaseObject(this._planet);
			}
			this._planetReady = false;
			this._planet = StellarBody.Create(base.App, base.App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(string.Empty, Vector3.Zero, 5000f, this._seed, 0, this._type, (float)this._hazard, 750f, this._faction, this._biosphere, this._population, new int?(this._typeVariant), ColonyStage.Open, SystemColonyType.Normal));
			this._planet.PostSetProp("Rotation", MathHelper.DegreesToRadians((float)this._rotation));
		}
		private void UI_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "factionList")
				{
					if (!string.IsNullOrEmpty(msgParams[0]))
					{
						this._faction = this._factions[int.Parse(msgParams[0])];
					}
					else
					{
						this._faction = null;
					}
					this._planetDirty = true;
					return;
				}
				if (panelName == "typeList")
				{
					if (!string.IsNullOrEmpty(msgParams[0]))
					{
						this._type = this._types[int.Parse(msgParams[0])];
					}
					else
					{
						this._type = null;
					}
					this._planetDirty = true;
					return;
				}
			}
			else
			{
				if (msgType == "slider_value")
				{
					if (panelName == "partRotationSlider")
					{
						this._rotation = int.Parse(msgParams[0]);
						this._planetDirty = true;
						return;
					}
					if (panelName == "partHazardSlider")
					{
						this._hazard = int.Parse(msgParams[0]);
						this._planetDirty = true;
						return;
					}
					if (panelName == "partPopulationSlider")
					{
						this._population = (double)int.Parse(msgParams[0]) * 1000000.0;
						this._planetDirty = true;
						return;
					}
					if (panelName == "partBiosphereSlider")
					{
						this._biosphere = (float)int.Parse(msgParams[0]);
						this._planetDirty = true;
						return;
					}
					if (panelName == "partRandomSeedSlider")
					{
						this._seed = int.Parse(msgParams[0]);
						this._planetDirty = true;
						return;
					}
					if (panelName == "partTypeVariantSlider")
					{
						this._typeVariant = int.Parse(msgParams[0]);
						this._planetDirty = true;
						return;
					}
				}
				else
				{
					if (msgType == "button_clicked" && "regenerateButton" == panelName)
					{
						this._planetDirty = true;
						base.App.AssetDatabase.PlanetGenerationRules.Reload();
					}
				}
			}
		}
		protected override void OnExit(GameState prev, ExitReason reason)
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
			}
			if (this._planet != null)
			{
				base.App.ReleaseObject(this._planet);
			}
			this._planet = null;
		}
		protected override void OnUpdate()
		{
			if (this._planetDirty)
			{
				this._planetDirty = false;
				this.Regenerate();
			}
			if (!this._planetReady && this._planet != null && this._planet.ObjectStatus != GameObjectStatus.Pending)
			{
				this._planetReady = true;
				this._planet.PostSetActive(true);
			}
		}
		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}
		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
		public TestPlanetState(App game) : base(game)
		{
		}
	}
}
