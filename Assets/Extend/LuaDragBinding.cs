using Extend.LuaUtil;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extend.Asset {
	public class LuaDragBinding : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
		private LuaBinding m_binding;
		private LuaUnityDragEventFunction m_onBeginDrag;
		private LuaUnityDragEventFunction m_onEndDrag;
		private LuaUnityDragEventFunction m_onDrag;

		private void Awake() {
			m_binding = GetComponent<LuaBinding>();
		}

		private void Start() {
			m_onBeginDrag = m_binding.GetLuaMethod<LuaUnityDragEventFunction>("begin_drag");
			m_onEndDrag = m_binding.GetLuaMethod<LuaUnityDragEventFunction>("end_drag");
			m_onDrag = m_binding.GetLuaMethod<LuaUnityDragEventFunction>("drag");
		}

		public void OnBeginDrag(PointerEventData eventData) {
			m_onBeginDrag?.Invoke(m_binding.LuaInstance, eventData);
		}

		public void OnEndDrag(PointerEventData eventData) {
			m_onEndDrag?.Invoke(m_binding.LuaInstance, eventData);
		}

		public void OnDrag(PointerEventData eventData) {
			m_onDrag?.Invoke(m_binding.LuaInstance, eventData);
		}
	}
}