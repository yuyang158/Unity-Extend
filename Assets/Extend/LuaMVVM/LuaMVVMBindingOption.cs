using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;
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

		private LuaTable m_dataSource;
		private PropertyInfo m_propertyInfo;
		private object m_value;

		[CSharpCallLua]
		private delegate void WatchCallback(LuaTable self, object val);

		[CSharpCallLua]
		private delegate void Detach(LuaTable self, string path, WatchCallback callback);

		private WatchCallback watchCallback;
		private Detach detach;

		private Delegate m_getPropertyDel;

		public void Start() {
			if( !BindTarget ) {
				Debug.LogError($"Binding target is null, Path : {Path} Property : {BindTargetProp}");
				return;
			}

			m_propertyInfo = BindTarget.GetType().GetProperty(BindTargetProp);
			Assert.IsNotNull(m_propertyInfo, BindTargetProp);
			watchCallback = SetPropertyValue;

			if( Mode == BindMode.TWO_WAY || Mode == BindMode.ONE_WAY_TO_SOURCE ) {
				m_getPropertyDel = Delegate.CreateDelegate(typeof(Func<object>), BindTarget, m_propertyInfo.GetMethod);
			}
		}

		public void Destroy() {
			TryDetach();
		}

		public void UpdateToSource() {
			if( m_dataSource == null )
				return;

			if( m_getPropertyDel == null )
				return;

			var fieldVal = m_getPropertyDel.DynamicInvoke();
			if( Equals(m_value, fieldVal) ) {
				return;
			}

			m_value = fieldVal;
			m_dataSource.SetInPath(Path, fieldVal);
		}

		private void SetPropertyValue(LuaTable _, object val) {
			if( m_propertyInfo.PropertyType == typeof(string) ) {
				m_value = val == null ? "" : val.ToString();
			}
			else if( m_propertyInfo.PropertyType == typeof(float) ) {
				m_value = (float)(double)val;
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

		public void Bind(LuaTable dataContext) {
			if( m_dataSource != null ) {
				TryDetach();
			}

			m_dataSource = dataContext;
			if( m_dataSource == null ) {
				return;
			}

			var watch = dataContext.GetInPath<Action<LuaTable, string, WatchCallback>>("watch");
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
						watch(dataContext, Path, watchCallback);
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