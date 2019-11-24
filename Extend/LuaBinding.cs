using System;
using System.Collections.Generic;
using SerializableCollections;
using UnityEngine;
using XLua;
using Object = UnityEngine.Object;

namespace Extend {
	[CSharpCallLua, LuaCallCSharp]
	public class LuaBinding : MonoBehaviour {
		[Serializable]
		public class BindingDictionaryContainer : SerializableDictionary<string, object> {
			
		}
		
		public string LuaFile;

		private void Awake() {
			if( string.IsNullOrEmpty( LuaFile ) )
				return;
			var ret = LuaVM.Default.LoadFileAtPath( LuaFile );
			if( !( ret[0] is LuaTable luaClass ) )
				return;
			var constructor = luaClass.Get<LuaFunction>( "new" );
			constructor?.Call( gameObject );
		}

		[HideInInspector] public BindingDictionaryContainer BindingContainer;
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