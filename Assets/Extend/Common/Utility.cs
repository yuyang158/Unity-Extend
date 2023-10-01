using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Extend.Common {
	public static class Utility {
		public static void HttpFileUpload(string url, NameValueCollection qs, string filePath) {
			var target = Application.persistentDataPath + "/upload.log";
			File.Copy(filePath, target, true);

			ThreadPool.QueueUserWorkItem((_) => {
				try {
					var client = new WebClient {QueryString = qs};
					var response = client.UploadFile(url, "POST", target);
					var resText = Encoding.UTF8.GetString(response);
					Debug.Log(resText);
				}
				catch( Exception e ) {
					Debug.LogError(e);
				}
				finally {
					File.Delete(target);
				}
			});
			
			//ToastHelper.ShowToast("Log Uploaded....");
		}
	}
}
