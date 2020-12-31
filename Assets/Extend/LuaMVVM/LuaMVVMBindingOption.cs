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
		public enum BindMode : byte {
			ONE_WAY,
			TWO_WAY,
			ONE_WAY_TO_SOURCE,
			ONE_TIME
		}

		public Component BindTarget;
		public string BindTargetProp;

		public BindMode Mode = BindMode.ONE_TIME;
		public string Path;
		
		[SerializeField]
		private bool m_expression;

		private LuaTable m_dataSource;
		private PropertyInfo m_propertyInfo;
		private WatchCallback watchCallback;
		private DetachLuaProperty detach;
		private IUnityPropertyChanged m_propertyChangeCallback;

#if UNITY_EDITOR
		public static Action<GameObject> DebugCheckCallback;
#endif

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
#if UNITY_EDITOR
			if( BindTarget ) {
				DebugCheckCallback?.Invoke(BindTarget.gameObject);
			}
#endif

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
				detach(m_dataSource, m_expression ? Path.GetHashCode().ToString() : Path, watchCallback);
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

			object bindingValue = null;
			try {
				m_dataSource = dataContext;
				if( m_expression ) {
					var function = TempBindingExpressCache.GenerateTempFunction(ref Path);
					var setupTempFunc = m_dataSource.GetInPath<LuaFunction>("setup_temp_getter");
					var key = Path.GetHashCode().ToString();
					setupTempFunc.Call(key, function);
					bindingValue = dataContext.GetInPath<object>(key);
				}
				else {
					if( m_dataSource == null ) {
						return;
					}

					bindingValue = dataContext.GetInPath<object>(Path);
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}


			if( bindingValue == null ) {
				Debug.LogWarning($"Not found value in path {Path}");
				return;
			}

			switch( Mode ) {
				case BindMode.ONE_WAY:
				case BindMode.TWO_WAY:
				case BindMode.ONE_TIME: {
					SetPropertyValue(dataContext, bindingValue);
					if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
						var watch = dataContext.GetInPath<WatchLuaProperty>("watch");
						watch(dataContext, m_expression ? Path.GetHashCode().ToString() : Path, watchCallback);
						detach = dataContext.Get<DetachLuaProperty>("detach");
						Assert.IsNotNull(detach);

						if( Mode == BindMode.TWO_WAY ) {
							if( m_expression ) {
								Debug.LogError("express type can not to source");
								return;
							}
							m_propertyChangeCallback = BindTarget.GetComponent<IUnityPropertyChanged>();
							m_propertyChangeCallback.OnPropertyChanged += OnPropertyChanged;
						}
					}

					break;
				}
				case BindMode.ONE_WAY_TO_SOURCE:
					if( m_expression ) {
						Debug.LogError("express type can not to source");
						return;
					}
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