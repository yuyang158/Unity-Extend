using System.Collections.Generic;
using Common;
using UnityEngine;
using XLua;

namespace Extend {
	[CSharpCallLua]
	public class LuaMVVM : IService, IServiceUpdate {
		private class RandomList<T> : List<T> {
			public bool SwapRemove(T item) {
				var index = IndexOf( item );
				if( index < 0 ) {
					return false;
				}

				if( Count == 1 ) {
					Clear();
				}
				else {
					var lastIndex = Count - 1;
					if( index != lastIndex ) {
						var last = this[Count - 1];
						this[index] = last;
					}

					RemoveAt( lastIndex );
				}

				return true;
			}
		}

		private class MVVMBindingList : RandomList<LuaMVVMBinding> {
		}

		private readonly Dictionary<string, MVVMBindingList> bindings = new Dictionary<string, MVVMBindingList>();

		private delegate Dictionary<string, Dictionary<string, object>> FetchChangeMethod();

		private delegate LuaTable GetRootDoc(string rootName);

		private FetchChangeMethod fetchMethod;
		private GetRootDoc getLuaRootDoc;
		private LuaTable module;

		public void RegisterBinding(LuaMVVMBinding binding) {
			if( !bindings.TryGetValue( binding.path, out var list ) ) {
				list = new MVVMBindingList();
				bindings.Add( binding.path, list );
			}

			list.Add( binding );
		}

		public void UnregisterBinding(LuaMVVMBinding binding) {
			if( !bindings.TryGetValue( binding.path, out var list ) ) return;
			if( !list.SwapRemove( binding ) ) {
				Debug.LogWarning( $"Not exist binding : {binding}" );
			}
		}

		public LuaTable GetDocRoot(string path) {
			return getLuaRootDoc( path );
		}

		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.MVVM_SERVICE;

		public void Initialize() {
			var ret = LuaVM.Default.LoadFileAtPath( "mvvm" );
			module = ret[0] as LuaTable;
			fetchMethod = module.GetInPath<FetchChangeMethod>( "fetch_all" );
			getLuaRootDoc = module.GetInPath<GetRootDoc>( "get_doc" );
		}

		public void Destroy() {
		}

		public void Update() {
			var changes = fetchMethod();
			foreach( var item in changes ) {
				foreach( var dirtyValue in item.Value ) {
					if( !bindings.TryGetValue( dirtyValue.Key, out var list ) ) continue;
					foreach( var binding in list ) {
						binding.Change( dirtyValue.Value );
					}
				}
			}
		}
	}
}