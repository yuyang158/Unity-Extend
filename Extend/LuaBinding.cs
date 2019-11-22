using System;
using System.Collections.Generic;
using SerializableCollections;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Extend {
	[CSharpCallLua, LuaCallCSharp]
	public class LuaBinding : MonoBehaviour {
		public string LuaFile;

		private void Awake() {
			if( string.IsNullOrEmpty( LuaFile ) )
				return;
			var ret = LuaVM.Default.LoadFileAtPath( LuaFile );
			var luaClass = ret[0] as LuaTable;
			if( luaClass == null )
				return;
			var constructor = luaClass.Get<LuaFunction>( "new" );
			constructor?.Call( gameObject );
		}

		[HideInInspector] public SerializableDictionary<string, object> BindingContainer;
		private LuaTable bindInstance;

		public void Bind(LuaTable instance) {
			bindInstance = instance;
			if( BindingContainer == null ) return;
			foreach( var binding in BindingContainer ) {
				if( binding.Value != null ) {
					if( !string.IsNullOrEmpty( binding.Key ) ) {
						bindInstance.Set( binding.Key, binding.Value );
					}
					else {
						Debug.LogWarning( $"Variable name component is empty : {binding.Key}" );
					}
				}
				else {
					Debug.LogWarning( $"Binding component is null : {binding.Value}" );
				}
			}
		}
	}
}