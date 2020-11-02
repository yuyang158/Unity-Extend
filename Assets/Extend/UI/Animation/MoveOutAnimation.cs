using System;
using DG.Tweening;
using Extend.Common;
using UnityEngine;

namespace Extend.UI.Animation {
	[Serializable]
	public class MoveOutAnimation : StateAnimation {
		public enum Direction {
			Left,
			Top,
			Right,
			Bottom
		}

		[SerializeField]
		private bool m_customFromTo;

		[SerializeField]
		private Vector3 m_moveTo;

		[SerializeField]
		private Vector3 m_moveFrom;

		[SerializeField, LabelText("Move From")]
		private Direction m_moveOutDirection = Direction.Left;

		public Direction MoveOutDirection {
			get => m_moveOutDirection;
			set {
				m_moveOutDirection = value;
				m_dirty = true;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			if( m_customFromTo ) {
				return t.DOAnchorPos3D(start + m_moveTo, Duration).SetEase(Ease).SetDelay(Delay).ChangeStartValue(start + m_moveFrom);
			}

			Vector2 endPosition = start;
			var size = t.rect.size;
			switch( MoveOutDirection ) {
				case Direction.Left:
					endPosition.x -= size.x;
					break;
				case Direction.Top:
					endPosition.y += size.y;
					break;
				case Direction.Right:
					endPosition.x += size.x;
					break;
				case Direction.Bottom:
					endPosition.y -= size.y;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return t.DOAnchorPos(endPosition, Duration).SetDelay(Delay).SetEase(Ease);
		}
	}
}