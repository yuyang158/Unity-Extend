using UnityEngine;

namespace Extend.LuaMVVM {
	[RequireComponent(typeof(Animator))]
	public class LuaMVVMAnimator : MonoBehaviour {
		private Animator m_animator;

		private int m_nameHash;

		[SerializeField]
		private string m_paramName;

		private void Awake() {
			m_animator = GetComponent<Animator>();
			m_nameHash = Animator.StringToHash(m_paramName);
		}

		public int IntValue {
			get => m_animator.GetInteger(m_nameHash);
			set => m_animator.SetInteger(m_nameHash, value);
		}

		public float FloatValue {
			get => m_animator.GetFloat(m_nameHash);
			set => m_animator.SetFloat(m_nameHash, value);
		}

		public bool BoolValue {
			get => m_animator.GetBool(m_nameHash);
			set => m_animator.SetBool(m_nameHash, value);
		}
	}
}