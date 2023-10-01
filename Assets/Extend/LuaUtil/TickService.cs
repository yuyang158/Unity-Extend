using System;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public class TickService : IService, IServiceUpdate, IServiceLateUpdate {
		public int ServiceType => (int) CSharpServiceManager.ServiceType.TICK_SERVICE;

		[CSharpCallLua]
		private Action m_tick;

		[CSharpCallLua]
		private Action m_lateTick;

		public event Action<Resolution> ResolutionChanged;
		private Resolution m_currentResolution;

		public void Initialize() {
			Restart();
		}

		private void Restart() {
			m_currentResolution = Screen.currentResolution;
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var luaGetService = luaVM.Global.GetInPath<GetLuaService>("_ServiceManager.GetService");
			var luaTickService = luaGetService(2);
			m_tick = luaTickService.Get<Action>("Tick");
			m_lateTick = luaTickService.Get<Action>("LateTick");
		}

		public void Destroy() {
		}

		public void Update() {
			var resolution = Screen.currentResolution;
			
			// Resolution change notify
			if( m_currentResolution.width != resolution.width || m_currentResolution.height != resolution.height ) {
				m_currentResolution = Screen.currentResolution;
				ResolutionChanged?.Invoke(m_currentResolution);
			}
			
			m_tick();
		}

		public void LateUpdate() {
			m_lateTick();
		}
	}
}