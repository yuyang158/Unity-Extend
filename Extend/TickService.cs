using System;
using Common;
using UnityEngine;
using XLua;

namespace Extend {
	[CSharpCallLua]
	public class TickService : IService, IServiceUpdate {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.TICK_SERVICE;

		private Action<float> tick;
		
		public void Initialize() {
			var luaGetService = LuaVM.Default.Global.GetInPath<LuaFunction>( "_ServiceManager.GetService" );
			var luaTickService = luaGetService.Call( 2 )[0] as LuaTable;
			tick = luaTickService.Get<Action<float>>( "Tick" );
		}
 
		public void Destroy() {
		}

		public void Update() {
			tick( Time.deltaTime );
		}
	}
}