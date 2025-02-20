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
		
		public int ServiceType => (int)CSharpServiceManager.ServiceType.COROUTINE_SERVICE;
		private CSharpServiceManager m_service;
		public void Initialize() {
			m_service = CSharpServiceManager.Instance;
		}

		private MonoBehaviour m_stateCoroutine;
		public void SetStateCoroutineBehavior(MonoBehaviour coroutine) {
			m_stateCoroutine = coroutine;
		}
		
		public Coroutine StartStateCoroutine(IEnumerator enumerator) {
			return m_stateCoroutine.StartCoroutine(enumerator);
		}
		
		public Coroutine StartBindingCoroutine(MonoBehaviour binding, IEnumerator enumerator) {
			return binding.StartCoroutine(enumerator);
		}

		public void StopBindingCoroutine(MonoBehaviour binding, Coroutine coroutine) {
			binding.StopCoroutine(coroutine);
		}

		public Coroutine StartCoroutine(IEnumerator enumerator) {
			return m_service.StartCoroutine(enumerator);
		}

		public void StopCoroutine(Coroutine co) {
			m_service.StopCoroutine(co);
		}

		public void StopAllCoroutines() {
			if(!m_service) return;
			m_service.StopAllCoroutines();
		}

		private IEnumerator WaitSecond(WaitForSeconds second, Action callback) {
			yield return second;
			callback();
		}
		
		private IEnumerator _WaitEndOfFrame(Action callback) {
			var wait = new WaitForEndOfFrame();
			yield return wait;
			callback();
		}

		public Coroutine WaitSecond(float second, Action callback) {
			var wait = new WaitForSeconds(second);
			return StartCoroutine(WaitSecond(wait, callback));
		}
		
		public Coroutine WaitEndOfFrame(Action callback) {
			return StartCoroutine(_WaitEndOfFrame(callback));
		}

		public void StopCoroutineLua(Coroutine co) {
			StopCoroutine(co);
		}

		[BlackList]
		public void Destroy() {
			StopAllCoroutines();
		}
	}
}