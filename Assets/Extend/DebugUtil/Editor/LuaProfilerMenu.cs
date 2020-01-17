using Extend.Common;
using UnityEditor;
using UnityEditor.PackageManager;

namespace Extend.DebugUtil.Editor {
	internal static class LuaProfilerMenu {
		[MenuItem("XLua/Attach Lua Profiler")]
		private static void _ExecuteAttach() {
			if( _isAttached ) {
				return;
			}

			_isAttached = true;

			const string script = @"
			local LuaProfiler = CS.Extend.DebugUtil.LuaProfiler
			local debug = debug
			local _cache = {}
			local _id_generator = 0
			local _ignore_count = 0
			local function lua_profiler_hook (event, line)
			    if event == 'call' then
			        local func = debug.getinfo (2, 'f').func
			        local id = _cache[func]
			        if id then
			            LuaProfiler.BeginSample (id)
			        else
			            local ar = debug.getinfo (2, 'Sn')
			            local method_name = ar.name
			            local linedefined = ar.linedefined
			            if linedefined ~= -1 or (method_name and method_name ~= '__index')  then
			                local short_src = ar.short_src
			                method_name = method_name or '[unknown]'
			                local index = short_src:match ('^.*()[/\\]')
			                local filename  = index and short_src:sub (index + 1) or short_src
			                local show_name = filename .. ':' .. method_name .. ' '.. linedefined
			                local id = _id_generator + 1
			                _id_generator = id
			                _cache[func] = id
			                LuaProfiler.BeginSample (id, show_name)
			            else
			                _ignore_count = _ignore_count + 1
			            end
			        end
			    elseif event == 'return' then
			        if _ignore_count == 0 then
			            LuaProfiler.EndSample ()
			        else
			            _ignore_count = _ignore_count - 1
			        end
			    end
			end
			debug.sethook (lua_profiler_hook, 'cr', 0)";

			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			luaVM.Default.DoString(script, "@LoadLuaProfiler");
		}

		[MenuItem("XLua/Detach Lua Profiler")]
		private static void _ExecuteDetach() {
			if( !_isAttached ) {
				return;
			}

			_isAttached = false;
			const string script = "debug.sethook (nil)";
			var luaVM = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE);
			luaVM.Default.DoString(script, "@UnloadLuaProfiler");
		}

		private static bool _isAttached;
	}
}