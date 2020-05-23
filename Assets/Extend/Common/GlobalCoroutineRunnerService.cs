using System;
using System.Collections;
using UnityEngine;
using XLua;

namespace Extend.Common {
	[LuaCallCSharp]
	public class GlobalCoroutineRunnerService : IService {
		public static GlobalCoroutineRunnerService Get() {
			return CSharpServiceManager.Get<GlobalCoroutineRunnerService>(CSharpServiceManager.ServiceType.COROUTINE_SERVICE);
		}
		
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

		private IEnumerator WaitSecond(WaitForSeconds second, Action callback) {
			yield return second;
			callback();
		}

		public void WaitSecond(float second, Action callback) {
			var wait = new WaitForSeconds(second);
			StartCoroutine(WaitSecond(wait, callback));
		}

		[BlackList]
		public void Destroy() {
			StopAllCoroutines();
		}
	}
}