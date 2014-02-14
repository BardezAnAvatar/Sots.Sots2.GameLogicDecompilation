using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Kerberos.Sots.Engine
{
	internal static class CommonMessageExtensions
	{
		public static void PostReport(this App state, string text)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_GAME_REPORT_EVENT,
				text
			});
		}
		public static void PostNewGame(this App state, int playerId)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_NEW_GAME,
				playerId
			});
		}
		public static void PostSetLocalPlayer(this App state, int playerId)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_SET_LOCAL_PLAYER,
				playerId
			});
		}
		public static void PostRequestSpeech(this App state, string cueName, int priority, int duration, float timeout)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_REQUEST_SPEECH,
				cueName,
				priority,
				duration,
				timeout
			});
		}
		public static void PostRequestEffectSound(this App state, string cueName)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_REQUEST_EFFECT_SOUND,
				cueName
			});
		}
		public static void PostRequestGuiSound(this App state, string cueName)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_REQUEST_GUI_SOUND,
				cueName
			});
		}
		public static void PostEnableEffectsSounds(this App state, bool enabled)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_EFFECTS,
				enabled
			});
		}
		public static void PostEnableGuiSounds(this App state, bool enabled)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_GUI,
				enabled
			});
		}
		public static void PostEnableSpeechSounds(this App state, bool enabled)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_SPEECH,
				enabled
			});
		}
		public static void PostEnableMusicSounds(this App state, bool enabled)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_MUSIC,
				enabled
			});
		}
		public static void PostDisableAllSounds(this App state)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_EFFECTS,
				false
			});
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_GUI,
				false
			});
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_SPEECH,
				false
			});
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_MUSIC,
				false
			});
		}
		public static void PostEnableAllSounds(this App state)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_EFFECTS,
				true
			});
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_GUI,
				true
			});
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_SPEECH,
				true
			});
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE_MUSIC,
				true
			});
		}
		public static void TurnOffSound(this App state)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE,
				false
			});
		}
		public static void TurnOnSound(this App state)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_ENABLE,
				true
			});
		}
		public static void PostSpeechSubtitles(this App state, bool value)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_SPEECH_SUBTITLES,
				value
			});
		}
		public static void PostSetVolumeMusic(this App state, int volume)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_VOLUME_MUSIC,
				volume
			});
		}
		public static void PostSetVolumeEffects(this App state, int volume)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_VOLUME_EFFECTS,
				volume
			});
		}
		public static void PostSetVolumeSpeech(this App state, int volume)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_SET_VOLUME_SPEECH,
				volume
			});
		}
		public static void PostRequestStopSounds(this App state)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_REQUEST_STOP_SOUNDS
			});
		}
		public static void PostRequestStopSound(this App state, string cueName)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_REQUEST_STOP_SOUND,
				cueName
			});
		}
		public static void PostPlayMusic(this App state, string cueName)
		{
			state.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_SOUND_PLAY_MUSIC,
				cueName
			});
		}
		public static void PostAddGoal(this IGameObject state, Vector3 targetPos, Vector3 look)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_ADD_GOAL,
				state.ObjectID,
				targetPos.X,
				targetPos.Y,
				targetPos.Z,
				look.X,
				look.Y,
				look.Z
			});
		}
		public static void PostSetLook(this IGameObject state, Vector3 look)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETLOOK,
				state.ObjectID,
				look.X,
				look.Y,
				look.Z
			});
		}
		public static void PostSetAggregate(this IGameObject state, IGameObject target)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETAGGREGATE,
				state.ObjectID,
				target.ObjectID
			});
		}
		public static void PostObjectAddObjects(this IGameObject state, params IGameObject[] objects)
		{
			if (objects == null || objects.Length == 0)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(InteropMessageID.IMID_ENGINE_OBJECT_ADDOBJECTS);
			list.Add(state.ObjectID);
			list.Add(objects.Length);
			list.AddRange((
				from x in objects
				select x.ObjectID).Cast<object>());
			state.App.PostEngineMessage(list);
		}
		public static void PostAttach(this IGameObject state, IGameObject target)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_ATTACH,
				state.ObjectID,
				1,
				target.ObjectID
			});
		}
		public static void PostAttach(this IGameObject state, IGameObject paired, IGameObject target, IGameObject socket1, string socket1NodeName, IGameObject socket2, string socket2NodeName)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_ATTACH,
				state.ObjectID,
				6,
				target.ObjectID,
				paired.ObjectID,
				socket1.ObjectID,
				socket1NodeName ?? string.Empty,
				socket2.ObjectID,
				socket2NodeName ?? string.Empty
			});
		}
		public static void PostDetach(this IGameObject state, IGameObject target)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_DETACH,
				state.ObjectID,
				target.ObjectID
			});
		}
		public static void PostSetParent(this IGameObject state, IGameObject parent)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPARENT,
				state.ObjectID,
				1,
				parent.ObjectID
			});
		}
		public static void PostSetParent(this IGameObject state, IGameObject parent, string parentNodeName)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPARENT,
				state.ObjectID,
				2,
				parent.ObjectID,
				parentNodeName
			});
		}
		public static void PostSetParent(this IGameObject state, IGameObject parent, string parentNodeName, string offsetNodeName)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPARENT,
				state.ObjectID,
				3,
				parent.ObjectID,
				parentNodeName,
				offsetNodeName
			});
		}
		public static void PostSetBattleRiderParent(this IGameObject state, int parentID)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_BATTLERIDER_SETPARENT,
				state.ObjectID,
				parentID
			});
		}
		public static void PostSetActive(this IGameObject state, bool value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETACTIVE,
				state.ObjectID,
				value ? 1 : 0
			});
		}
		public static void PostSetPosition(this IGameObject state, Vector3 value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPOS,
				state.ObjectID,
				value.X,
				value.Y,
				value.Z
			});
		}
		public static void PostSetRotation(this IGameObject state, Vector3 value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETROT,
				state.ObjectID,
				value.X,
				value.Y,
				value.Z
			});
		}
		public static void PostSetScale(this IGameObject state, float value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETSCALE,
				state.ObjectID,
				value
			});
		}
		public static void PostSetPlayer(this IGameObject state, int playerId)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPLAYER,
				state.ObjectID,
				playerId
			});
		}
		public static void PostNotifyObjectHasBeenAdded(this IGameObject state, int objectID)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_ADDED,
				state.ObjectID,
				objectID
			});
		}
		public static void PostNotifyObjectsHaveBeenAdded(this IGameObject state, int[] objectIDs)
		{
			List<object> list = new List<object>();
			list.Add(InteropMessageID.IMID_ENGINE_OBJECTS_ADDED);
			list.Add(state.ObjectID);
			list.Add(objectIDs.Length);
			for (int i = 0; i < objectIDs.Length; i++)
			{
				int num = objectIDs[i];
				list.Add(num);
			}
			state.App.PostEngineMessage(list.ToArray());
		}
		public static void PostSetProp(this IGameObject state, string propertyName, Vector2 value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				value.X,
				value.Y
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, Vector3 value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				value.X,
				value.Y,
				value.Z
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, Matrix value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				value.M11,
				value.M12,
				value.M13,
				value.M14,
				value.M21,
				value.M22,
				value.M23,
				value.M24,
				value.M31,
				value.M32,
				value.M33,
				value.M34,
				value.M41,
				value.M42,
				value.M43,
				value.M44
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, float value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				value
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, string value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				value
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, int value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				value
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, bool value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				value
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, IGameObject value)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName,
				(value != null) ? value.ObjectID : 0
			});
		}
		public static void PostSetProp(this IGameObject state, string propertyName, IGameObject[] value)
		{
			List<object> list = new List<object>();
			list.Add(InteropMessageID.IMID_ENGINE_OBJECT_SETPROP);
			list.Add(state.ObjectID);
			list.Add(propertyName);
			if (value != null)
			{
				list.Add(value.Length);
				for (int i = 0; i < value.Length; i++)
				{
					list.Add((value[i] != null) ? value[i].ObjectID : 0);
				}
			}
			state.App.PostEngineMessage(list);
		}
		public static void PostSetProp(this IGameObject state, string propertyName, params object[] values)
		{
			object[] elements = new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
				state.ObjectID,
				propertyName
			}.Concat(values).ToArray<object>();
			state.App.PostEngineMessage(elements);
		}
		public static void PostSetInt(this IGameObject state, int property, params object[] values)
		{
			object[] elements = new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_SETINT,
				state.ObjectID,
				property
			}.Concat(values).ToArray<object>();
			state.App.PostEngineMessage(elements);
		}
		public static void PostFormationDefinition(this IGameObject state, Vector3 position, Vector3 facing, Vector3 dimensions)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_FORMATION_DEF,
				state.ObjectID,
				position.X,
				position.Y,
				position.Z,
				facing.X,
				facing.Y,
				facing.Z,
				dimensions.X,
				dimensions.Y,
				dimensions.Z
			});
		}
		public static void PostCreateFormationFromShips(this App game, params object[] msgParams)
		{
			object[] elements = new object[]
			{
				InteropMessageID.IMID_ENGINE_FORMATION_FROM_SHIPS
			}.Concat(msgParams).ToArray<object>();
			game.PostEngineMessage(elements);
		}
		public static void PostApplyFormationPattern(this App game, params object[] msgParams)
		{
			object[] elements = new object[]
			{
				InteropMessageID.IMID_ENGINE_FORMATION_APPLY_PATTERN
			}.Concat(msgParams).ToArray<object>();
			game.PostEngineMessage(elements);
		}
		public static void PostRemoveShipsFromFormation(this App game, params object[] msgParams)
		{
			object[] elements = new object[]
			{
				InteropMessageID.IMID_ENGINE_FORMATION_REMOVE_SHIPS
			}.Concat(msgParams).ToArray<object>();
			game.PostEngineMessage(elements);
		}
		public static void PostShipFormationPosition(this IGameObject state, IGameObject ship, Vector3 position)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_SHIP_FORMATION_POS,
				state.ObjectID,
				ship.ObjectID,
				position.X,
				position.Y,
				position.Z
			});
		}
		public static void PostFormationBattleRider(this IGameObject state, IGameObject ship, int parentID, Vector3 position)
		{
			state.App.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_FORMATION_BATTLE_RIDER,
				state.ObjectID,
				ship.ObjectID,
				parentID,
				position.X,
				position.Y,
				position.Z
			});
		}
		public static void PostNetworkMessage(this App game, string msgType, params object[] msgParams)
		{
			object[] elements = new object[]
			{
				InteropMessageID.IMID_ENGINE_NETWORK,
				msgType
			}.Concat(msgParams).ToArray<object>();
			game.PostEngineMessage(elements);
		}
		public static object GetTag(this IGameObject state)
		{
			return state.App.GetObjectTag(state);
		}
		public static T GetTag<T>(this IGameObject state) where T : class
		{
			return state.App.GetObjectTag(state) as T;
		}
		public static void SetTag(this IGameObject state, object value)
		{
			state.App.SetObjectTag(state, value);
		}
		public static void ClearTag(this IGameObject state)
		{
			state.App.RemoveObjectTag(state);
		}
	}
}
