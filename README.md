- [功能](#功能)
  - [通过分析注释内容(emmylua)反射lua变量](#通过分析注释内容emmylua反射lua变量)
  - [Unity UI与Lua Table的绑定,整体参考vue](#unity-ui与lua-table的绑定整体参考vue)
  - [配置文件读取](#配置文件读取)
  - [AssetBundle打包](#assetbundle打包)
  - [网络模块](#网络模块)
  - [通过Attribute扩展Editor编辑功能](#通过attribute扩展editor编辑功能)
  - [基于ScriptableObject的事件功能](#基于scriptableobject的事件功能)
  - [In Game Console功能](#in-game-console功能)

# 功能

## 通过分析注释内容(emmylua)反射lua变量

![alt text](https://github.com/yuyang158/Unity-Extend/raw/master/ReadMeImage/LuaBinding.png "")
* LuaBinding.cs
* 支持类型：Unity组件（单个、数组）、string、number、integer、boolean

## Unity UI与Lua Table的绑定,整体参考vue

* LuaMVVMBinding.cs LuaMVVMForEach.cs
* mvvm.lua
* BindingDemo场景中MVVM开头相关节点

## 配置文件读取

* 读取以\t为分隔符的tsv文件
* 格式：第一行为Key，第二行为该列类型，第三行为列描述
* 支持类型 int number string json link boolean
* link类型为外链id，可以在lua中直接访问对应链接表
* 相关实现代码为Example/Resources/Lua/ConfigService.lua

## AssetBundle打包

* 手动强制指定文件夹AssetBundleName
* 自动分析资源依赖，去除依赖短链
* 自动生成文件位置描述
* 自动生成更新描述文件

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
