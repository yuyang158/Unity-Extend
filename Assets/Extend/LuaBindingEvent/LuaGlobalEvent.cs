using System.Collections.Generic;
using Extend.Asset;
using Extend.Asset.Attribute;
using Extend.EventAsset;
using Extend.LuaUtil;
using UnityEngine;
using XLua;

namespace Extend.LuaBindingEvent {
	[CSharpCallLua]
	public class LuaGlobalEvent : MonoBehaviour {
		private static readonly Dictionary<EventInstance, LuaEventCallback> m_eventCallbacks = new();
		public static void Register(EventInstance e, LuaEventCallback callback) {
			m_eventCallbacks.Add(e, callback);
		}

		public static void Trigger(EventInstance e) {
			if(!m_eventCallbacks.TryGetValue(e, out var cb)) {
				return;
			}

			cb.Invoke(e);
		}

		[AssetReferenceAssetType(AssetType = typeof(EventInstance)), BlackList]
		public AssetReference Event;

		[BlackList]
		public void Dispatch() {
			Trigger(Event.GetScriptableObject<EventInstance>());
		}
		
		private void OnDestroy() {
			Event.Dispose();
		}
	}
}