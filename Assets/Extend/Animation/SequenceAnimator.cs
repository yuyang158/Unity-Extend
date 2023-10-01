using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using XLua;

namespace Extend.Animation {
	[CreateAssetMenu(menuName = "Sequence/Animator", fileName = "Sequence Animator.asset"), LuaCallCSharp]
	public class SequenceAnimator : ScriptableObject {
		[Serializable]
		public class SequenceAnimation {
			[SerializeField]
			private string m_name;

			[SerializeField]
			private bool m_loop = true;

			public string Name => m_name;
			public bool Loop => m_loop;
		}

		public SequenceAnimation[] Animations;
		public string DefaultAnimation;
		public SpriteAtlas Atlas;

		private readonly Dictionary<string, SequenceAnimation> m_animationTypeNameMap =
			new Dictionary<string, SequenceAnimation>();

		private readonly Dictionary<string, Sprite> m_sprites = new Dictionary<string, Sprite>();

#if UNITY_EDITOR
		private void OnEnable() {
			Application.quitting += OnExit;
		}

		private void OnExit() {
			foreach( KeyValuePair<string, Sprite> keyValuePair in m_sprites ) {
				DestroyImmediate(keyValuePair.Value);
			}
			m_sprites.Clear();
		}

		private void OnDisable() {
			Application.quitting -= OnExit;
		}
#endif

		public Sprite GetSprite(string spriteName) {
			if( m_sprites.Count != Atlas.spriteCount ) {
				var sprites = new Sprite[Atlas.spriteCount];
				Atlas.GetSprites(sprites);
				foreach( Sprite s in sprites ) {
					m_sprites.Add(s.name[..^7], s);
				}
			}

			m_sprites.TryGetValue(spriteName, out var sprite);
			return sprite;
		}

		private void PreProcess() {
			m_animationTypeNameMap.Clear();
			foreach( SequenceAnimation animation in Animations ) {
				var index = animation.Name.IndexOf('_');
				var typeName = animation.Name[( index + 1 )..];
				m_animationTypeNameMap.Add(typeName, animation);
			}
		}

		private void OnDestroy() {
			foreach( KeyValuePair<string, Sprite> keyValuePair in m_sprites ) {
				DestroyImmediate(keyValuePair.Value);
			}

			m_sprites.Clear();
		}

		public SequenceAnimation FindAnimation(string animationName) {
			if( Animations.Length != m_animationTypeNameMap.Count ) {
				PreProcess();
			}

			m_animationTypeNameMap.TryGetValue(animationName, out var animation);
			return animation;
		}
	}
}
