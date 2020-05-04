using UnityEngine;

namespace Extend.LuaBindingEvent.AnimationEvent {
	[CreateAssetMenu(fileName = "EventInstance", menuName = "Animation Event/Event Instance", order = 1)]
	public class EventInstance : ScriptableObject {
		public string EventName;
		public EventParam Param;
	}
}