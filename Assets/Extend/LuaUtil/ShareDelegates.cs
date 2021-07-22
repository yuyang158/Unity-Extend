using Extend.Asset;
using Extend.EventAsset;
using Extend.Network.SocketClient;
using UnityEngine;
using XLua;

namespace Extend.LuaUtil {
	[CSharpCallLua]
	public delegate LuaTable GetLuaService(int index);

	[CSharpCallLua]
	public delegate LuaTable LuaBindingClassNew(GameObject go);

	[CSharpCallLua]
	public delegate void LuaUnityEventFunction(LuaTable self);

	[CSharpCallLua]
	public delegate LuaTable LuaUnityReturnTableFunc(LuaTable self);

	[CSharpCallLua]
	public delegate void LuaUnityCollision2DEventFunction(LuaTable self, Collision2D collision);

	[CSharpCallLua]
	public delegate void WatchCallback(object val);

	[CSharpCallLua]
	public delegate void WatchLuaProperty(LuaTable self, string path, WatchCallback callback, bool expression);

	[CSharpCallLua]
	public delegate void DetachLuaProperty(LuaTable self, string path, WatchCallback callback);

	[CSharpCallLua]
	public delegate void NoneEventAction(LuaTable t, object data);

	[CSharpCallLua]
	public delegate void IntEventAction(LuaTable t, object data, int val);

	[CSharpCallLua]
	public delegate void FloatEventAction(LuaTable t, object data, float val);

	[CSharpCallLua]
	public delegate void StringEventAction(LuaTable t, object data, string val);

	[CSharpCallLua]
	public delegate void AssetEventAction(LuaTable t, object data, AssetReference val);

	[CSharpCallLua]
	public delegate void PropertyChangedAction(Component sender, object value);

	[CSharpCallLua]
	public delegate void OnSocketStatusChanged(LuaTable self, AutoReconnectTcpClient.Status status);

	[CSharpCallLua]
	public delegate void OnRecvData(LuaTable self, byte[] data);

	[CSharpCallLua]
	public delegate bool LuaEventCallback(EventInstance e);

	[CSharpCallLua]
	public delegate object GetGlobalVM(string path);

	[CSharpCallLua]
	public delegate object ComputeGet(string path);

	[CSharpCallLua]
	public delegate object GetLuaValue(LuaTable self);

	[CSharpCallLua]
	public delegate void SetupLuaNewClassCallback(LuaTable classMeta, LuaTable parentClassMeta);

	[CSharpCallLua]
	public delegate void BindingEventDispatch(int id, object eventData);

	[CSharpCallLua]
	public delegate void BindEvent(string eventName, GameObject go, LuaFunction func);
}