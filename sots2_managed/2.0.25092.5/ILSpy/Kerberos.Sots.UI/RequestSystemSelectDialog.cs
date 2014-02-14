using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.UI
{
	internal class RequestSystemSelectDialog : Dialog
	{
		private const string HeaderLabel = "lblHeader";
		private const string ItemList = "lstItems";
		private const string RequestButton = "btnFinishRequest";
		private const string CancelButton = "btnCancel";
		private const string StarmapObjectHost = "ohStarmap";
		private int _otherPlayer;
		private RequestType _type;
		private RequestInfo _request;
		private GameObjectSet _crits;
		private Sky _sky;
		private StarMap _starmap;
		public RequestSystemSelectDialog(App game, RequestType type, int otherPlayer, string template = "dialogRequestSystemSelect") : base(game, template)
		{
			this._otherPlayer = otherPlayer;
			this._type = type;
			this._request = new RequestInfo();
			this._request.InitiatingPlayer = game.LocalPlayer.ID;
			this._request.ReceivingPlayer = this._otherPlayer;
			this._request.State = AgreementState.Unrequested;
			this._request.Type = type;
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
			this._app.UI.SetText("lblHeader", string.Format(App.Localize(RequestTypeDialog.RequestTypeLocMap[this._type]), this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type)));
			this._app.UI.SetEnabled("btnFinishRequest", true);
			List<StarSystemInfo> source = this._app.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			switch (this._type)
			{
			case RequestType.SystemInfoRequest:
			{
				List<StarSystemInfo> list = source.ToList<StarSystemInfo>();
				foreach (StarSystemInfo current in list)
				{
					if (current.IsVisible)
					{
						this._app.UI.AddItem("lstItems", string.Empty, current.ID, string.Empty);
						string itemGlobalID = this._app.UI.GetItemGlobalID("lstItems", string.Empty, current.ID, string.Empty);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"lblHeader"
						}), current.Name);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"lblValue"
						}), "");
						this._starmap.PostSetProp("ProvincePoolEffect", new object[]
						{
							true,
							this._starmap.Systems.Reverse[current.ID]
						});
					}
				}
				if (list.Count > 0)
				{
					this._app.UI.SetSelection("lstItems", list.First<StarSystemInfo>().ID);
					return;
				}
				this._app.UI.SetEnabled("btnFinishRequest", false);
				break;
			}
			case RequestType.ResearchPointsRequest:
				break;
			case RequestType.MilitaryAssistanceRequest:
			{
				List<StarSystemInfo> list = (
					from x in source
					where this._app.GameDatabase.IsSurveyed(this._app.LocalPlayer.ID, x.ID)
					select x).ToList<StarSystemInfo>();
				foreach (StarSystemInfo current2 in list)
				{
					if (current2.IsVisible)
					{
						this._app.UI.AddItem("lstItems", string.Empty, current2.ID, string.Empty);
						string itemGlobalID2 = this._app.UI.GetItemGlobalID("lstItems", string.Empty, current2.ID, string.Empty);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"lblHeader"
						}), current2.Name);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID2,
							"lblValue"
						}), "");
						this._starmap.PostSetProp("ProvincePoolEffect", new object[]
						{
							true,
							this._starmap.Systems.Reverse[current2.ID]
						});
					}
				}
				if (list.Count > 0)
				{
					this._app.UI.SetSelection("lstItems", list.First<StarSystemInfo>().ID);
					return;
				}
				this._app.UI.SetEnabled("btnFinishRequest", false);
				return;
			}
			case RequestType.GatePermissionRequest:
			{
				List<int> colonizedSystemIds = this._app.GameDatabase.GetPlayerColonySystemIDs(this._otherPlayer).ToList<int>();
				List<StarSystemInfo> list2 = (
					from x in source
					where colonizedSystemIds.Contains(x.ID) && StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, x, null)
					select x).ToList<StarSystemInfo>();
				foreach (StarSystemInfo current3 in list2)
				{
					if (current3.IsVisible)
					{
						this._app.UI.AddItem("lstItems", string.Empty, current3.ID, string.Empty);
						string itemGlobalID3 = this._app.UI.GetItemGlobalID("lstItems", string.Empty, current3.ID, string.Empty);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID3,
							"lblHeader"
						}), current3.Name);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID3,
							"lblValue"
						}), "");
						this._starmap.PostSetProp("ProvincePoolEffect", new object[]
						{
							true,
							this._starmap.Systems.Reverse[current3.ID]
						});
					}
				}
				if (list2.Count > 0)
				{
					this._app.UI.SetSelection("lstItems", list2.First<StarSystemInfo>().ID);
					return;
				}
				this._app.UI.SetEnabled("btnFinishRequest", false);
				return;
			}
			case RequestType.EstablishEnclaveRequest:
			{
				List<int> colonizedSystemIds = this._app.GameDatabase.GetPlayerColonySystemIDs(this._otherPlayer).ToList<int>();
				List<StarSystemInfo> list2 = (
					from x in source
					where colonizedSystemIds.Contains(x.ID) && StarMap.IsInRange(this._app.Game.GameDatabase, this._app.LocalPlayer.ID, x, null)
					select x).ToList<StarSystemInfo>();
				foreach (StarSystemInfo current4 in list2)
				{
					if (current4.IsVisible)
					{
						this._app.UI.AddItem("lstItems", string.Empty, current4.ID, string.Empty);
						string itemGlobalID4 = this._app.UI.GetItemGlobalID("lstItems", string.Empty, current4.ID, string.Empty);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID4,
							"lblHeader"
						}), current4.Name);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID4,
							"lblValue"
						}), "");
						this._starmap.PostSetProp("ProvincePoolEffect", new object[]
						{
							true,
							this._starmap.Systems.Reverse[current4.ID]
						});
					}
				}
				if (list2.Count > 0)
				{
					this._app.UI.SetSelection("lstItems", list2.First<StarSystemInfo>().ID);
					return;
				}
				this._app.UI.SetEnabled("btnFinishRequest", false);
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
					this._app.GameDatabase.SpendDiplomacyPoints(playerInfo, this._app.GameDatabase.GetPlayerFactionID(this._otherPlayer), this._app.AssetDatabase.GetDiplomaticRequestPointCost(this._type));
					this._app.GameDatabase.InsertRequest(this._request);
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "btnCancel")
				{
					this._request = null;
					this._app.UI.CloseDialog(this, true);
					return;
				}
			}
			else
			{
				if (msgType == "list_sel_changed")
				{
					if (this._starmap.Systems.Reverse.ContainsKey((int)this._request.RequestValue))
					{
						this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
						{
							false,
							this._starmap.Systems.Reverse[(int)this._request.RequestValue]
						});
					}
					this._request.RequestValue = float.Parse(msgParams[0]);
					if (this._starmap.Systems.Reverse.ContainsKey((int)this._request.RequestValue))
					{
						this._starmap.SetFocus(this._starmap.Systems.Reverse[(int)this._request.RequestValue]);
						this._starmap.PostSetProp("ProvinceSystemSelectEffect", new object[]
						{
							true,
							this._starmap.Systems.Reverse[(int)this._request.RequestValue]
						});
						this._app.Game.StarMapSelectedObject = this._starmap.Systems.Reverse[(int)this._request.RequestValue];
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			List<string> list = new List<string>();
			if (this._request == null)
			{
				list.Add("true");
			}
			else
			{
				list.Add("false");
			}
			this._crits.Dispose();
			this._app.GetGameState<StarMapState>().RefreshCameraControl();
			return list.ToArray();
		}
	}
}
