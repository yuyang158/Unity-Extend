using System.Collections.Generic;
using XLua;

namespace Extend.LuaUtil {
	public class LuaClassCache {
		private class LuaClass {
			public LuaTable ClassMetaTable;
			public readonly List<LuaTable> ChildClasses = new List<LuaTable>();
		}

		private readonly Dictionary<LuaTable, LuaClass> m_cachedClasses = new Dictionary<LuaTable, LuaClass>(256);

		public void Register(LuaTable classMeta, LuaTable parentClassMeta = null) {
			if( parentClassMeta != null ) {
				var parentClass = m_cachedClasses[parentClassMeta];
				parentClass.ChildClasses.Add(classMeta);
			}

			m_cachedClasses.Add(classMeta, new LuaClass() {
				ClassMetaTable = classMeta
			});
		}

		public bool IsSubClassOf(LuaTable klass, LuaTable klassToCheck) {
			if( !m_cachedClasses.TryGetValue(klassToCheck, out var luaClass) ) {
				return false;
			}

			foreach( var childClass in luaClass.ChildClasses ) {
				if( Equals(childClass, klass) ) {
					return true;
				}
			}

			return false;
		}
	}
}