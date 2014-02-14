using System;
namespace Kerberos.Sots.UI
{
	internal class AutoMenuDialog : Dialog
	{
		public const string UIAutoDefenseTog = "autoDefences";
		public const string UIAutoRepairTog = "autoRepair";
		public const string UIAutoGoopTog = "autoGoop";
		public const string UIAutoJokerTog = "autoJoker";
		public const string UIAutoAOETog = "autoAOE";
		public const string UIAutoPatrolTog = "autoPatrol";
		public const string okaybtn = "autoOptions_ok";
		public AutoMenuDialog(App game) : base(game, "dialog_autoOptions")
		{
		}
		public override void Initialize()
		{
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"autoDefences"
			}), this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPlaceDefenseAssets);
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"autoRepair"
			}), this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoRepairShips);
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"autoGoop"
			}), this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseGoopModules);
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"autoJoker"
			}), this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseJokerModules);
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"autoAOE"
			}), this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoAoe);
			this._app.UI.SetChecked(this._app.UI.Path(new string[]
			{
				base.ID,
				"autoPatrol"
			}), this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPatrol);
		}
		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "autoOptions_ok")
				{
					this._app.UI.CloseDialog(this, true);
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
					if (panelName == "autoDefences")
					{
						bool flag = msgParams[0] == "1";
						if (flag != this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPlaceDefenseAssets)
						{
							this._app.GameDatabase.UpdatePlayerAutoPlaceDefenses(this._app.LocalPlayer.ID, flag);
							this._app.UserProfile.AutoPlaceDefenseAssets = flag;
							this._app.UserProfile.SaveProfile();
							return;
						}
					}
					else
					{
						if (panelName == "autoRepair")
						{
							bool flag2 = msgParams[0] == "1";
							if (flag2 != this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoRepairShips)
							{
								this._app.GameDatabase.UpdatePlayerAutoRepairFleets(this._app.LocalPlayer.ID, flag2);
								this._app.UserProfile.AutoRepairFleets = flag2;
								this._app.UserProfile.SaveProfile();
								return;
							}
						}
						else
						{
							if (panelName == "autoGoop")
							{
								bool flag3 = msgParams[0] == "1";
								if (flag3 != this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseGoopModules)
								{
									this._app.GameDatabase.UpdatePlayerAutoUseGoop(this._app.LocalPlayer.ID, flag3);
									this._app.UserProfile.AutoUseGoop = flag3;
									this._app.UserProfile.SaveProfile();
									return;
								}
							}
							else
							{
								if (panelName == "autoJoker")
								{
									bool flag4 = msgParams[0] == "1";
									if (flag4 != this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoUseJokerModules)
									{
										this._app.GameDatabase.UpdatePlayerAutoUseJoker(this._app.LocalPlayer.ID, flag4);
										this._app.UserProfile.AutoUseJoker = flag4;
										this._app.UserProfile.SaveProfile();
										return;
									}
								}
								else
								{
									if (panelName == "autoAOE")
									{
										bool flag5 = msgParams[0] == "1";
										if (flag5 != this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoAoe)
										{
											this._app.GameDatabase.UpdatePlayerAutoUseAOE(this._app.LocalPlayer.ID, flag5);
											this._app.UserProfile.AutoAOE = flag5;
											this._app.UserProfile.SaveProfile();
											return;
										}
									}
									else
									{
										if (panelName == "autoPatrol")
										{
											bool flag6 = msgParams[0] == "1";
											if (flag6 != this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID).AutoPatrol)
											{
												this._app.GameDatabase.UpdatePlayerAutoPatrol(this._app.LocalPlayer.ID, flag6);
												this._app.UserProfile.AutoPatrol = flag6;
												this._app.UserProfile.SaveProfile();
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		public override string[] CloseDialog()
		{
			return null;
		}
	}
}
