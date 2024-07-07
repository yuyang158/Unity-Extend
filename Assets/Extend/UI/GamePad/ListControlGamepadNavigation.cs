using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Extend.UI.GamePad {
	public class ListControlGamepadNavigation : MonoBehaviour {
		private static readonly List<Selectable> m_selectables = new(32);
		public enum Orientation {
			Vertical,
			Horizon
		}

		[SerializeField]
		private Orientation m_orientation = Orientation.Vertical;
		
		private void OnTransformChildrenChanged() {
			if( transform.childCount <= 1 ) {
				return;
			}
			m_selectables.Clear();
			var navigation = new Navigation {
				mode = Navigation.Mode.Explicit
			};
			for( int i = 0; i < transform.childCount; i++ ) {
				var t = transform.GetChild(i);
				var selectable = t.GetComponent<Selectable>();
				m_selectables.Add(selectable);
			}

			if( m_orientation == Orientation.Horizon ) {
				for( int i = 0; i < m_selectables.Count; i++ ) {
					var selectable = m_selectables[i];
					if( i == 0 ) {
						navigation.selectOnLeft = m_selectables[^1];
						navigation.selectOnRight = m_selectables[1];
					}
					else if( i == m_selectables.Count - 1 ) {
						navigation.selectOnLeft = m_selectables[i - 1];
						navigation.selectOnRight = m_selectables[0];
					}
					else {
						navigation.selectOnLeft = m_selectables[i - 1];
						navigation.selectOnRight = m_selectables[i + 1];
					}

					selectable.navigation = navigation;
				}
			}
			else {
				for( int i = 0; i < m_selectables.Count; i++ ) {
					var selectable = m_selectables[i];
					if( i == 0 ) {
						navigation.selectOnUp = m_selectables[^1];
						navigation.selectOnDown = m_selectables[1];
					}
					else if( i == m_selectables.Count - 1 ) {
						navigation.selectOnUp = m_selectables[i - 1];
						navigation.selectOnDown = m_selectables[0];
					}
					else {
						navigation.selectOnUp = m_selectables[i - 1];
						navigation.selectOnDown = m_selectables[i + 1];
					}

					selectable.navigation = navigation;
				}
			}
		}
	}
}