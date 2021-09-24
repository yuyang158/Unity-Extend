# TickService Lua

## 功能

提供组件的Update、提供类Timer的倒计时功能

## 具体使用

### Update注册与反注册
```lua
local SM = require("ServiceManager")
local TickService = SM.GetService(SM.SERVICE_TYPE.TICK)

function M:Tick(deltaTime)
    
end

-- 一个Register必须对应一个Unregister
TickService.Register(M.Tick, self)
TickService.Unregister(M.Tick, self)
```

### Timeout使用
创建一个不停循环的Timer
```lua
local SM = require("ServiceManager")
local TickService = SM.GetService(SM.SERVICE_TYPE.TICK)

TickService.Timeout(10, -1, function()
    
end)
```

打断循环（例如在切换界面等时候）
1. 方法1, 调用返回的方法
```lua
local interrupt = TickService.Timeout(10, -1, function()

end)
interrupt()
```
2. 方法二，在触发函数中回调true
```lua
TickService.Timeout(10, -1, function()
    return true
end)
```