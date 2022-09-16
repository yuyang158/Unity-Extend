using UnityEngine;
using System.Runtime.InteropServices;

namespace ToastPlugin {
	public static class ToastHelper {
#if UNITY_IOS
		[DllImport ("__Internal")]
		private static extern void showToast(string msg, bool isLong);
#endif

		/// <summary>
		/// Show a Toast message from Android.
		/// </summary>
		/// <param name="toastMsg">the message you want to show</param>
		/// <param name="isLong">does the message appear for a short of long amount of time (time is default from android)</param>
		public static void ShowToast(string toastMsg, bool isLong = false) {
#if UNITY_EDITOR
			Debug.Log(toastMsg);
#elif UNITY_ANDROID
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
			currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				Toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, toastMsg, Toast.GetStatic<int>("LENGTH_LONG")).Call("show");
			}));
#elif UNITY_IOS
			showToast(toastMsg, isLong);
#endif
		}

		/// <summary>
		/// Get the unity activity for context. 
		/// </summary>
		/// <returns></returns>
		private static AndroidJavaObject getActivity() {
			var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject act = actClass.GetStatic<AndroidJavaObject>("currentActivity");
			return act;
		}
	}
}