- [功能](#功能)
  - [通过分析注释内容(emmylua)反射lua变量](#通过分析注释内容emmylua反射lua变量)
  - [Unity UI与Lua Table的绑定,整体参考vue](#unity-ui与lua-table的绑定整体参考vue)
  - [配置文件读取](#配置文件读取)
  - [AssetBundle打包](#assetbundle打包)
  - [网络模块](#网络模块)
  - [通过Attribute扩展Editor编辑功能](#通过attribute扩展editor编辑功能)
  - [基于ScriptableObject的事件功能](#基于scriptableobject的事件功能)
  - [In Game Console功能](#in-game-console功能)
  - [远程Lua指令执行](#远程lua指令执行)
  - [Mock功能](#mock功能)
  - [LuaCheck集成](#luacheck集成)

# 功能

## 通过分析注释内容(emmylua)反射lua变量

![alt text](https://github.com/yuyang158/Unity-Extend/raw/master/ReadMeImage/LuaBinding.png "")
* LuaBinding.cs
* 支持类型：Unity组件（单个、数组）、string、number、integer、boolean

## Unity UI与Lua Table的绑定,整体参考vue

* LuaMVVMBinding.cs LuaMVVMForEach.cs
* BindingDemo场景中MVVM开头相关节点
```lua
local binding = require("mvvm.binding")
local a = binding.build({
	data = {
		a1 = 1,
		a2 = 2,
		a3 = "3"
	},
	computed = {
		c1 = function(this)
			return this.a1 + this.a2
		end
	}
})

local b = binding.build({
	mixins = {a},
	data = {
		b1 = 1,
		b2 = 2,
		b3 = "3"
	}
})
print(b.c1, b.b1)
```

## 配置文件读取

* 读取以\t为分隔符的tsv文件
* 格式：第一行为Key，第二行为该列类型，第三行为列描述
* 支持类型 int number string json link boolean AssetReference
* link类型为外链id，可以在lua中直接访问对应链接表
* AssetReference类型实际为Unity中资源的GUID，可通过Tools/Excel Asset Tool编辑
* 相关实现代码为 Lua/ConfigService.lua

## 资源加载

* 手动强制指定文件夹AssetBundleName
* 自动分析资源依赖，去除依赖短链
* 自动生成文件位置描述
* 自动生成更新描述文件
* CI：https://github.com/yuyang158/Unity-Extend/tree/master/Tools/Jenkins
* Lua、配置不放在Unity工程，增加迭代速度
* Editor使用AssetDatabase，Runtime使用AssetBundle自动切换
* 异步加载，加载流量控制

## 网络模块

* 接入sproto协议
* 支持rpc
* 支持断线自动重连
* 示例中连接到测试skynet服务器，服务器为Server文件夹

## 通过Attribute扩展Editor编辑功能

![alt text](https://github.com/yuyang158/Unity-Extend/raw/master/ReadMeImage/AttributeExample.png "示例图片，详情参考AttributeExmple.cs")
* HideIf (可以设置某个变量为某个值时隐藏该属性)
* ShowIf (可以设置某个变量为某个值时显示该属性)
* LabelText (覆盖变量的DisplayLable)
* ReorderList (针对数组、List型变量优化为ReorderableList编辑，如上图D)
* Require (变量不可为空)
* AssetOnly (可快捷编辑上图E右侧画笔)
* AssetReferenceAssetType (Asset系统资源通过GUID连接)
* EnumToggleButtons (将枚举值并列为几个按钮，如上图Enum Value)
* Button (Method Attribute，可以在Inspector中生成一个该函数的按钮)

## 基于ScriptableObject的事件功能

* 基于ScriptableObject（EventInstance.cs）生成的文件作为事件参数，解决命名错误、多个重复事件的问题
* 全局事件功能（LuaGlobalEvent.cs），通过组件发送全局事件。可以在Lua、C#侧注册回掉相应。处理某些通用响应逻辑。

## In Game Console功能

* 在游戏客户端内接收Unity Log回掉，显示Log输出，方便在非编辑器状态下查看Log
* Error级Log自动弹出
* 指令输入功能
* 通过键盘上的上下键查看最近10次的输入记录
  
## 远程Lua指令执行

* 通过Http将需要执行的Lua代码上传到服务器（服务器代码目录：UnityRemoteCmd）
  1. http://127.0.0.1/?cmd=devices 获取所有在线设备
  2. http://127.0.0.1/?cmd=lua&device=604
       * Method: POST
       * Body: lua代码
       * Content-Type: text/plain
  3. http服务器将代码发送到客户端RemoteCmdClient执行
  4. 客户端调用Lua全局函数Global_DebugFunction处理发过来的代码
  5. 预览地址：http://private-tunnel.site:4888/index.html#/lua/lua

## Mock功能
* 通过拦截、分析客户端发送及收到的协议数据生成伪数据
* 功能代码文件：Assets/Resources/Lua/base/MockService.lua

## LuaCheck集成
* 选中Assets/Extend/Editor/LuaCheckSetting.asset 设置LuaCheck.exe的路径
* 在Lua文件保存时，自动执行LuaCheck指令，并将结果输出到Console
