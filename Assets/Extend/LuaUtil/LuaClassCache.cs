using System;
using System.Collections.Generic;
using XLua;

namespace Extend.LuaUtil {
	public class LuaClassCache {
		public class LuaClass {
			public LuaTable ClassMetaTable;
			public LuaClass ParentClass;
			public readonly List<LuaTable> ChildClasses = new();
			private readonly Dictionary<string, Delegate> m_cachedMethods = new();

			public T GetLuaMethod<T>(string methodName) where T : Delegate {
				if( m_cachedMethods.TryGetValue(methodName, out var m) ) {
					return m as T;
				}
				
				var luaMethod = ClassMetaTable.GetInPath<T>(methodName);
				m_cachedMethods.Add(methodName, luaMethod);
				return luaMethod;
			}

			public void AddChildClass(LuaTable childClass) {
				ChildClasses.Add(childClass);
				ParentClass?.AddChildClass(childClass);
			}
		}

		private readonly Dictionary<LuaTable, LuaClass> m_cachedClasses = new Dictionary<LuaTable, LuaClass>(256);

		public void Register(LuaTable classMeta, LuaTable parentClassMeta = null) {
			LuaClass parentClass = null;
			if( parentClassMeta != null ) {
				parentClass = m_cachedClasses[parentClassMeta];
				parentClass.AddChildClass(classMeta);
			}

			var klass = new LuaClass {
				ClassMetaTable = classMeta,
				ParentClass = parentClass
			};
			m_cachedClasses.Add(classMeta, klass);
		}

		public LuaClass TryGetClass(LuaTable klass) {
			m_cachedClasses.TryGetValue(klass, out var luaClass);
			return luaClass;
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

		public void Clear() {
			m_cachedClasses.Clear();
		}
	}
}