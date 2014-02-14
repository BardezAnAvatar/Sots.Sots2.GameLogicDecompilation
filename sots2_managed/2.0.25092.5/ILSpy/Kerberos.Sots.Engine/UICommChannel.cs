using Kerberos.Sots.Framework;
using Kerberos.Sots.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Kerberos.Sots.Engine
{
	internal class UICommChannel
	{
		public enum AnchorPoint
		{
			TopLeft
		}
		public const string GameEventObjectClicked = "ObjectClicked";
		public const string GameEventContextMenu = "ContextMenu";
		public const string GameEventListContextMenu = "ListContextMenu";
		public const string GameEventDragAndDrop = "DragAndDropEvent";
		public const string GameEventMouseOver = "MouseOver";
		public const string GameEventLocalizeText = "LocalizeText";
		public const string ScreenReadyID = "screen_ready";
		public const string ScreenLoadedID = "screen_loaded";
		public const string TextChangedMsg = "text_changed";
		public const string TextConfirmedMsg = "edit_confirmed";
		public const string ButtonClickedID = "button_clicked";
		public const string ButtonRightClickedID = "button_rclicked";
		public const string EndTurnActivated = "endturn_activated";
		public const string SliderValueChangedMsg = "slider_value";
		public const string SliderOnNotchedMsg = "slider_notched";
		public const string ListSelectionChangedID = "list_sel_changed";
		public const string ListItemDblClickedID = "list_item_dblclk";
		public const string ListItemRightClickedID = "list_item_rightclk";
		public const string CheckBoxClickedMsg = "checkbox_clicked";
		public const string ColorChangedMsg = "color_changed";
		public const string DialogClosedMsg = "dialog_closed";
		public const string DialogOpenedMsg = "dialog_opened";
		public const string MovieDoneMsg = "movie_done";
		public const string MouseEnter = "mouse_enter";
		public const string MouseLeave = "mouse_leave";
		public const string ChatMessageReceived = "ChatMessage";
		public const string SliderPropValueMin = "value_min";
		public const string SliderPropValueMax = "value_max";
		public const string SliderPropValue = "value";
		public const string TextBoxPropTextFile = "text_file";
		public const string CheckBoxPropChecked = "checked";
		public const string LabelValuePropLabel = "label";
		public const string LabelValuePropValue = "value";
		public const string PanelPropText = "text";
		public const string ImagePropColor = "color";
		public const string ButtonTimeout = "timeout";
		public const string SliderPropNotchTolerance = "notch_tolerance";
		public const string SliderPropAddNotch = "add_notch";
		public const string SliderPropRemoveNotch = "rem_notch";
		public const string SliderPropClearNotches = "clear_notchs";
		public const string SliderSnapToNearestNotch = "auto_notch_snap";
		public const string PlayerInfoRequestResponse = "PlayerInfoRequestResponse";
		public const string PlayerStatusChanged = "PlayerStatusChanged";
		public static bool LogEnable;
		private ScriptMessageWriter _scriptMessageWriter = new ScriptMessageWriter();
		private readonly UnicodeEncoding _stringcoder = new UnicodeEncoding();
		private readonly IMessageQueue _messageQueue;
		private readonly byte[] _messageBuffer;
		private int _insertItemID;
		private List<Dialog> _dialogStack = new List<Dialog>();
		private static int DrawLayer = 15000;
		public event UIEventGameEvent GameEvent;
		public event UIEventPanelMessage PanelMessage;
		public event UIEventUpdate UpdateEvent;
		public UICommChannel(IMessageQueue messageQueue)
		{
			this._messageQueue = messageQueue;
			this._messageBuffer = new byte[this._messageQueue.IncomingCapacity];
		}
		public void Update()
		{
			new List<string>();
			this._messageQueue.Update();
			while (true)
			{
				int nextMessage = this._messageQueue.GetNextMessage(this._messageBuffer);
				if (nextMessage == 0)
				{
					break;
				}
				this.ProcessEngineMessage(this._stringcoder.GetString(this._messageBuffer, 0, nextMessage));
			}
			if (this.UpdateEvent != null)
			{
				this.UpdateEvent();
			}
		}
		public string Path(params string[] panelNames)
		{
			string text = string.Empty;
			for (int i = 0; i < panelNames.Length; i++)
			{
				if (i != 0 && panelNames[i - 1].Length > 0)
				{
					text += ".";
				}
				text += panelNames[i];
			}
			return text;
		}
		public Dialog GetTopDialog()
		{
			if (this._dialogStack.Count<Dialog>() == 0)
			{
				return null;
			}
			return this._dialogStack.Last<Dialog>();
		}
		public void HandleDialogMessage(ScriptMessageReader mr)
		{
			string a = mr.ReadString();
			foreach (Dialog current in this._dialogStack)
			{
				if (a == current.ID)
				{
					current.HandleScriptMessage(mr);
					break;
				}
			}
		}
		public string CreateDialog(Dialog dialog, string parentPanel = null)
		{
			this.CreatePanelFromTemplate(dialog.Template, dialog.ID);
			if (parentPanel != null)
			{
				this.SetParent(dialog.ID, parentPanel);
			}
			else
			{
				this.ParentToMainPanel(dialog.ID);
			}
			this.SetDrawLayer(dialog.ID, UICommChannel.DrawLayer++);
			this.Send(new object[]
			{
				"PushFocus",
				dialog.ID
			});
			this._dialogStack.Add(dialog);
			dialog.Initialize();
			return dialog.ID;
		}
		public void CloseDialog(Dialog dialog, bool dispose = true)
		{
			if (!this._dialogStack.Contains(dialog))
			{
				return;
			}
			this._dialogStack.Remove(dialog);
			this.Send(new object[]
			{
				"PopFocus",
				dialog.ID
			});
			this.PanelMessage(dialog.ID, "dialog_closed", dialog.CloseDialog());
			this.DestroyPanel(dialog.ID);
			if (dispose)
			{
				dialog.Dispose();
			}
		}
		public string CreateOverlay(Dialog dialog, string parentPanel = null)
		{
			this.CreatePanelFromTemplate(dialog.Template, dialog.ID);
			if (parentPanel != null)
			{
				this.SetParent(dialog.ID, parentPanel);
			}
			else
			{
				this.ParentToMainPanel(dialog.ID);
			}
			this.SetDrawLayer(dialog.ID, UICommChannel.DrawLayer++);
			dialog.Initialize();
			return dialog.ID;
		}
		public void ShowOverlay(Dialog overlay)
		{
			this._dialogStack.Add(overlay);
		}
		public void HideOverlay(Dialog overlay)
		{
			this._dialogStack.Remove(overlay);
		}
		public string FormatColor(Vector3 color)
		{
			return string.Format("{0},{1},{2}", color.X * 255f, color.Y * 255f, color.Z * 255f);
		}
		public bool ParseListItemId(string msgParam, out int id)
		{
			id = 0;
			if (string.IsNullOrEmpty(msgParam))
			{
				return false;
			}
			id = int.Parse(msgParam);
			return true;
		}
		public void SetPostMouseOverEvents(string panelId, bool value)
		{
			this.Send(new object[]
			{
				"SetPostMouseOverEvents",
				panelId,
				value
			});
		}
		public void ParentToMainPanel(string panelId)
		{
			this.Send(new object[]
			{
				"ParentToMainPanel",
				panelId
			});
		}
		public void SetDrawLayer(string panelId, int value)
		{
			this.Send(new object[]
			{
				"SetDrawLayer",
				panelId,
				value
			});
		}
		public void MovePanelToMouse(string panelId, UICommChannel.AnchorPoint anchorPoint, Vector2 positionOffset)
		{
			this.Send(new object[]
			{
				"MoveToMouse",
				panelId,
				anchorPoint.ToString(),
				positionOffset.X,
				positionOffset.Y
			});
		}
		public void SetVisible(string panelId, bool value)
		{
			this.Send(new object[]
			{
				"SetVisible",
				panelId,
				value
			});
		}
		public void SetEnabled(string panelId, bool value)
		{
			this.Send(new object[]
			{
				"SetEnabled",
				panelId,
				value
			});
		}
		public void SetShape(string panelId, int left, int top, int width, int height)
		{
			this.Send(new object[]
			{
				"SetShape",
				panelId,
				left,
				top,
				width,
				height
			});
		}
		public void SetShapeToPanel(string panelId, string shapePanel)
		{
			this.Send(new object[]
			{
				"SetShapeToPanel",
				panelId,
				shapePanel
			});
		}
		public void SetParent(string panelId, string parent)
		{
			this.Send(new object[]
			{
				"SetParent",
				panelId,
				parent
			});
		}
		public void SetPosition(string panelId, int x, int y)
		{
			this.Send(new object[]
			{
				"SetPosition",
				panelId,
				x,
				y
			});
		}
		public void ForceLayout(string panelId)
		{
			this.Send(new object[]
			{
				"ForceLayout",
				panelId
			});
		}
		public void ShakeViolently(string panelId)
		{
			this.Send(new object[]
			{
				"ForceLayout",
				panelId
			});
			this.Send(new object[]
			{
				"ForceLayout",
				panelId
			});
		}
		public void SetTooltip(string panelId, string text)
		{
			this.Send(new object[]
			{
				"SetTooltip",
				panelId,
				text
			});
		}
		public void SetPropertyString(string panelId, string propertyName, string propertyValue)
		{
			this.Send(new object[]
			{
				"SetPropString",
				panelId,
				propertyName,
				propertyValue
			});
		}
		public void SetPropertyInt(string panelId, string propertyName, int propertyValue)
		{
			this.Send(new object[]
			{
				"SetPropInt",
				panelId,
				propertyName,
				propertyValue
			});
		}
		public void SetPropertyFloat(string panelId, string propertyName, float propertyValue)
		{
			this.Send(new object[]
			{
				"SetPropFloat",
				panelId,
				propertyName,
				propertyValue
			});
		}
		public void SetPropertyBool(string panelId, string propertyName, bool propertyValue)
		{
			this.Send(new object[]
			{
				"SetPropBool",
				panelId,
				propertyName,
				propertyValue
			});
		}
		public void SetPropertyColor(string panelId, string propertyName, float r, float g, float b)
		{
			this.Send(new object[]
			{
				"SetPropColor3",
				panelId,
				propertyName,
				r / 255f,
				g / 255f,
				b / 255f
			});
		}
		public void SetPropertyColorNormalized(string panelId, string propertyName, float r, float g, float b)
		{
			this.Send(new object[]
			{
				"SetPropColor3",
				panelId,
				propertyName,
				r,
				g,
				b
			});
		}
		public void SetPropertyColor(string panelId, string propertyName, float r, float g, float b, float a)
		{
			this.Send(new object[]
			{
				"SetPropColor4",
				panelId,
				propertyName,
				r / 255f,
				g / 255f,
				b / 255f,
				a / 255f
			});
		}
		public void SetPropertyColorNormalized(string panelId, string propertyName, float r, float g, float b, float a)
		{
			this.Send(new object[]
			{
				"SetPropColor4",
				panelId,
				propertyName,
				r,
				g,
				b,
				a
			});
		}
		public void SetPropertyColor(string panelId, string propertyName, Vector3 value)
		{
			this.SetPropertyColor(panelId, propertyName, value.X, value.Y, value.Z);
		}
		public void SetPropertyColorNormalized(string panelId, string propertyName, Vector3 value)
		{
			this.SetPropertyColorNormalized(panelId, propertyName, value.X, value.Y, value.Z);
		}
		public void SetPropertyColorNormalized(string panelId, string propertyName, Vector4 value)
		{
			this.SetPropertyColorNormalized(panelId, propertyName, value.X, value.Y, value.Z, value.W);
		}
		public void SetPropertyColor(string panelId, string propertyName, Vector4 value)
		{
			this.SetPropertyColor(panelId, propertyName, value.X, value.Y, value.Z, value.W);
		}
		public void SetChecked(string panelId, bool isChecked)
		{
			this.SetPropertyBool(panelId, "checked", isChecked);
		}
		public void SetSliderValue(string panelId, int value)
		{
			this.SetPropertyInt(panelId, "value", value);
		}
		public void SetSliderRange(string panelId, int min, int max)
		{
			this.SetPropertyInt(panelId, "value_min", min);
			this.SetPropertyInt(panelId, "value_max", max);
		}
		public void InitializeSlider(string panelId, int minValue, int maxValue, int value)
		{
			this.SetPropertyInt(panelId, "value_min", minValue);
			this.SetPropertyInt(panelId, "value_max", maxValue);
			this.SetSliderValue(panelId, value);
		}
		public void AddSliderNotch(string panelId, int value)
		{
			this.SetPropertyInt(panelId, "add_notch", value);
		}
		public void RemoveSliderNotch(string panelId, int value)
		{
			this.SetPropertyInt(panelId, "rem_notch", value);
		}
		public void SetSliderTolerance(string panelId, int value)
		{
			this.SetPropertyInt(panelId, "notch_tolerance", value);
		}
		public void SetSliderAutoSnap(string panelId, bool value)
		{
			this.SetPropertyBool(panelId, "auto_notch_snap", value);
		}
		public void ClearSliderNotches(string panelId)
		{
			this.SetPropertyBool(panelId, "clear_notchs", true);
		}
		public void AutoSize(string panelId)
		{
			this.Send(new object[]
			{
				"AutoSize",
				panelId
			});
		}
		public void SetListCleanClear(string listPanelId, bool value)
		{
			this.Send(new object[]
			{
				"SetListCleanClear",
				listPanelId,
				value
			});
		}
		public void SetExpanded(string panelId, bool expanded)
		{
			this.Send(new List<object>
			{
				"SetExpanded",
				panelId,
				expanded
			}.ToArray());
		}
		public void Reshape(string panelId)
		{
			this.Send(new List<object>
			{
				"Reshape",
				panelId
			}.ToArray());
		}
		public void AutoSizeContents(string panelId)
		{
			this.Send(new object[]
			{
				"AutoSizeContents",
				panelId
			});
		}
		public void ClearItems(string listId)
		{
			this.Send(new object[]
			{
				"ClearItems",
				listId
			});
		}
		public void ClearItemsTopLayer(string listId)
		{
			this.Send(new object[]
			{
				"ClearItemsTop",
				listId
			});
		}
		public void RemoveItems(string listId, int userItemId)
		{
			this.Send(new object[]
			{
				"DelItem",
				listId,
				1,
				userItemId
			});
		}
		public void RemoveItems(string listId, IEnumerable<int> userItemIds)
		{
			if (userItemIds.Any<int>())
			{
				List<object> list = new List<object>();
				list.Add("DelItem");
				list.Add(listId);
				list.Add(userItemIds.Count<int>());
				list.AddRange(userItemIds.Cast<object>());
				this.SendElements(list);
			}
		}
		public void AddItem(string listId, string fieldId, int userItemId, string text = "")
		{
			this.Send(new object[]
			{
				"AddItem",
				listId,
				fieldId,
				userItemId,
				text,
				""
			});
		}
		public void AddItem(string listId, string fieldId, int userItemId, string text, string panelTemplate)
		{
			this.Send(new object[]
			{
				"AddItem",
				listId,
				fieldId,
				userItemId,
				text,
				panelTemplate
			});
		}
		public void AddSpacer(string listId)
		{
			this.Send(new object[]
			{
				"AddItem",
				listId,
				"",
				-1,
				"",
				"use_spacer_template"
			});
		}
		public string GetItemGlobalID(string listId, string fieldId, int userItemId, string text = "")
		{
			this._insertItemID++;
			if (this._insertItemID < 0)
			{
				this._insertItemID = 1;
			}
			this.SetItemPropertyInt(listId, fieldId, userItemId, text, "globalid", this._insertItemID);
			return "&" + this._insertItemID.ToString();
		}
		public string GetGlobalID(string panelId)
		{
			this._insertItemID++;
			if (this._insertItemID < 0)
			{
				this._insertItemID = 1;
			}
			this.SetPropertyInt(panelId, "globalid", this._insertItemID);
			return "&" + this._insertItemID.ToString();
		}
		public void SetItemText(string listId, string fieldId, int userItemId, string text)
		{
			this.SetItemPropertyString(listId, fieldId, userItemId, string.Empty, "text", text);
		}
		public void SetItemPropertyString(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, string value)
		{
			this.Send(new object[]
			{
				"SetItemPropString",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				value
			});
		}
		public void SetItemPropertyInt(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, int value)
		{
			this.Send(new object[]
			{
				"SetItemPropInt",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				value
			});
		}
		public void SetItemPropertyFloat(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, float value)
		{
			this.Send(new object[]
			{
				"SetItemPropFloat",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				value
			});
		}
		public void SetItemPropertyBool(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, bool value)
		{
			this.Send(new object[]
			{
				"SetItemPropBool",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				value
			});
		}
		public void SetItemPropertyColor(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, float x, float y, float z)
		{
			this.Send(new object[]
			{
				"SetItemPropColor3",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				x / 255f,
				y / 255f,
				z / 255f
			});
		}
		public void SetItemPropertyColor(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, float x, float y, float z, float w)
		{
			this.Send(new object[]
			{
				"SetItemPropColor4",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				x / 255f,
				y / 255f,
				z / 255f,
				w / 255f
			});
		}
		public void SetItemPropertyColor(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, Vector3 color)
		{
			this.Send(new object[]
			{
				"SetItemPropColor3",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				color.X / 255f,
				color.Y / 255f,
				color.Z / 255f
			});
		}
		public void SetItemPropertyColor(string listId, string fieldId, int userItemId, string subPanelId, string propertyName, Vector4 color)
		{
			this.Send(new object[]
			{
				"SetItemPropColor4",
				listId,
				fieldId,
				userItemId,
				subPanelId,
				propertyName,
				color.X / 255f,
				color.Y / 255f,
				color.Z / 255f,
				color.W / 255f
			});
		}
		public void SetSelection(string listId, int userItemId)
		{
			this.Send(new object[]
			{
				"SetSel",
				listId,
				1,
				userItemId
			});
		}
		public void ClearSelection(string listId)
		{
			this.Send(new object[]
			{
				"ClearSel",
				listId
			});
		}
		public void SetSelection(string listId, IEnumerable<int> userItemIds)
		{
			if (userItemIds.Any<int>())
			{
				List<object> list = new List<object>();
				list.Add("SetSel");
				list.Add(listId);
				list.Add(userItemIds.Count<int>());
				list.AddRange(userItemIds.Cast<object>());
				this.SendElements(list);
				return;
			}
			this.ClearSelection(listId);
		}
		public void ClearDisabledItems(string listId)
		{
			this.Send(new object[]
			{
				"ClearDisabled",
				listId
			});
		}
		public void SetDisabledItems(string listId, IEnumerable<int> userItemIds)
		{
			if (userItemIds.Any<int>())
			{
				List<object> list = new List<object>();
				list.Add("SetDisabled");
				list.Add(listId);
				list.Add(userItemIds.Count<int>());
				list.AddRange(userItemIds.Cast<object>());
				this.SendElements(list);
				return;
			}
			this.ClearDisabledItems(listId);
		}
		public void SetButtonText(string panelId, string text)
		{
			this.Send(new object[]
			{
				"SetText",
				panelId + ".idle.menulabel",
				text
			});
			this.Send(new object[]
			{
				"SetText",
				panelId + ".mouse_over.menulabel",
				text
			});
			this.Send(new object[]
			{
				"SetText",
				panelId + ".pressed.menulabel",
				text
			});
			this.Send(new object[]
			{
				"SetText",
				panelId + ".disabled.menulabel",
				text
			});
		}
		public void SetText(string panelId, string text)
		{
			this.Send(new object[]
			{
				"SetText",
				panelId,
				text
			});
		}
		public void LocalizeText(string panelId, string text)
		{
			this.Send(new object[]
			{
				"LocalizeText",
				panelId,
				text
			});
		}
		public void SetTextFile(string panelId, string file)
		{
			this.SetPropertyString(panelId, "text_file", file);
		}
		public void SetScreen(string screenId)
		{
			this.Send(new object[]
			{
				"SetScreen",
				screenId
			});
		}
		public void LoadScreen(string screenId)
		{
			this.Send(new object[]
			{
				"LoadScreen",
				screenId
			});
		}
		public void DeleteScreen(string screenId)
		{
			this.Send(new object[]
			{
				"DeleteScreen",
				screenId
			});
		}
		public void PurgeFleetWidgetCache()
		{
			this.Send(new object[]
			{
				"PurgeFleetWidgetCache"
			});
		}
		public void LockUI()
		{
			this.Send(new object[]
			{
				"LockUI"
			});
		}
		public void UnlockUI()
		{
			this.Send(new object[]
			{
				"UnlockUI"
			});
		}
		public string CreatePanelFromTemplate(string templateId, string id = null)
		{
			if (id == null)
			{
				id = Guid.NewGuid().ToString();
			}
			this.Send(new object[]
			{
				"CreatePanelFromTemplate",
				id,
				templateId
			});
			return id;
		}
		public void DestroyPanel(string id)
		{
			this.Send(new object[]
			{
				"DestroyPanel",
				id
			});
		}
		public void ShowTooltip(string id, float x, float y)
		{
			this.Send(new object[]
			{
				"ShowTooltip",
				id,
				x,
				y
			});
		}
		public void HideTooltip()
		{
			this.Send(new object[]
			{
				"HideTooltip"
			});
		}
		private void ProcessEngineMessage(string engMsg)
		{
			string[] array = engMsg.Split(new char[]
			{
				','
			});
			if (array.Length > 0)
			{
				if (array[0] == "GameEvent" && this.ProcessGameEventMessage(array))
				{
					return;
				}
				if (array[0] == "Panel" && this.ProcessPanelMessage(array))
				{
					return;
				}
			}
			this.Msg("UICC ProcessEngineMessages {" + engMsg + "} what is this?");
		}
		public void Send(params object[] elements)
		{
			this.SendElements(elements);
		}
		public void SendElements(IEnumerable elements)
		{
			this._scriptMessageWriter.Clear();
			this._scriptMessageWriter.Write(elements);
			this._messageQueue.PutMessage(this._scriptMessageWriter.GetBuffer(), (int)this._scriptMessageWriter.GetSize());
		}
		private string[] SeparateParams(string[] subStrings, int firstParamIndex)
		{
			string[] array = new string[subStrings.Length - firstParamIndex];
			for (int i = firstParamIndex; i < subStrings.Length; i++)
			{
				array[i - firstParamIndex] = subStrings[i];
			}
			return array;
		}
		private bool ProcessGameEventMessage(string[] subStrings)
		{
			string eventName = subStrings[1];
			string[] eventParams = this.SeparateParams(subStrings, 2);
			if (this.GameEvent != null)
			{
				this.GameEvent(eventName, eventParams);
			}
			return true;
		}
		private bool ProcessPanelMessage(string[] subStrings)
		{
			if (subStrings.Length >= 3)
			{
				string panelName = subStrings[1];
				string msgType = subStrings[2];
				string[] msgParams = this.SeparateParams(subStrings, 3);
				if (this.PanelMessage != null)
				{
					this.PanelMessage(panelName, msgType, msgParams);
				}
				return true;
			}
			return false;
		}
		private void Msg(string message)
		{
			if (UICommChannel.LogEnable)
			{
				App.Log.Trace(message, "gui");
			}
		}
	}
}
