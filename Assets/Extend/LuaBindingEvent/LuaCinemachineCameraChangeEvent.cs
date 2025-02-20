using Cinemachine;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaBindingEvent {
	public class LuaCinemachineCameraChangeEvent : MonoBehaviour {
		public void OnCinemachineCameraChange(ICinemachineCamera newCamera, ICinemachineCamera oldCamera) {
			var luaVm = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			luaVm.Global.GetInPath<LuaFunction>("__CINEMACHINE_CAMERA_CHANGED__").Action(newCamera.VirtualCameraGameObject, gameObject);
		}
	}
}