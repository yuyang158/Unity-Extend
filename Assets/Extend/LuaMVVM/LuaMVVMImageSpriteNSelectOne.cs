using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace Extend.LuaMVVM {
	[RequireComponent(typeof(Image)), LuaCallCSharp]
	public class LuaMVVMImageSpriteNSelectOne : MonoBehaviour {
		[SerializeField]
		private Sprite[] m_sprites;

		private Image m_img;
		private void Awake() {
			m_img = GetComponent<Image>();
		}

		private int m_selectIndex;
		public int SelectIndex {
			get => m_selectIndex;
			set {
				m_selectIndex = value;
				m_img.sprite = m_selectIndex == -1 ? null : m_sprites[m_selectIndex];
			}
		}
	}
}
