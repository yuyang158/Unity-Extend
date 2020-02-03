using System;
using System.Collections.Generic;
using System.Linq;
using Extend.LuaBindingData;
using UnityEngine;
using XLua;

namespace Extend {
	[CSharpCallLua, LuaCallCSharp]
	public class LuaBinding : MonoBehaviour, ISerializationCallbackReceiver {
		[AssetPath(AssetType = typeof(TextAsset), order = 1, RootDir = "Assets/Resources/Lua", Extension = ".lua")]
		public string LuaFile;

		private delegate void LuaUnityEventFunction(LuaTable self);

		private void Awake() {
			if( string.IsNullOrEmpty(LuaFile) )
				return;
			var ret = LuaVM.Default.LoadFileAtPath(LuaFile);
			if( !( ret[0] is LuaTable luaClass ) )
				return;
			var constructor = luaClass.Get<LuaFunction>("new");
			ret = constructor?.Call(gameObject);
			if( ret.Length <= 0 )
				return;
			var luaTable = ret[0] as LuaTable;
			Bind(luaTable);

			var awake = luaTable.Get<LuaUnityEventFunction>("awake");
			awake(luaTable);
		}

		private void Start() {
			var start = LuaInstance.Get<LuaUnityEventFunction>("start");
			start(LuaInstance);
		}

		[BlackList, NonSerialized]
		public List<LuaBindingDataBase> BindingContainer;

		public LuaTable LuaInstance { get; private set; }

		public void Bind(LuaTable instance) {
			LuaInstance = instance;
			LuaInstance.SetInPath("__CSBinding", this);
			if( BindingContainer == null ) return;
			foreach( var binding in BindingContainer ) {
				binding.ApplyToLuaInstance(instance);
			}
		}

		[HideInInspector]
		public LuaBindingIntegerData[] IntData;
		[HideInInspector]
		public LuaBindingBooleanData[] BoolData;
		[HideInInspector]
		public LuaBindingNumberData[] NumData;
		[HideInInspector]
		public LuaBindingStringData[] StrData;
		[HideInInspector]
		public LuaBindingUOData[] UOData;
		[HideInInspector]
		public LuaBindingAssetReferenceData[] AssetReferenceData;
		[HideInInspector]
		public LuaBindingUOArrayData[] UOArrayData;

		public void OnBeforeSerialize() {
			var fieldInfos = GetType().GetFields();
			if( BindingContainer == null || BindingContainer.Count == 0 ) {
				foreach( var info in fieldInfos ) {
					if( info.FieldType.IsArray && info.FieldType.GetElementType().IsSubclassOf(typeof(LuaBindingDataBase)) ) {
						info.SetValue(this, null);
					}
				}
				return;
			}

			foreach( var fieldInfo in fieldInfos ) {
				if( !fieldInfo.FieldType.IsArray || !fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(LuaBindingDataBase)) ) continue;
				var count = BindingContainer.Count(bind => bind.GetType() == fieldInfo.FieldType.GetElementType());

				if( count > 0 ) {
					var arr = Array.CreateInstance(fieldInfo.FieldType.GetElementType() ?? throw new Exception(), count);
					var index = 0;
					foreach( var bind in BindingContainer.Where(bind => bind.GetType() == fieldInfo.FieldType.GetElementType()) ) {
						arr.SetValue(bind, index);
						index++;
					}
					fieldInfo.SetValue(this, arr);
				}
				else {
					fieldInfo.SetValue(this, null);
				}
			}
		}

		public void OnAfterDeserialize() {
			var fieldInfos = GetType().GetFields();

			BindingContainer = new List<LuaBindingDataBase>();
			foreach( var fieldInfo in fieldInfos ) {
				if( !fieldInfo.FieldType.IsArray ) continue;

				var arr = fieldInfo.GetValue(this) as Array;
				if(arr == null || arr.Length == 0)
					continue;
				foreach( var element in arr ) {
					BindingContainer.Add(element as LuaBindingDataBase);
				}
			}
		}
	}
}