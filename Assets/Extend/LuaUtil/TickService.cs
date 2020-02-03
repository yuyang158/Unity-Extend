using Extend.Common;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public class TickService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.TICK_SERVICE;
		private LuaFunction tick;
		
		public void Initialize() {
			var luaGetService = LuaVM.Default.Global.GetInPath<LuaFunction>( "_ServiceManager.GetService" );
			var luaTickService = luaGetService.Call( 2 )[0] as LuaTable;
			tick = luaTickService.Get<LuaFunction>( "Tick" );
		}
 
		public void Destroy() {
		}

		public void Update() {
			tick.Call( Time.deltaTime );
		}
	}
}