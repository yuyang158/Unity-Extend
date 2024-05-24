using System.Collections.Generic;
using Extend.Asset;
using Extend.Common;
using TMPro;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {

	[RequireComponent(typeof(TMP_Dropdown))]
	public class LuaMVVMDropdown : MonoBehaviour, ILuaMVVM {
		private TMP_Dropdown m_dropdown;
		private LuaTable m_arrayData;

		private void Awake() {
			m_dropdown = GetComponent<TMP_Dropdown>();
			m_dropdown.options.Clear();
		}

		public LuaTable LuaArrayData {
			get => m_arrayData;
			set {
				m_arrayData?.Dispose();
				m_arrayData = value;

				var count = m_arrayData?.Length ?? 0;
				List<TMP_Dropdown.OptionData> optionsData = new List<TMP_Dropdown.OptionData>(count);
				for( int i = 0; i < count; i++ ) {
					m_arrayData.Get(i + 1, out LuaTable data);
					var txt = data.GetInPath<string>("text");
					var icon = m_arrayData.GetInPath<Sprite>("icon");
					var option = new TMP_Dropdown.OptionData(txt, icon);
					data.Dispose();
					optionsData.Add(option);
				}

				m_dropdown.options = optionsData;
			}
		}

		public void Detach() {
			LuaArrayData = null;
		}

		public void SetDataContext(LuaTable dataSource) {
			LuaArrayData = dataSource;
		}

		public LuaTable GetDataContext() {
			return LuaArrayData;
		}

		private void OnDestroy() {
			if( !CSharpServiceManager.Initialized ) {
				return;
			}

			Detach();
		}
	}
}
