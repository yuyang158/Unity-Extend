using System.Collections.Generic;
using Extend.Asset;
using Extend.Common;
using TMPro;
using UnityEngine;
using XLua;

namespace Extend.LuaMVVM {
	public class OptionDataWithElement : TMP_Dropdown.OptionData {
		private PackedSprite.SpriteElement m_spriteElement;
		private readonly TMP_Dropdown m_dropdown;

		public OptionDataWithElement(TMP_Dropdown dropdown) {
			m_dropdown = dropdown;
		}

		public PackedSprite.SpriteElement SpriteElement {
			get => m_spriteElement;
			set {
				if( m_spriteElement != null ) {
					SpriteElement.OnSpriteLoaded -= OnSpriteLoaded;
					SpriteElement.Release();
				}

				m_spriteElement = value;
				if( m_spriteElement != null ) {
					m_spriteElement.OnSpriteLoaded += OnSpriteLoaded;
					m_spriteElement.Acquire();
				}
			}
		}

		private void OnSpriteLoaded(PackedSprite.SpriteElement element) {
			this.image = element.Sprite;
			m_dropdown.RefreshShownValue();
		}
	}

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
				if( m_dropdown.options != null && m_dropdown.options.Count > 0 ) {
					foreach( var optionData in m_dropdown.options ) {
						var option = optionData as OptionDataWithElement;
						option.SpriteElement = null;
					}
				}

				m_arrayData?.Dispose();
				m_arrayData = value;

				var count = m_arrayData?.Length ?? 0;
				List<TMP_Dropdown.OptionData> optionsData = new List<TMP_Dropdown.OptionData>(count);
				for( int i = 0; i < count; i++ ) {
					m_arrayData.Get(i + 1, out LuaTable data);
					var txt = data.GetInPath<string>("text");
					var iconPath = m_arrayData.GetInPath<string>("icon");
					var option = new OptionDataWithElement(m_dropdown) {
						text = txt
					};
					if( !string.IsNullOrEmpty(iconPath) ) {
						option.SpriteElement = SpriteAssetService.Get().RequestIcon(iconPath);
					}

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