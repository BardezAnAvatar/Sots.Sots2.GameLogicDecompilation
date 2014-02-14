using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.GameStates
{
	internal class ProvinceEditStarMapStateMode : StarMapStateMode
	{
		private enum ProvinceEditStage
		{
			SystemSelect,
			CapitalSelect,
			ProvinceName
		}
		private const string UIProvinceEditWindow = "pnlProvinceEditWindow";
		private const string UICommitButton = "btnNextStage";
		private const string UIDescriptionLabel = "lblDescription";
		private const string UITitleLabel = "lblTitle";
		private StarMap _starmap;
		private StarMapState _state;
		private ProvinceEditStarMapStateMode.ProvinceEditStage _stage;
		private string _provinceNameDialog;
		private float MaxProvinceDistance;
		private int MaxSystemsInProvince;
		private int MinSystemsInProvince;
		private List<int> SystemPool;
		private List<int> ProvincePool = new List<int>();
		private List<StarSystemInfo> Systems = new List<StarSystemInfo>();
		private StarSystemInfo Capital;
		public ProvinceEditStarMapStateMode(GameSession sim, StarMapState state, StarMap starmap) : base(sim)
		{
			this._starmap = starmap;
			this._state = state;
		}
		public override void Initialize()
		{
			this.MaxProvinceDistance = base.App.GetStratModifier<float>(StratModifiers.MaxProvincePlanetRange, base.Sim.LocalPlayer.ID);
			this.MaxSystemsInProvince = base.App.GetStratModifier<int>(StratModifiers.MaxProvincePlanets, base.Sim.LocalPlayer.ID);
			this.MinSystemsInProvince = base.App.GetStratModifier<int>(StratModifiers.MinProvincePlanets, base.Sim.LocalPlayer.ID);
			this._starmap.Select(null);
			this._starmap.SelectEnabled = true;
			this.SystemPool = this.GetProvinceableSystems().ToList<int>();
			this.UpdateProvincePool();
			this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.SystemSelect;
			base.App.UI.SetText(base.App.UI.Path(new string[]
			{
				"pnlProvinceEditWindow",
				"lblDescription"
			}), string.Format(App.Localize("@UI_PROVINCE_EDIT_SYSTEM_SELECT"), this.MinSystemsInProvince - this.Systems.Count));
			base.App.UI.SetEnabled(base.App.UI.Path(new string[]
			{
				"pnlProvinceEditWindow",
				"btnNextStage"
			}), false);
			base.App.UI.SetVisible("pnlProvinceEditWindow", true);
		}
		public override void Terminate()
		{
			foreach (int current in this.SystemPool)
			{
				this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
				{
					false,
					this._starmap.Systems.Reverse[current]
				});
				this._starmap.PostSetProp("ProvincePoolEffect", new object[]
				{
					false,
					this._starmap.Systems.Reverse[current]
				});
			}
			this._starmap.SelectEnabled = true;
			base.App.UI.SetVisible("pnlProvinceEditWindow", false);
		}
		public override bool OnGameObjectClicked(IGameObject obj)
		{
			if (obj == null || !typeof(StarMapSystem).IsAssignableFrom(obj.GetType()))
			{
				return true;
			}
			switch (this._stage)
			{
			case ProvinceEditStarMapStateMode.ProvinceEditStage.SystemSelect:
			{
				StarSystemInfo starSystemInfo = base.Sim.GameDatabase.GetStarSystemInfo(this._starmap.Systems.Forward[(StarMapSystem)obj]);
				if (!this.Systems.Contains(starSystemInfo))
				{
					if (this.ProvincePool.Contains(starSystemInfo.ID))
					{
						this.Systems.Add(starSystemInfo);
						this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
						{
							true,
							obj
						});
						this.UpdateProvincePool();
					}
				}
				else
				{
					this.Systems.Remove(starSystemInfo);
					this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
					{
						false,
						obj
					});
					List<StarSystemInfo> list = new List<StarSystemInfo>(this.Systems);
					foreach (StarSystemInfo current in list)
					{
						if (this.Systems.Count <= 1)
						{
							break;
						}
						List<StarSystemInfo> list2 = new List<StarSystemInfo>();
						list2.Add(this.Systems.First<StarSystemInfo>());
						if (!(this.Systems.First<StarSystemInfo>() == current) && !this.isChained(list2, this.Systems, current))
						{
							this.Systems.Remove(current);
							this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
							{
								false,
								this._starmap.Systems.Reverse[current.ID]
							});
						}
					}
					this.UpdateProvincePool();
				}
				base.App.UI.SetText(base.App.UI.Path(new string[]
				{
					"pnlProvinceEditWindow",
					"lblDescription"
				}), string.Format(App.Localize("@UI_PROVINCE_EDIT_SYSTEM_SELECT"), Math.Max(this.MinSystemsInProvince - this.Systems.Count, 0)));
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					"pnlProvinceEditWindow",
					"btnNextStage"
				}), this.Systems.Count >= this.MinSystemsInProvince);
				break;
			}
			case ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect:
			{
				StarSystemInfo starSystemInfo2 = base.Sim.GameDatabase.GetStarSystemInfo(this._starmap.Systems.Forward[(StarMapSystem)obj]);
				if (this.Systems.Contains(starSystemInfo2) && starSystemInfo2 != null && starSystemInfo2 != this.Capital)
				{
					if (this.Capital != null)
					{
						this._starmap.PostSetProp("ProvinceCapitalEffect", new object[]
						{
							false,
							this._starmap.Systems.Reverse[this.Capital.ID]
						});
					}
					this.Capital = starSystemInfo2;
					this._starmap.PostSetProp("ProvinceCapitalEffect", new object[]
					{
						true,
						obj
					});
				}
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					"pnlProvinceEditWindow",
					"btnNextStage"
				}), this.Capital != null);
				break;
			}
			}
			return true;
		}
		public override bool OnGameObjectMouseOver(IGameObject obj)
		{
			return false;
		}
		public override bool OnUIButtonPressed(string panelName)
		{
			if (this._stage == ProvinceEditStarMapStateMode.ProvinceEditStage.SystemSelect && panelName == "btnNextStage")
			{
				this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect;
				base.App.UI.SetText(base.App.UI.Path(new string[]
				{
					"pnlProvinceEditWindow",
					"lblDescription"
				}), App.Localize("@UI_PROVINCE_EDIT_CAPITAL_SELECT"));
				base.App.UI.SetEnabled(base.App.UI.Path(new string[]
				{
					"pnlProvinceEditWindow",
					"btnNextStage"
				}), false);
				return true;
			}
			if (this._stage == ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect && panelName == "btnNextStage")
			{
				this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.ProvinceName;
				this._provinceNameDialog = base.App.UI.CreateDialog(new GenericTextEntryDialog(base.App, App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_TITLE"), App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_DESC"), base.Sim.NamesPool.GetProvinceName(base.Sim.LocalPlayer.Faction.Name), 1024, 2, true, EditBoxFilterMode.None), null);
				base.App.UI.SetVisible("pnlProvinceEditWindow", false);
				return true;
			}
			return false;
		}
		public override bool OnUIDialogClosed(string panelName, string[] msgParams)
		{
			if (panelName == this._provinceNameDialog)
			{
				if (bool.Parse(msgParams[0]))
				{
					List<ProvinceInfo> source = base.Sim.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>();
					if (source.Any((ProvinceInfo x) => x.Name == msgParams[1]))
					{
						this._provinceNameDialog = base.App.UI.CreateDialog(new GenericTextEntryDialog(base.App, App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_TITLE"), App.Localize("@UI_PROVINCE_EDIT_NAME_PROVINCE_DESC"), base.Sim.NamesPool.GetProvinceName(base.Sim.LocalPlayer.Faction.Name), 1024, 2, true, EditBoxFilterMode.None), null);
						base.App.UI.CreateDialog(new GenericTextDialog(base.App, App.Localize("@UI_DIALOGDUPLICATEPROVINCE_TITLE"), string.Format(App.Localize("@UI_DIALOGDUPLICATEPROVINCE_DESC"), msgParams[1]), "dialogGenericMessage"), null);
						return false;
					}
					base.Sim.GameDatabase.InsertProvince(msgParams[1], base.Sim.LocalPlayer.ID, 
						from x in this.Systems
						select x.ID, this.Capital.ID);
					GameSession.ApplyMoralEvent(base.App, MoralEvent.ME_PROVINCE_FORMED, base.Sim.LocalPlayer.ID, null, null, null);
					this._starmap.Sync(this._state.GetCrits());
					this._state.SetProvinceMode(false);
				}
				else
				{
					base.App.UI.SetVisible("pnlProvinceEditWindow", true);
					this._stage = ProvinceEditStarMapStateMode.ProvinceEditStage.CapitalSelect;
				}
				return true;
			}
			return false;
		}
		private static bool IsSystemProvinceable(GameSession sim, int SystemId)
		{
			if (sim.GameDatabase.GetStarSystemProvinceID(SystemId).HasValue)
			{
				return false;
			}
			List<int> list = sim.GameDatabase.GetStarSystemOrbitalObjectIDs(SystemId).ToList<int>();
			foreach (int current in list)
			{
				ColonyInfo colonyInfoForPlanet = sim.GameDatabase.GetColonyInfoForPlanet(current);
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == sim.LocalPlayer.ID && Colony.IsColonySelfSufficient(sim, colonyInfoForPlanet, sim.GameDatabase.GetPlanetInfo(current)))
				{
					return true;
				}
			}
			return false;
		}
		private bool isChained(List<StarSystemInfo> curChain, List<StarSystemInfo> availableSystems, StarSystemInfo target)
		{
			List<StarSystemInfo> systemsInRange = new List<StarSystemInfo>();
			foreach (StarSystemInfo ssi in curChain)
			{
				systemsInRange.AddRange(
					from x in availableSystems
					where !systemsInRange.Contains(x) && !curChain.Contains(x) && (x.Origin - ssi.Origin).Length <= this.MaxProvinceDistance
					select x);
			}
			if (systemsInRange.Count<StarSystemInfo>() == 0)
			{
				return false;
			}
			if (systemsInRange.Contains(target))
			{
				return true;
			}
			foreach (StarSystemInfo current in systemsInRange)
			{
				if (this.isChained(new List<StarSystemInfo>(curChain)
				{
					current
				}, availableSystems, target))
				{
					return true;
				}
			}
			return false;
		}
		private IEnumerable<int> GetProvinceableSystems()
		{
			List<int> source = base.Sim.GameDatabase.GetPlayerColonySystemIDs(base.Sim.LocalPlayer.ID).ToList<int>();
			return (
				from x in source
				where ProvinceEditStarMapStateMode.IsSystemProvinceable(base.Sim, x)
				select x).ToList<int>();
		}
		private void UpdateProvincePool()
		{
			this.ProvincePool.Clear();
			foreach (int current in this.SystemPool)
			{
				if (this.Systems.Count <= 0)
				{
					this._starmap.PostSetProp("ProvincePoolEffect", new object[]
					{
						true,
						this._starmap.Systems.Reverse[current]
					});
					this.ProvincePool.Add(current);
				}
				else
				{
					if (this.Systems.Count >= this.MaxSystemsInProvince)
					{
						this._starmap.PostSetProp("ProvincePoolEffect", new object[]
						{
							false,
							this._starmap.Systems.Reverse[current]
						});
						this.ProvincePool.Remove(current);
					}
					else
					{
						StarSystemInfo ssi = base.Sim.GameDatabase.GetStarSystemInfo(current);
						if (!this.Systems.Contains(ssi) && this.Systems.Any((StarSystemInfo x) => (x.Origin - ssi.Origin).Length <= this.MaxProvinceDistance))
						{
							this._starmap.PostSetProp("ProvincePoolEffect", new object[]
							{
								true,
								this._starmap.Systems.Reverse[current]
							});
							this.ProvincePool.Add(current);
						}
						else
						{
							this._starmap.PostSetProp("ProvincePoolEffect", new object[]
							{
								false,
								this._starmap.Systems.Reverse[current]
							});
							this.ProvincePool.Remove(current);
						}
					}
				}
			}
		}
	}
}
