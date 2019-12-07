using System.Collections;
using UnityEngine;

namespace Extend.Common {
	public class GlobalCoroutineRunnerService : IService {
		public CSharpServiceManager.ServiceType ServiceType => CSharpServiceManager.ServiceType.COROUTINE_SERVICE;
		private CSharpServiceManager service;
		public void Initialize() {
			service = GameObject.Find("CSharpServiceManager").GetComponent<CSharpServiceManager>();
		}

		public Coroutine StartCoroutine(IEnumerator enumerator) {
			return service.StartCoroutine(enumerator);
		}

		public void Destroy() {
			
		}
	}
}