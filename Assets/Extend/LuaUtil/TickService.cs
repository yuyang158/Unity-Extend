using System;
using Extend.Common;
using Extend.DebugUtil;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public class TickService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.TICK_SERVICE;
		[CSharpCallLua]
		private Action<float> m_tick;
		
		public void Initialize() {
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var luaGetService = luaVM.Global.GetInPath<GetLuaService>( "_ServiceManager.GetService" );
			var luaTickService = luaGetService( 2 );
			m_tick = luaTickService.Get<Action<float>>( "Tick" );
		}
 
		public void Destroy() {
		}

		public void Update() {
			m_tick( Time.deltaTime );
		}
	}
}