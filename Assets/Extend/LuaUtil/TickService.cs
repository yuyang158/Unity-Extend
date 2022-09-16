using System;
using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public class TickService : IService, IServiceUpdate, IServiceLateUpdate {
		public int ServiceType => (int)CSharpServiceManager.ServiceType.TICK_SERVICE;
		[CSharpCallLua]
		private Action m_tick;
		[CSharpCallLua]
		private Action m_lateTick;
		
		public void Initialize()
		{
			Restart();
		}

		public void Restart()
		{
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var luaGetService = luaVM.Global.GetInPath<GetLuaService>( "_ServiceManager.GetService" );
			var luaTickService = luaGetService( 2 );
			m_tick = luaTickService.Get<Action>( "Tick" );
			m_lateTick = luaTickService.Get<Action>( "LateTick" );
		}
 
		public void Destroy() {
		}

		public void Update() {
			m_tick();
		}

		public void LateUpdate() {
			m_lateTick();
		}
	}
}