using Extend.Common;
using UnityEngine;
using XiaoIceland.Service;

namespace Extend {
	public class AppStartUp : MonoBehaviour {
		private void Start() {
			VersionService service = new VersionService();
			service.Initialize();
			service.StartLua();
		}
	}
}