using System;
using System.Collections.Generic;
using System.Reflection;
using Extend.Common;
using Extend.LuaBindingEvent;
using Extend.LuaMVVM.PropertyChangeInvoke;
using Extend.LuaUtil;
using Extend.StateActionGroup;
using Extend.Switcher;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using XLua;

namespace Extend.LuaMVVM {
	[Serializable]
	public class LuaMVVMBindingOptions {
		public LuaMVVMBindingOption[] Options;

		public void Sort() {
			Array.Sort(Options);
		}
	}

	[Serializable]
	public class LuaMVVMBindingOption : IComparable<LuaMVVMBindingOption> {
		public enum BindMode : byte {
			ONE_WAY,
			TWO_WAY,
			ONE_WAY_TO_SOURCE,
			ONE_TIME,
			EVENT
		}

		private static readonly Dictionary<Type, Type> m_sourceBindRelations = new Dictionary<Type, Type> {
			{typeof(TMP_Dropdown), typeof(DropdownValueChanged)},
			{typeof(Slider), typeof(SliderValueChanged)},
			{typeof(TMP_InputField), typeof(TMP_InputTextChanged)},
			{typeof(InputField), typeof(UGUI_InputTextChanged)},
			{typeof(Toggle), typeof(ToggleIsOnChanged)},
			{typeof(ToggleSAG), typeof(ToggleIsOnChanged)},
			{typeof(StateToggle), typeof(ToggleIsOnChanged)}
		};

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
		private bool m_prepared;

#if UNITY_EDITOR
		public static Action<GameObject> DebugCheckCallback;
#endif

		public void Prepare() {
			if( !BindTarget || m_prepared ) {
				// Debug.LogError($"Binding target is null, Path : {Path} Property : {BindTargetProp}, Owner: {owner.name}", owner);
				return;
			}

			m_propertyInfo = BindTargetProp == "SetActive" ? null : BindTarget.GetType().GetProperty(BindTargetProp);
			watchCallback = SetPropertyValue;
			m_prepared = true;
		}

		public void Destroy() {
			TryDetach();
		}

		private void SetPropertyValue(object val) {
			if( !m_prepared ) {
				// Debug.LogError($"LuaMVVMBinding {BindTarget}.{Path} not ready, dont SetDataContext in awake.");
				Prepare();
			}
			
#if UNITY_EDITOR
			if( BindTarget ) {
				DebugCheckCallback?.Invoke(BindTarget.gameObject);
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
				Debug.LogError($"MVVM Set Property Error : {BindTarget}.{BindTargetProp} = {Path}:{val}");
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
				LuaBindingEventBase.UnbindEvent(BindTargetProp, BindTarget, m_bindFunc);
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

			if( BindTarget is ILuaMVVM mvvm ) {
				mvvm.Detach();
			}

			if( m_propertyChangeCallback != null ) {
				m_propertyChangeCallback.OnPropertyChanged -= OnPropertyChanged;
			}
		}

		public int CompareTo(LuaMVVMBindingOption other) {
			if( BindTargetProp == "SetActive" ) {
				return -1;
			}

			if( other.BindTargetProp == "SetActive" ) {
				return 1;
			}

			return BindTarget.GetInstanceID().CompareTo(other.BindTarget.GetInstanceID());
		}

		public override string ToString() {
			return $"Binding {BindTarget}.{BindTargetProp} = {Path}";
		}

		public string GetExpressionKey() {
			return $"{Path}_{BindTarget.gameObject.GetInstanceID().ToString()}".GetHashCode().ToString();
		}

		private object m_luaValue;
		public object LuaValue => m_luaValue;

		public void Bind(LuaTable dataContext) {
			if( m_dataSource != null ) {
				if( m_dataSource.Equals(dataContext) ) {
					return;
				}
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
						if( m_bindFunc != null ) {
							LuaBindingEventBase.BindEvent(BindTargetProp, BindTarget, m_bindFunc);	
						}
						return;
					}

					bindingValue = Path == "self" ? dataContext : dataContext.GetInPath<object>(Path);
				}
			}
			catch( Exception e ) {
				Debug.LogException(e);
			}

			m_luaValue = bindingValue;
			if( bindingValue == null && Mode != BindMode.ONE_WAY_TO_SOURCE) {
				// Debug.LogWarning($"Not found value in path {Path}");
				return;
			}

			switch( Mode ) {
				case BindMode.ONE_WAY:
				case BindMode.TWO_WAY:
				case BindMode.ONE_TIME: {
					if( Mode is BindMode.ONE_WAY or BindMode.TWO_WAY ) {
						if( !m_prepared ) {
							Prepare();
						}
						var watch = dataContext.GetInPath<WatchLuaProperty>("watch");
						watch(dataContext, m_expression ? GetExpressionKey() : Path, watchCallback, m_expression);
						detach = dataContext.Get<DetachLuaProperty>("detach");
						Assert.IsNotNull(detach);

						if( Mode == BindMode.TWO_WAY ) {
							if( m_expression ) {
								Debug.LogError("express type can not to source");
							}
							else {
								m_propertyChangeCallback = GetOrAddPropertyChange();
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

					m_propertyChangeCallback = GetOrAddPropertyChange();
					dataContext.SetInPath(Path, m_propertyChangeCallback.ProvideCurrentValue());
					m_propertyChangeCallback.OnPropertyChanged += OnPropertyChanged;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private IUnityPropertyChanged GetOrAddPropertyChange() {
			return BindTarget.GetOrAddComponent(m_sourceBindRelations[BindTarget.GetType()]) as IUnityPropertyChanged;
		}

		private void OnPropertyChanged(Component sender, object value) {
			m_dataSource.SetInPath(Path, value);
		}
	}
}
