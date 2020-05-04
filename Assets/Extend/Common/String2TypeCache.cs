using System;
using System.Collections.Generic;
using System.Linq;

namespace Extend.Common {
	public static class String2TypeCache {
		private static readonly Dictionary<string, Type> str2Types = new Dictionary<string, Type>();

		public static Type GetType( string fullTypeName ) {
			if( str2Types.TryGetValue(fullTypeName, out var type) ) {
				return type;
			}

			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
			foreach( var t in types ) {
				if( t.FullName == fullTypeName ) {
					str2Types.Add(fullTypeName, t);
					return t;
				}
			}
			return null;
		}
	}
}