using System.Collections;
using Extend.Common.Lua;
using UnityEngine;

namespace Extend.Common {
	[LuaCallCSharp]
	public class GlobalCoroutineRunnerService : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.COROUTINE_SERVICE;
		private CSharpServiceManager m_service;
		public void Initialize() {
			m_service = CSharpServiceManager.Instance;
		}

		public Coroutine StartCoroutine(IEnumerator enumerator) {
			return m_service.StartCoroutine(enumerator);
		}

		public void StopCoroutine(Coroutine co) {
			m_service.StopCoroutine(co);
		}

		public void StopAllCoroutines() {
			m_service.StopAllCoroutines();
		}

		public void Destroy() {
			StopAllCoroutines();
		}
	}
}