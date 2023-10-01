# Lua侧异步加载同步写法

## 实现原理

通过Lua的协程（coroutine）功能实现了lua侧类同步写法（实际上还是异步）

## 示例代码

```LUA
local CoroutineAssetLoader = require(" base.asset.CoroutineAssetLoader")
CoroutineAssetLoader.new(function(this)
    local gameObjects = this:InstantiatePrefab("XXX/ABC.prefab", 5)
    
    assert(#gameObjects == 5)
    local assetRef = this:LoadAsset("XXX/ABB.anim", "LoadAnimationClipAsync")
    local clip = assetRef:GetAnimationClip()
end)
```
