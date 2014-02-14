using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class DemandSystemSelectDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string ItemList = "lstItems";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private const string StarmapObjectHost = "ohStarmap";
		private int _otherPlayer;
		private DemandType _type;
		private DemandInfo _demand;
		private GameObjectSet _crits;
		private Sky _sky;
		private StarMap _starmap;
		public DemandSystemSelectDialog(App game, DemandType type, int otherPlayer, string template = "dialogRequestSystemSelect") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._demand = new DemandInfo();
			this._demand.InitiatingPlayer = game.LocalPlayer.ID;
			this._demand.ReceivingPlayer = this._otherPlayer;
			this._demand.State = AgreementState.Unrequested;
			this._demand.Type = type;
			this._crits = new GameObjectSet(game);
			this._sky = new Sky(game, SkyUsage.StarMap, 0);
			this._crits.Add(this._sky);
			this._starmap = new StarMap(game, game.Game, this._sky);
			this._crits.Add(this._starmap);
			this._starmap.SetCamera(game.Game.StarMapCamera);
		}
		public override void Initialize()
		{
			this._crits.Activate();
			this._starmap.Sync(this._crits);
			this._app.UI.Send(new object[]
			{
				"SetGameObject",
				this._app.UI.Path(new string[]
				{
					base.ID,
					"ohStarmap"
				}),
				this._starmap.ObjectID
			});
			DiplomacyUI.SyncDiplomacyPopup(this._app, base.ID, this._otherPlayer);
			this.SyncSystemSelect();
		}
        private void SyncSystemSelect()
        {
            Func<ProvinceInfo, bool> predicate = null;
            base._app.UI.SetText("lblHeader", string.Format(App.Localize(DemandTypeDialog.DemandTypeLocMap[this._type]), base._app.AssetDatabase.GetDiplomaticDemandPointCost(this._type)));
            base._app.UI.SetEnabled("btnFinishRequest", true);
            List<StarSystemInfo> StarSystems = base._app.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
            List<int> colonizedSystemIds;
            switch (this._type)
            {
                case DemandType.SystemInfoDemand:
                    {
                        List<StarSystemInfo> source = StarSystems.ToList<StarSystemInfo>();
                        foreach (StarSystemInfo info2 in source)
                        {
                            if (info2.IsVisible)
                            {
                                base._app.UI.AddItem("lstItems", string.Empty, info2.ID, string.Empty);
                                string str2 = base._app.UI.GetItemGlobalID("lstItems", string.Empty, info2.ID, string.Empty);
                                base._app.UI.SetText(base._app.UI.Path(new string[] { str2, "lblHeader" }), info2.Name);
                                base._app.UI.SetText(base._app.UI.Path(new string[] { str2, "lblValue" }), "");
                                this._starmap.PostSetProp("ProvincePoolEffect", new object[] { true, this._starmap.Systems.Reverse[info2.ID] });
                            }
                        }
                        if (source.Count > 0)
                        {
                            base._app.UI.SetSelection("lstItems", source.First<StarSystemInfo>().ID);
                            return;
                        }
                        base._app.UI.SetEnabled("btnFinishRequest", false);
                        return;
                    }
                case DemandType.ResearchPointsDemand:
                case DemandType.SlavesDemand:
                    break;

                case DemandType.WorldDemand:
                    {
                        colonizedSystemIds = base._app.GameDatabase.GetPlayerColonySystemIDs(this._otherPlayer).ToList<int>();
                        List<StarSystemInfo> list3 = (from x in StarSystems
                                                      where colonizedSystemIds.Contains(x.ID) && StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, x, null)
                                                      select x).ToList<StarSystemInfo>();
                        foreach (StarSystemInfo info3 in list3)
                        {
                            if (info3.IsVisible)
                            {
                                base._app.UI.AddItem("lstItems", string.Empty, info3.ID, string.Empty);
                                string str3 = base._app.UI.GetItemGlobalID("lstItems", string.Empty, info3.ID, string.Empty);
                                base._app.UI.SetText(base._app.UI.Path(new string[] { str3, "lblHeader" }), info3.Name);
                                base._app.UI.SetText(base._app.UI.Path(new string[] { str3, "lblValue" }), "");
                                this._starmap.PostSetProp("ProvincePoolEffect", new object[] { true, this._starmap.Systems.Reverse[info3.ID] });
                            }
                        }
                        if (list3.Count > 0)
                        {
                            base._app.UI.SetSelection("lstItems", list3.First<StarSystemInfo>().ID);
                            return;
                        }
                        base._app.UI.SetEnabled("btnFinishRequest", false);
                        break;
                    }
                case DemandType.ProvinceDemand:
                    {
                        if (predicate == null)
                        {
                            predicate = x => (x.PlayerID == this._otherPlayer) && StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, StarSystems.First<StarSystemInfo>(y => y.ID == x.CapitalSystemID), null);
                        }
                        List<ProvinceInfo> list = base._app.GameDatabase.GetProvinceInfos().ToList<ProvinceInfo>().Where<ProvinceInfo>(predicate).ToList<ProvinceInfo>();
                        foreach (ProvinceInfo info in list)
                        {
                            base._app.UI.AddItem("lstItems", string.Empty, info.ID, string.Empty);
                            string str = base._app.UI.GetItemGlobalID("lstItems", string.Empty, info.ID, string.Empty);
                            base._app.UI.SetText(base._app.UI.Path(new string[] { str, "lblHeader" }), info.Name);
                            base._app.UI.SetText(base._app.UI.Path(new string[] { str, "lblValue" }), "");
                            this._starmap.PostSetProp("ProvincePoolEffect", new object[] { true, this._starmap.Systems.Reverse[info.CapitalSystemID] });
                        }
                        if (list.Count > 0)
                        {
                            base._app.UI.SetSelection("lstItems", list.First<ProvinceInfo>().ID);
                            return;
                        }
                        base._app.UI.SetEnabled("btnFinishRequest", false);
                        return;
                    }
                default:
                    return;
            }
        }
        protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnFinishRequest")
				{
					PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
					this._app.GameDatabase.SpendDiplomacyPoints(playerInfo, this._app.GameDatabase.GetPlayerFactionID(this._otherPlayer), this._app.AssetDatabase.GetDiplomaticDemandPointCost(this._type));
					this._app.GameDatabase.InsertDemand(this._demand);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnCancel")
				{
					this._demand = null;
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "list_sel_changed" && panelName == "lstItems")
				{
					if (this._starmap.Systems.Reverse.ContainsKey((int)this._demand.DemandValue))
					{
						this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
						{
							false,
							this._starmap.Systems.Reverse[(int)this._demand.DemandValue]
						});
					}
					this._demand.DemandValue = float.Parse(msgParams[0]);
					if (this._starmap.Systems.Reverse.ContainsKey((int)this._demand.DemandValue))
					{
						this._starmap.SetFocus(this._starmap.Systems.Reverse[(int)this._demand.DemandValue]);
						this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
						{
							true,
							this._starmap.Systems.Reverse[(int)this._demand.DemandValue]
						});
						this._app.Game.StarMapSelectedObject = this._starmap.Systems.Reverse[(int)this._demand.DemandValue];
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = null;
			}
			List<string> list = new List<string>();
			if (this._demand == null)
			{
				list.Add("true");
			}
			else
			{
				list.Add("false");
			}
			return list.ToArray();
		}
	}
}
