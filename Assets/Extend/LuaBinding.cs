using System;
using System.Collections.Generic;
using System.Linq;
using Extend.Common;
using Extend.LuaBindingData;
using UnityEngine;
using XLua;

namespace Extend {
	[CSharpCallLua, LuaCallCSharp]
	public class LuaBinding : MonoBehaviour, ISerializationCallbackReceiver {
		[AssetPath(AssetType = typeof(TextAsset), RootDir = "Assets/Resources/Lua", Extension = ".lua"), BlackList]
		public string LuaFile;
		public LuaTable LuaInstance { get; private set; }

		private delegate void LuaUnityEventFunction(LuaTable self);

		private void Awake() {
			if( string.IsNullOrEmpty(LuaFile) )
				return;
			var ret = CSharpServiceManager.Get<LuaVM>(CSharpServiceManager.ServiceType.LUA_SERVICE).LoadFileAtPath(LuaFile);
			if( !( ret[0] is LuaTable luaClass ) )
				return;
			var constructor = luaClass.Get<LuaFunction>("new");
			ret = constructor?.Call(gameObject);
			if( ret.Length <= 0 )
				return;
			var luaTable = ret[0] as LuaTable;
			Bind(luaTable);

			var awake = luaTable.Get<LuaUnityEventFunction>("awake");
			awake?.Invoke(luaTable);
		}

		private void Start() {
			var start = LuaInstance.Get<LuaUnityEventFunction>("start");
			start?.Invoke(LuaInstance);
		}

		private void OnDestroy() {
			var destroy = LuaInstance.Get<LuaUnityEventFunction>("destroy");
			destroy?.Invoke(LuaInstance);
			LuaInstance?.Dispose();
			LuaInstance = null;
		}

		[BlackList, NonSerialized]
		public List<LuaBindingDataBase> BindingContainer;
		
		public void Bind(LuaTable instance) {
			LuaInstance = instance;
			LuaInstance.SetInPath("__CSBinding", this);
			if( BindingContainer == null ) return;
			foreach( var binding in BindingContainer ) {
				binding.ApplyToLuaInstance(instance);
			}
		}

		[HideInInspector, BlackList]
		public LuaBindingIntegerData[] IntData;
		[HideInInspector, BlackList]
		public LuaBindingBooleanData[] BoolData;
		[HideInInspector, BlackList]
		public LuaBindingNumberData[] NumData;
		[HideInInspector, BlackList]
		public LuaBindingStringData[] StrData;
		[HideInInspector, BlackList]
		public LuaBindingUOData[] UOData;
		[HideInInspector, BlackList]
		public LuaBindingAssetReferenceData[] AssetReferenceData;
		[HideInInspector, BlackList]
		public LuaBindingUOArrayData[] UOArrayData;

		[BlackList]
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

		[BlackList]
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
