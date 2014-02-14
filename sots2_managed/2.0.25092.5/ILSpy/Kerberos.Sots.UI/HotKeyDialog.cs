using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
namespace Kerberos.Sots.UI
{
	internal class HotKeyDialog : Dialog, IHotkeyVKListener
	{
		public const string UIItemAltToggle = "alt_toggle";
		public const string UIItemCtrlToggle = "ctrl_toggle";
		public const string UIItemShiftToggle = "shift_toggle";
		public const string UIItemHotkey = "gameHotKey";
		public const string UIItemClearBinding = "clearBinding";
		public const string UIHotKeyList = "hotkey_list";
		public const string okaybtn = "hotKeyOptions_ok";
		public const string defaultbtn = "hotKeyOptions_default";
		public const string backgroundpanel = "altbackgrnd";
		private HotKeyManager.HotKeyActions _bindingaction = HotKeyManager.HotKeyActions.NoAction;
		public HotKeyDialog(App game) : base(game, "dialogHotKeyMenu")
		{
		}
		public override void Initialize()
		{
			this.PopulateHotKeyList();
			this._app.HotKeyManager.AddVKListener(this);
		}
		private void PopulateHotKeyList()
		{
			this._app.UI.ClearItems(this._app.UI.Path(new string[]
			{
				base.ID,
				"hotkey_list"
			}));
			Array values = Enum.GetValues(typeof(HotKeyManager.HotKeyActions));
			string b = "";
			this._app.UI.AddItem(this._app.UI.Path(new string[]
			{
				base.ID,
				"hotkey_list"
			}), "", 9998, "");
			string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
			{
				base.ID,
				"hotkey_list"
			}), "", 9998, "");
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"inputrow"
			}), false);
			this._app.UI.SetText(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"keyOption"
			}), "");
			this._app.UI.SetVisible(this._app.UI.Path(new string[]
			{
				itemGlobalID,
				"altbackgrnd"
			}), false);
			foreach (HotKeyManager.HotKeyActions hotKeyActions in values)
			{
				if (hotKeyActions != HotKeyManager.HotKeyActions.NoAction)
				{
					string text = hotKeyActions.ToString().Split(new char[]
					{
						'_'
					})[0];
					if (text != b)
					{
						this._app.UI.AddItem(this._app.UI.Path(new string[]
						{
							base.ID,
							"hotkey_list"
						}), "", (9999 * (int)hotKeyActions + 1), "");
						itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
						{
							base.ID,
							"hotkey_list"
						}), "", (9999 * (int)hotKeyActions + 1), "");
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"inputrow"
						}), false);
						this._app.UI.SetText(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"keyOption"
						}), "------ " + App.Localize("@UI_HOTKEY_SUB_" + text.ToUpper()) + " ------");
						this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"keyOption"
						}), "color", 13f, 220f, 255f);
						this._app.UI.SetVisible(this._app.UI.Path(new string[]
						{
							itemGlobalID,
							"altbackgrnd"
						}), false);
						b = text;
					}
					this._app.UI.AddItem(this._app.UI.Path(new string[]
					{
						base.ID,
						"hotkey_list"
					}), "", (int)(hotKeyActions + 1), "");
					string itemGlobalID2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						base.ID,
						"hotkey_list"
					}), "", (int)(hotKeyActions + 1), "");
					HotKeyManager.KeyCombo keyCombo = this._app.HotKeyManager.GetHotKeyCombo(hotKeyActions);
					if (keyCombo == null)
					{
						keyCombo = new HotKeyManager.KeyCombo();
					}
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"inputrow"
					}), true);
					this._app.UI.SetChecked(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"alt_toggle"
					}), keyCombo.alt);
					this._app.UI.SetChecked(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"ctrl_toggle"
					}), keyCombo.control);
					this._app.UI.SetChecked(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"shift_toggle"
					}), keyCombo.shift);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"altbackgrnd"
					}), (((HotKeyManager.HotKeyActions)((int)hotKeyActions % (int)HotKeyManager.HotKeyActions.State_DesignScreen)) == HotKeyManager.HotKeyActions.State_Starmap));
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"keyOption"
					}), App.Localize("@UI_HOTKEY_" + hotKeyActions.ToString().ToUpper()));
					UICommChannel arg_63E_0 = this._app.UI;
					string arg_63E_1 = this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"alt_toggle"
					});
					string arg_63E_2 = "id";
					string arg_639_0 = "alt_toggle|";
					int num = (int)hotKeyActions;
					arg_63E_0.SetPropertyString(arg_63E_1, arg_63E_2, arg_639_0 + num.ToString());
					UICommChannel arg_690_0 = this._app.UI;
					string arg_690_1 = this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"ctrl_toggle"
					});
					string arg_690_2 = "id";
					string arg_68B_0 = "ctrl_toggle|";
					int num2 = (int)hotKeyActions;
					arg_690_0.SetPropertyString(arg_690_1, arg_690_2, arg_68B_0 + num2.ToString());
					UICommChannel arg_6E2_0 = this._app.UI;
					string arg_6E2_1 = this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"shift_toggle"
					});
					string arg_6E2_2 = "id";
					string arg_6DD_0 = "shift_toggle|";
					int num3 = (int)hotKeyActions;
					arg_6E2_0.SetPropertyString(arg_6E2_1, arg_6E2_2, arg_6DD_0 + num3.ToString());
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"gameHotKey",
						"keyLabel"
					}), this._app.HotKeyManager.GetStringforKey(keyCombo.key));
					this._app.UI.SetPropertyColor(this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"keyOption"
					}), "color", 255f, 255f, 255f);
					UICommChannel arg_7DA_0 = this._app.UI;
					string arg_7DA_1 = this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"gameHotKey"
					});
					string arg_7DA_2 = "id";
					string arg_7D5_0 = "gameHotKey|";
					int num4 = (int)hotKeyActions;
					arg_7DA_0.SetPropertyString(arg_7DA_1, arg_7DA_2, arg_7D5_0 + num4.ToString());
					UICommChannel arg_82C_0 = this._app.UI;
					string arg_82C_1 = this._app.UI.Path(new string[]
					{
						itemGlobalID2,
						"clearBinding"
					});
					string arg_82C_2 = "id";
					string arg_827_0 = "clearBinding|";
					int num5 = (int)hotKeyActions;
					arg_82C_0.SetPropertyString(arg_82C_1, arg_82C_2, arg_827_0 + num5.ToString());
				}
			}
		}
		public void UpdateHotkeyUI(List<HotKeyManager.HotKeyActions> hotkeys)
		{
			foreach (HotKeyManager.HotKeyActions current in hotkeys)
			{
				if (current != HotKeyManager.HotKeyActions.NoAction)
				{
					HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(current);
					string itemGlobalID = this._app.UI.GetItemGlobalID(this._app.UI.Path(new string[]
					{
						base.ID,
						"hotkey_list"
					}), "", (int)(current + 1), "");
					UICommChannel arg_C0_0 = this._app.UI;
					UICommChannel arg_B5_0 = this._app.UI;
					string[] array = new string[2];
					array[0] = itemGlobalID;
					string[] arg_B2_0 = array;
					int arg_B2_1 = 1;
					string arg_AD_0 = "alt_toggle|";
					int num = (int)current;
					arg_B2_0[arg_B2_1] = arg_AD_0 + num.ToString();
					arg_C0_0.SetChecked(arg_B5_0.Path(array), hotKeyCombo.alt);
					UICommChannel arg_10D_0 = this._app.UI;
					UICommChannel arg_102_0 = this._app.UI;
					string[] array2 = new string[2];
					array2[0] = itemGlobalID;
					string[] arg_FF_0 = array2;
					int arg_FF_1 = 1;
					string arg_FA_0 = "ctrl_toggle|";
					int num2 = (int)current;
					arg_FF_0[arg_FF_1] = arg_FA_0 + num2.ToString();
					arg_10D_0.SetChecked(arg_102_0.Path(array2), hotKeyCombo.control);
					UICommChannel arg_15A_0 = this._app.UI;
					UICommChannel arg_14F_0 = this._app.UI;
					string[] array3 = new string[2];
					array3[0] = itemGlobalID;
					string[] arg_14C_0 = array3;
					int arg_14C_1 = 1;
					string arg_147_0 = "shift_toggle|";
					int num3 = (int)current;
					arg_14C_0[arg_14C_1] = arg_147_0 + num3.ToString();
					arg_15A_0.SetChecked(arg_14F_0.Path(array3), hotKeyCombo.shift);
					UICommChannel arg_1C0_0 = this._app.UI;
					UICommChannel arg_1A5_0 = this._app.UI;
					string[] array4 = new string[3];
					array4[0] = itemGlobalID;
					string[] arg_199_0 = array4;
					int arg_199_1 = 1;
					string arg_194_0 = "gameHotKey|";
					int num4 = (int)current;
					arg_199_0[arg_199_1] = arg_194_0 + num4.ToString();
					array4[2] = "keyLabel";
					arg_1C0_0.SetText(arg_1A5_0.Path(array4), this._app.HotKeyManager.GetStringforKey(hotKeyCombo.key));
				}
			}
		}
		public bool OnVKReported(Keys key, bool shift, bool ctrl, bool alt)
		{
			this._app.HotKeyManager.SetVkReportMode(false);
			if (this._bindingaction != HotKeyManager.HotKeyActions.NoAction)
			{
				this._app.UI.SetVisible(this._app.UI.Path(new string[]
				{
					base.ID,
					"dialogBinding"
				}), false);
				HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(this._bindingaction);
				hotKeyCombo.shift = shift;
				hotKeyCombo.control = ctrl;
				hotKeyCombo.alt = alt;
				hotKeyCombo.key = key;
				this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(this._bindingaction, hotKeyCombo));
				this._bindingaction = HotKeyManager.HotKeyActions.NoAction;
			}
			return true;
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "hotKeyOptions_ok")
				{
					this._app.HotKeyManager.SaveProfile();
					this._app.HotKeyManager.SyncKeyProfile("");
					this._app.UI.CloseDialog(this, true);
					return;
				}
				if (panelName == "hotKeyOptions_default")
				{
					this._app.HotKeyManager.DeleteProfile();
					this._app.HotKeyManager.CreateProfile(this._app.UserProfile.ProfileName);
					this.PopulateHotKeyList();
					return;
				}
				if (panelName.Contains("clearBinding"))
				{
					HotKeyManager.HotKeyActions action = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split(new char[]
					{
						'|'
					})[1]);
					HotKeyManager.KeyCombo hotKeyCombo = this._app.HotKeyManager.GetHotKeyCombo(action);
					hotKeyCombo.alt = false;
					hotKeyCombo.control = false;
					hotKeyCombo.shift = false;
					hotKeyCombo.key = Keys.None;
					this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(action, hotKeyCombo));
					return;
				}
				if (panelName.Contains("gameHotKey"))
				{
					HotKeyManager.HotKeyActions hotKeyActions = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split(new char[]
					{
						'|'
					})[1]);
					this._app.HotKeyManager.GetHotKeyCombo(hotKeyActions);
					this._app.UI.SetVisible(this._app.UI.Path(new string[]
					{
						base.ID,
						"dialogBinding"
					}), true);
					this._app.UI.SetText(this._app.UI.Path(new string[]
					{
						base.ID,
						"dialogBinding",
						"bindtext"
					}), "Press modifiers + key to bind for action - " + App.Localize("@UI_HOTKEY_" + hotKeyActions.ToString().ToUpper()));
					this._bindingaction = hotKeyActions;
					this._app.HotKeyManager.SetVkReportMode(true);
					return;
				}
			}
			else
			{
				if (msgType == "dialog_closed")
				{
					return;
				}
				if (msgType == "checkbox_clicked")
				{
					if (panelName.StartsWith("alt_toggle"))
					{
						HotKeyManager.HotKeyActions hotKeyActions2 = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split(new char[]
						{
							'|'
						})[1]);
						bool alt = msgParams[0] == "1";
						HotKeyManager.KeyCombo hotKeyCombo2 = this._app.HotKeyManager.GetHotKeyCombo(hotKeyActions2);
						if (hotKeyActions2 != HotKeyManager.HotKeyActions.NoAction)
						{
							hotKeyCombo2.alt = alt;
							this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(hotKeyActions2, hotKeyCombo2));
							return;
						}
					}
					else
					{
						if (panelName.StartsWith("ctrl_toggle"))
						{
							HotKeyManager.HotKeyActions hotKeyActions3 = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split(new char[]
							{
								'|'
							})[1]);
							bool control = msgParams[0] == "1";
							HotKeyManager.KeyCombo hotKeyCombo3 = this._app.HotKeyManager.GetHotKeyCombo(hotKeyActions3);
							if (hotKeyActions3 != HotKeyManager.HotKeyActions.NoAction)
							{
								hotKeyCombo3.control = control;
								this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(hotKeyActions3, hotKeyCombo3));
								return;
							}
						}
						else
						{
							if (panelName.StartsWith("shift_toggle"))
							{
								HotKeyManager.HotKeyActions hotKeyActions4 = (HotKeyManager.HotKeyActions)int.Parse(panelName.Split(new char[]
								{
									'|'
								})[1]);
								bool shift = msgParams[0] == "1";
								HotKeyManager.KeyCombo hotKeyCombo4 = this._app.HotKeyManager.GetHotKeyCombo(hotKeyActions4);
								if (hotKeyActions4 != HotKeyManager.HotKeyActions.NoAction)
								{
									hotKeyCombo4.shift = shift;
									this.UpdateHotkeyUI(this._app.HotKeyManager.SetHotKeyCombo(hotKeyActions4, hotKeyCombo4));
								}
							}
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			this._app.HotKeyManager.RemoveVKListener(this);
			return null;
		}
	}
}
