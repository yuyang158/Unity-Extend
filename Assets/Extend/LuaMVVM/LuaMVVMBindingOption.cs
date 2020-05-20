using System;
using System.Reflection;
using Extend.Common.Lua;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
using LuaAPI = XLua.LuaDLL.Lua;
using Object = UnityEngine.Object;

namespace Extend.LuaMVVM {
	[Serializable]
	public class LuaMVVMBindingOptions {
		public LuaMVVMBindingOption[] Options;
	}

	[Serializable]
	public class LuaMVVMBindingOption {
		public enum BindMode {
			ONE_WAY,
			TWO_WAY,
			ONE_WAY_TO_SOURCE,
			ONE_TIME
		}

		public Object BindTarget;
		public string BindTargetProp;

		public BindMode Mode = BindMode.ONE_TIME;
		public string Path;

		private ILuaTable m_dataSource;
		private PropertyInfo m_propertyInfo;
		private object m_value;

		private delegate void WatchCallback(ILuaTable self, object val);

		private delegate void Detach(ILuaTable self, string path, WatchCallback callback);

		[CSharpCallLua]
		private WatchCallback watchCallback;

		[CSharpCallLua]
		private Detach detach;

		public void Start() {
			if( !BindTarget ) {
				Debug.LogError($"Binding target is null, Path : {Path} Property : {BindTargetProp}");
				return;
			}

			m_propertyInfo = BindTarget.GetType().GetProperty(BindTargetProp);
			Assert.IsNotNull(m_propertyInfo, BindTargetProp);
			watchCallback = SetPropertyValue;
		}

		public void Destroy() {
			TryDetach();
		}

		public void UpdateToSource() {
			if( m_dataSource == null )
				return;

			if( Mode != BindMode.TWO_WAY && Mode != BindMode.ONE_WAY_TO_SOURCE )
				return;

			var fieldVal = m_propertyInfo.GetValue(BindTarget);
			if( Equals(m_value, fieldVal) ) {
				return;
			}

			m_dataSource.SetInPath(Path, m_value);
			m_value = fieldVal;
		}

		private void SetPropertyValue(ILuaTable _, object val) {
			if( m_propertyInfo.PropertyType == typeof(string) ) {
				m_value = val == null ? "" : val.ToString();
			}
			else {
				m_value = val;
			}

			m_propertyInfo.SetValue(BindTarget, m_value);
		}

		private void TryDetach() {
			if( detach == null )
				return;

			if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
				detach(m_dataSource, Path, watchCallback);
				detach = null;
			}

			m_dataSource?.Dispose();
			m_dataSource = null;

			if( BindTarget is LuaMVVMForEach forEach ) {
				forEach.LuaArrayData = null;
			}
		}

		public override string ToString() {
			return $"Binding {BindTarget}->{Path}";
		}

		public void Bind(ILuaTable dataContext) {
			if( m_dataSource != null ) {
				TryDetach();
			}

			m_dataSource = dataContext;
			if( m_dataSource == null ) {
				return;
			}

			var watch = dataContext.GetInPath<LuaFunction>("watch");
			var val = dataContext.GetInPath<object>(Path);
			if( val == null ) {
				Debug.LogWarning($"Not found value in path {Path}");
				return;
			}

			switch( Mode ) {
				case BindMode.ONE_WAY:
				case BindMode.TWO_WAY:
				case BindMode.ONE_TIME: {
					SetPropertyValue(dataContext, val);
					if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
						watch.Call(dataContext, Path, watchCallback);
						detach = dataContext.Get<Detach>("detach");
						Assert.IsNotNull(detach);
						if( Mode == BindMode.TWO_WAY ) {
							m_value = val;
						}
					}

					break;
				}
				case BindMode.ONE_WAY_TO_SOURCE:
					m_value = m_propertyInfo.GetValue(BindTarget);
					dataContext.SetInPath(Path, m_value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}