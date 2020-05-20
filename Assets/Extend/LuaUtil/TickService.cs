using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public class TickService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.TICK_SERVICE;
		private LuaFunction m_tick;
		
		public void Initialize() {
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			var luaGetService = luaVM.Global.GetInPath<LuaFunction>( "_ServiceManager.GetService" );
			var luaTickService = luaGetService.Call( 2 )[0] as LuaTable;
			m_tick = luaTickService.Get<LuaFunction>( "Tick" );
		}
 
		public void Destroy() {
			m_tick.Dispose();
		}

		public void Update() {
			m_tick.Call( Time.deltaTime );
		}
	}
}