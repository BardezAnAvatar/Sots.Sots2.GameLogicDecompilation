using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Kerberos.Sots.Engine
{
	internal class GameObjectMediator
	{
		private App _game;
		private int _prevGameObjectID;
		private readonly Dictionary<int, IGameObject> _pending = new Dictionary<int, IGameObject>();
		private readonly Dictionary<int, IGameObject> _objs = new Dictionary<int, IGameObject>();
		private readonly Dictionary<Type, InteropGameObjectType> _forwardTypeMap = new Dictionary<Type, InteropGameObjectType>();
		private readonly Dictionary<InteropGameObjectType, Type> _reverseTypeMap = new Dictionary<InteropGameObjectType, Type>();
		private readonly Dictionary<IGameObject, object> _objectTags = new Dictionary<IGameObject, object>();
		private int GetNextGameObjectID()
		{
			if (this._prevGameObjectID == 2147483647)
			{
				this._prevGameObjectID = 0;
			}
			this._prevGameObjectID++;
			return this._prevGameObjectID;
		}
		private static InteropGameObjectType? GetAssociatedGameObjectType(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(GameObjectTypeAttribute), true);
			if (customAttributes.Length > 0)
			{
				return new InteropGameObjectType?((customAttributes[0] as GameObjectTypeAttribute).Value);
			}
			return null;
		}
		public IGameObject GetObject(int id)
		{
			IGameObject result;
			if (this._objs.TryGetValue(id, out result))
			{
				return result;
			}
			return null;
		}
		public void SetObjectTag(IGameObject state, object value)
		{
			this._objectTags[state] = value;
		}
		public void RemoveObjectTag(IGameObject state)
		{
			if (this._objectTags.Keys.Contains(state))
			{
				this._objectTags[state] = null;
				this._objectTags.Remove(state);
			}
		}
		public object GetObjectTag(IGameObject state)
		{
			object result = null;
			this._objectTags.TryGetValue(state, out result);
			return result;
		}
		private void RegisterType(Type type)
		{
			InteropGameObjectType? associatedGameObjectType = GameObjectMediator.GetAssociatedGameObjectType(type);
			if (!associatedGameObjectType.HasValue)
			{
				throw new InvalidOperationException("No associated game object type for " + type + ".");
			}
			this._forwardTypeMap.Add(type, associatedGameObjectType.Value);
			this._reverseTypeMap.Add(associatedGameObjectType.Value, type);
		}
		private void RegisterTypes(params Type[] types)
		{
			for (int i = 0; i < types.Length; i++)
			{
				Type type = types[i];
				this.RegisterType(type);
			}
		}
		public GameObjectMediator(App game)
		{
			this._game = game;
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				Type type = types[i];
				if (GameObjectMediator.GetAssociatedGameObjectType(type).HasValue)
				{
					this.RegisterType(type);
				}
			}
		}
		private void AddExistingObjectCore(IGameObject o, InteropGameObjectType gameObjectType, params object[] initParams)
		{
			GameObject gameObject = (GameObject)o;
			gameObject.ObjectID = this.GetNextGameObjectID();
			gameObject.App = this._game;
			List<object> list = new List<object>(3);
			list.Add(InteropMessageID.IMID_ENGINE_OBJECT_ADD);
			list.Add(gameObject.ObjectID);
			list.Add(gameObjectType);
			if (initParams != null)
			{
				list.AddRange(initParams);
			}
			this._game.PostEngineMessage(list);
			this._pending.Add(gameObject.ObjectID, gameObject);
			this._objs.Add(gameObject.ObjectID, gameObject);
		}
		private IGameObject AddObjectCore(Type type, InteropGameObjectType gameObjectType, params object[] initParams)
		{
			IGameObject gameObject = (IGameObject)Activator.CreateInstance(type);
			this.AddExistingObjectCore(gameObject, gameObjectType, initParams);
			return gameObject;
		}
		public IGameObject AddObject(Type type, params object[] initParams)
		{
			return this.AddObjectCore(type, this._forwardTypeMap[type], initParams);
		}
		public void AddExistingObject(IGameObject o, params object[] initParams)
		{
			if (o.App != null)
			{
				throw new InvalidOperationException(string.Concat(new object[]
				{
					"Game object (",
					o.GetType(),
					", ",
					o.ObjectID,
					") is already in use."
				}));
			}
			this.AddExistingObjectCore(o, this._forwardTypeMap[o.GetType()], initParams);
		}
		public IGameObject AddObject(InteropGameObjectType gameObjectType, params object[] initParams)
		{
			return this.AddObjectCore(this._reverseTypeMap[gameObjectType], gameObjectType, initParams);
		}
		public void ReleaseObject(IGameObject obj)
		{
			this._pending.Remove(obj.ObjectID);
			this._objs.Remove(obj.ObjectID);
			this._game.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_RELEASE,
				obj.ObjectID
			});
		}
		public void ReleaseObjects(IEnumerable<IGameObject> range)
		{
			foreach (IGameObject current in range)
			{
				this._pending.Remove(current.ObjectID);
				this._objs.Remove(current.ObjectID);
			}
			this._game.PostEngineMessage(new object[]
			{
				InteropMessageID.IMID_ENGINE_OBJECT_RELEASEMULTI,
				range.Count<IGameObject>()
			}.Concat((
				from x in range
				select x.ObjectID).Cast<object>()));
		}
		public void OnObjectStatus(int objectID, GameObjectStatus objectStatus)
		{
			IGameObject gameObject;
			if (!this._pending.TryGetValue(objectID, out gameObject))
			{
				return;
			}
			(gameObject as GameObject).PromoteEngineObjectStatus(objectStatus);
			if (objectStatus != GameObjectStatus.Pending)
			{
				this._pending.Remove(objectID);
			}
		}
		public void OnObjectScriptMessage(InteropMessageID messageId, int objectId, ScriptMessageReader mr)
		{
			IGameObject gameObject;
			if (!this._objs.TryGetValue(objectId, out gameObject))
			{
				App.Log.Warn(string.Concat(new object[]
				{
					"Received message ",
					messageId.ToString(),
					" for nonexistant object ID = ",
					objectId
				}), "engine");
				return;
			}
			this._objs[objectId].OnEngineMessage(messageId, mr);
		}
	}
}
