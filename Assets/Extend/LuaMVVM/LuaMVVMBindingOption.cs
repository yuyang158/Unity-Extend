using System;
using System.Reflection;
using Extend.Common;
using Extend.LuaBindingEvent;
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
			ONE_TIME,
			EVENT
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
		private LuaFunction m_bindFunc;

#if UNITY_EDITOR
		public static Action<GameObject> DebugCheckCallback;
#endif

		public void Prepare(GameObject owner) {
			if( !BindTarget ) {
				Debug.LogError($"Binding target is null, Path : {Path} Property : {BindTargetProp}, Owner: {owner.name}");
				return;
			}

			m_propertyInfo = BindTargetProp == "SetActive" ? null : BindTarget.GetType().GetProperty(BindTargetProp);
			watchCallback = SetPropertyValue;
		}

		public void Destroy() {
			TryDetach();
		}

		private void SetPropertyValue(object val) {
#if UNITY_EDITOR
			if( BindTarget ) {
				DebugCheckCallback?.Invoke(BindTarget.gameObject);
			}
			else {
				Debug.Log("");
			}
#endif

#if UNITY_DEBUG
			StatService.Get().Increase(StatService.StatName.MVVM_DISPATCH, 1);
#endif

			try {
				if( m_propertyInfo == null ) {
					BindTarget.gameObject.SetActive((bool)val);
				}
				else {
					if( m_propertyInfo.PropertyType == typeof(string) ) {
						m_propertyInfo.SetValue(BindTarget, val == null ? "" : val.ToString());
					}
					else if( m_propertyInfo.PropertyType == typeof(float) ) {
						if( val is long i ) {
							m_propertyInfo.SetValue(BindTarget, (float)i);
						}
						else {
							m_propertyInfo.SetValue(BindTarget, (float)(double)val);
						}
					}
					else if( m_propertyInfo.PropertyType == typeof(int) ) {
						if( val is long i ) {
							m_propertyInfo.SetValue(BindTarget, (int)i);
						}
						else {
							m_propertyInfo.SetValue(BindTarget, (int)(double)val);
						}
					}
					else {
						m_propertyInfo.SetValue(BindTarget, val);
					}
				}
			}
			catch( Exception e ) {
				Debug.LogError($"MVVM Set Property Error : {BindTarget}.{Path} = {val}");
				Debug.LogError(e);
			}
		}

		public void TryDetach() {
			if( !CSharpServiceManager.Initialized )
				return;
			if( m_dataSource == null )
				return;

			if( Mode == BindMode.EVENT ) {
				if( m_bindFunc == null || !BindTarget )
					return;
				LuaBindingEventBase.UnbindEvent(BindTargetProp, BindTarget.gameObject, m_bindFunc);
				m_bindFunc?.Dispose();
				m_bindFunc = null;
				m_dataSource.Dispose();
				m_dataSource = null;
				return;
			}

			if( detach != null && ( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) ) {
				detach(m_dataSource, m_expression ? GetExpressionKey() : Path, watchCallback);
				detach = null;
			}

			m_dataSource.Dispose();
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

		public string GetExpressionKey() {
			return $"{Path}_{BindTarget.gameObject.GetInstanceID().ToString()}".GetHashCode().ToString();
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
					using( var setupTempFunc = m_dataSource.GetInPath<LuaFunction>("setup_temp_getter") ) {
						bindingValue = setupTempFunc.Func<string, LuaFunction, object>(GetExpressionKey(), function);
					}
				}
				else {
					if( m_dataSource == null ) {
						return;
					}

					if( Mode == BindMode.EVENT ) {
						m_bindFunc = dataContext.GetInPath<LuaFunction>(Path);
						Assert.IsNotNull(m_bindFunc);
						LuaBindingEventBase.BindEvent(BindTargetProp, BindTarget.gameObject, m_bindFunc);
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
					if( Mode == BindMode.ONE_WAY || Mode == BindMode.TWO_WAY ) {
						var watch = dataContext.GetInPath<WatchLuaProperty>("watch");
						watch(dataContext, m_expression ? GetExpressionKey() : Path, watchCallback, m_expression);
						detach = dataContext.Get<DetachLuaProperty>("detach");
						Assert.IsNotNull(detach);

						if( Mode == BindMode.TWO_WAY ) {
							if( m_expression ) {
								Debug.LogError("express type can not to source");
							}
							else {
								m_propertyChangeCallback = BindTarget.GetComponent<IUnityPropertyChanged>();
								m_propertyChangeCallback.OnPropertyChanged += OnPropertyChanged;
							}
						}
					}

					SetPropertyValue(bindingValue);

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