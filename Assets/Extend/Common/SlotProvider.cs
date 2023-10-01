using System;
using UnityEngine;
using XLua;

namespace Extend.Common {
	[LuaCallCSharp]
	public class SlotProvider : MonoBehaviour {
		[Serializable]
		public class Slot {
			public string SlotName;
			public Transform Bone;
		}

		[SerializeField]
		private Slot[] _slots;

		public Transform GetSlot(string slotName) {
			foreach( Slot slot in _slots ) {
				if( slot.SlotName == slotName ) {
					return slot.Bone;
				}
			}

			return null;
		}
	}
}