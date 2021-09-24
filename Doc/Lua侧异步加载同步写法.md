# Lua侧异步加载同步写法

## 实现原理

通过Lua的协程（coroutine）功能实现了lua侧类同步写法（实际上还是异步）

## 示例代码

```LUA
local CoroutineAssetLoader = require("base.asset.CoroutineAssetLoader")
---@type base.asset.CoroutineAssetLoader
CoroutineAssetLoader.Create(function(procedural, aaa, bbb)
	local matAssetRef = procedural:LoadAsset("Mat_Cow", typeof(CS.UnityEngine.Material))
	local material = matAssetRef:GetMaterial()
	print(material.name)
	matAssetRef:Dispose()
	local gameObjects = procedural:InstantiatePrefab("Buildings/House01/Prefabs/house01_tell03", 10)
	for _, v in ipairs(gameObjects) do
		print(v.name)
	end
end)
```