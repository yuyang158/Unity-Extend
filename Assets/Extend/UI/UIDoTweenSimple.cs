using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace Extend.UI {
	[LuaCallCSharp]
	public class UIDoTweenSimple : MonoBehaviour {
		public UIAnimation Animation;
		private Tween[] m_currentTweens;

		private Tween[] CurrentTweens {
			get => m_currentTweens;
			set {
				if( CurrentTweens != null ) {
					foreach( var tween in CurrentTweens ) {
						tween.Kill(true);
					}
				}

				m_currentTweens = value;
			}
		}
		
		private void Awake() {
			Animation.CacheStartValue(transform);
		}

		public void Play() {
			CurrentTweens = Animation.Active(transform);
		}
	}
}