using System;
using DG.Tweening;
using Extend.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extend.UI.Animation {
	[Serializable]
	public class MoveInAnimation : StateAnimation {
		public enum Direction {
			Left,
			Top,
			Right,
			Bottom
		}

		[SerializeField, LabelText("Move From")]
		private Direction m_moveInDirection = Direction.Left;

		public Direction MoveInDirection {
			get => m_moveInDirection;
			set {
				m_moveInDirection = value;
				dirty = true;
			}
		}

		protected override Tween DoGenerateTween(RectTransform t, Vector3 start) {
			Vector2 startPosition =  start;
			var size = t.rect.size;
			Vector2 position = start;
			switch( MoveInDirection ) {
				case Direction.Left:
					startPosition.x -= size.x;
					break;
				case Direction.Top:
					startPosition.y += size.y;
					break;
				case Direction.Right:
					startPosition.x += size.x;
					break;
				case Direction.Bottom:
					startPosition.y -= size.y;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return t.DOAnchorPos(position, Duration).SetDelay(Delay).SetEase(Ease).ChangeStartValue(startPosition);
		}
	}
}