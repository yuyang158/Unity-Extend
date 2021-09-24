# LuaBinding

## 优势
* 沟通Lua与C#,提供MonoBehaviour各种时机到Lua。例如：awake、start、destroy等
* 反射Lua变量提供类MonoBehaviour的数据绑定
* 反射Lua中的函数以供事件回调
* 反射DEBUG开头的函数到Inspector，供运行时调用

## 默认设置参数
1. __CSBinding LuaBinding 组件自身，用于Lua测更方便的获取其他组件等操作
2. name 绑定Lua文件的名字，例如UILogin
3. fullname 绑定Lua文件的全路径，例如UI.UILogin

## Demo
Assets/Scene/Editor/Demo/LuaBindingDemo

## 其他
* Sync按钮：可以在运行时动态修改的Inspector参数强制同步到Lua
* 重新加载Lua文件：强制重新解析Lua文件
* 在编辑器中打开：支持Rider或vscode在编辑器打开对应Lua文件

## 相关文件
### 运行时

* Assets\Extend\LuaBinding.cs
* Assets\Extend\LuaBindingData
* Assets\Extend\LuaBindingEvent

### 编辑器

* Assets\Extend\Editor\LuaBindingEditor.cs
* Lua注释分析 Assets\Extend\Editor\LuaClassEditorFactory.cs
