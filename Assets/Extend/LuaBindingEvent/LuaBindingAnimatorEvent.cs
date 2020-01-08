using UnityEngine;
using XLua;

namespace Extend.LuaBindingEvent {
	public class LuaBindingAnimatorEvent : MonoBehaviour {
		public LuaBinding[] Bindings;
		
		public void OnEvent(string funcName) {
			foreach( var binding in Bindings ) {
				var func = binding.LuaInstance.GetInPath<LuaFunction>(funcName);
				func.Call();
			}
		}
	}
}