
# Unity-Extend

## 功能

1. 通过分析注释内容(emmylua)反射lua变量，Unity组件（单个、数组）、string、number、integer、boolean，相关文件：

* LuaBinding.cs
* Resources/Lua/UI/TestPanel.lua
* Resources/Lua/UI/ItemSlot.lua

2. Unity UI与Lua Table的绑定

* Assets\Extend\Example\Resources\Lua\mvvm.lua.txt
* BindingDemo场景中MVVM开头相关节点
* 核心功能文件tracedoc.lua.txt参考：[tracedoc](https://blog.codingnow.com/2017/02/tracedoc.html)

3. 配置文件读取

* 读取以\t为分隔符的csv文件
* 格式：第一行为Key，第二行为该列类型，第三行为列描述
* 支持类型 int number string json link boolean
* link类型为外链id，可以在lua中直接访问对应链接表
* 相关实现代码为Example/Resources/Lua/ConfigService.lua

4. AssetBundle打包（开发中）
* 手动强制指定文件夹AssetBundleName
* 自动分析资源依赖，去除依赖短链
* 自动生成文件位置描述

## Demo查看方法

将Extend目录拷贝到xLua\Assets目录即可
