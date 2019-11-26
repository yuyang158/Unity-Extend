using System;
using System.Collections.Generic;
using System.Linq;

namespace Extend.LuaBindingData {
	public static class String2TypeCache {
		private static readonly Dictionary<string, Type> str2Types = new Dictionary<string, Type>();

		public static Type GetType( string fullTypeName ) {
			if( str2Types.TryGetValue(fullTypeName, out var type) ) {
				return type;
			}
			type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).First(x => x.FullName == fullTypeName);
			if( type == null )
				return null;
			str2Types.Add(fullTypeName, type);
			return type;
		}
	}
}