using System;
using System.Reflection;
using Extend.LuaMVVM.PropertyChangeInvoke;
using Extend.LuaUtil;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

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

		public Component BindTarget;
		public string BindTargetProp;

		public BindMode Mode = BindMode.ONE_TIME;
		public string Path;

		private LuaTable m_dataSource;
		private PropertyInfo m_propertyInfo;
		private WatchCallback watchCallback;
		private DetachLuaProperty detach;
		private IUnityPropertyChanged m_propertyChangeCallback;

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

		private void SetPropertyValue(LuaTable _, object val) {
			if( m_propertyInfo.PropertyType == typeof(string) ) {
				m_propertyInfo.SetValue(BindTarget, val == null ? "" : val.ToString());
			}
			else if( m_propertyInfo.PropertyType == typeof(float) ) {
				m_propertyInfo.SetValue(BindTarget, (float)(double)val);
			}
			else {
				m_propertyInfo.SetValue(BindTarget, val);
			}

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

			if( m_propertyChangeCallback != null ) {
				m_propertyChangeCallback.OnPropertyChanged -= OnPropertyChanged;
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
						var watch = dataContext.GetInPath<WatchLuaProperty>("watch");
						watch(dataContext, Path, watchCallback);
						detach = dataContext.Get<DetachLuaProperty>("detach");
						Assert.IsNotNull(detach);

						if( Mode == BindMode.TWO_WAY ) {
							m_propertyChangeCallback = BindTarget.GetComponent<IUnityPropertyChanged>();
							m_propertyChangeCallback.OnPropertyChanged += OnPropertyChanged;
						}
					}
					break;
				}
				case BindMode.ONE_WAY_TO_SOURCE:
					m_propertyChangeCallback = BindTarget.GetComponent<IUnityPropertyChanged>();
					dataContext.SetInPath(Path, m_propertyChangeCallback.ProvideCurrentValue());
					m_propertyChangeCallback.OnPropertyChanged += OnPropertyChanged;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnPropertyChanged(Component sender, object value) {
			m_dataSource.SetInPath(Path, value);
		}
	}
}