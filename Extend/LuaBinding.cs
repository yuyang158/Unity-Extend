using System;
using System.Collections.Generic;
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
			var constructor = luaClass.Get<LuaFunction>( "new" );
			constructor.Call( gameObject );
		}

		[Serializable]
		public class LuaUnityBinding {
			public Object UnityObject;
			public string VariableName;
		}

		[HideInInspector] public List<LuaUnityBinding> BindingContainer;
		private LuaTable bindInstance;

		public void Bind(LuaTable instance) {
			bindInstance = instance;
			if( BindingContainer == null ) return;
			foreach( var binding in BindingContainer ) {
				if( binding.UnityObject ) {
					if( !string.IsNullOrEmpty( binding.VariableName ) ) {
						bindInstance.Set( binding.VariableName, binding.UnityObject );
					}
					else {
						Debug.LogWarning( $"Variable name component is empty : {binding.UnityObject}" );
					}
				}
				else {
					Debug.LogWarning( $"Binding component is null : {binding.VariableName}" );
				}
			}
		}
	}
}