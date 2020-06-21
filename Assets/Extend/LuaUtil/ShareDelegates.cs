using Extend.Asset;
using Extend.Network.SocketClient;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public delegate LuaTable GetLuaService(int index);

	[CSharpCallLua]
	public delegate LuaTable LuaBindingClassNew(GameObject go);

	[CSharpCallLua]
	public delegate void LuaUnityEventFunction(LuaTable self);

	[CSharpCallLua]
	public delegate void WatchCallback(LuaTable self, object val);

	[CSharpCallLua]
	public delegate void WatchLuaProperty(LuaTable self, string path, WatchCallback callback);

	[CSharpCallLua]
	public delegate void DetachLuaProperty(LuaTable self, string path, WatchCallback callback);

	[CSharpCallLua]
	public delegate void NoneEventAction(LuaTable t, PointerEventData data);

	[CSharpCallLua]
	public delegate void IntEventAction(LuaTable t, PointerEventData data, int val);

	[CSharpCallLua]
	public delegate void FloatEventAction(LuaTable t, PointerEventData data, float val);

	[CSharpCallLua]
	public delegate void StringEventAction(LuaTable t, PointerEventData data, string val);

	[CSharpCallLua]
	public delegate void AssetEventAction(LuaTable t, PointerEventData data, AssetReference val);

	[CSharpCallLua]
	public delegate void PropertyChangedAction(Component sender, object value);

	[CSharpCallLua]
	public delegate void OnSocketStatusChanged(LuaTable self, AutoReconnectTcpClient.Status status);
	
	[CSharpCallLua]
	public delegate void OnRecvData(LuaTable self, byte[] data);

	[CSharpCallLua]
	public delegate void GlobalEventCallback();
}