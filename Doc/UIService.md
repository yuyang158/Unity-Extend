# UIService

## 作用
管理UI、提供UI配置、加载、卸载、层级管理

## 模块

### 配置模块UI View List窗口

* 通过Unity菜单Window/UIView Window打开
* 配置项介绍：

|Name|UI View|Background Fx|Full Screen|Attach Layer|Transition|Close|
|:---|:---|:---|:---|:---|:---|:---|
|名称可用于显示隐藏界面|界面Prefab|背景Prefab|是否为全屏|UI添加到的层级|过渡UI|关闭方法|

* 特殊说明
  * 第一列的Error图标为当前检查到的错误，如果你配置的行包含Error图标则表示你的配置有误
  * Full Screen ：全屏界面会导致其下层界面隐藏以节省性能
  * Close : 目前支持以下几种方式：
    * AnyWhere： 点击任意位置关闭
    * Outside：点击外面关闭
    * Button：点击关闭按钮关闭

### C#端

#### 动画组件

* UIViewDoTween : 通过DoTween支持的显示隐藏动画
* UIViewAnimator : 通过Animator支持的显示隐藏动画
  * Animator中需包含Trigger：Show，Hide
  * Show，Hide动画需在结束时调用OnEvent，参数为string，内容为：Finish
  * Animator中可选Trigger：Loop
* UIViewTimeline : 需赋值不同的PlayableDirector
* UIViewCompound : 上面三个动画的组合

#### 动画事件
* Showing : 开始显示动画事件
* Shown : 显示动画结束事件
* Hiding : 开始隐藏动画事件
* Hidden : 隐藏动画结束事件

**！所有动画回调均不需要删除，C#端会在调用后自动清除**


### Lua端

UIService.lua
```lua
local SM = require("ServiceManager")
local UIService = SM.GetService(SM.SERVICE_TYPE.UI)
```

1. 仅加载界面，不显示

```lua
local context = UIService.Load("name", function(err, go)
end)

-- some code
context:Show()
```

2. 显示界面
```lua
local context = UIService.Show("name", function(err, go)

end)
```

3. 隐藏界面
* 方式1 : UIService关闭 + 卸载资源
```lua
UIService.Hide("name")
```

* 方式2 : Context关闭 + 卸载资源
```lua
context:Close()
```

* 方式3 : Context关闭，不卸载资源，适用于界面内反复显示的小窗口，例如物品的Tip等。
```lua
context:Hide()
```
