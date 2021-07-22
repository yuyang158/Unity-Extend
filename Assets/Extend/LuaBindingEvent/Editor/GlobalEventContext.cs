using System;
using Extend.Common;
using Extend.EventAsset;
using UnityEngine;

namespace Extend.LuaBindingEvent.Editor {
	[CreateAssetMenu(fileName = "GlobalEventList", menuName = "Event/Editor Global Event List", order = 1)]
	public class GlobalEventContext : ScriptableObject {
		[Serializable]
		public class Context {
			public EventInstance Event;
			public string Description;
		}

		[ReorderList]
		public Context[] Contexts;
	}
}