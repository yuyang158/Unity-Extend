
# xLua-Extend

## 功能

1. 自动向Lua实例中绑定Unity组件，相关文件：

* LuaBinding.cs
* Example\Resources\Lua\test_binding.lua.txt

2. Unity UI与Lua Table的绑定

* Assets\Extend\Example\Resources\Lua\mvvm.lua.txt
* BindingDemo场景中MVVM开头相关节点
* 核心功能文件tracedoc.lua.txt参考：[tracedoc](https://blog.codingnow.com/2017/02/tracedoc.html)

3. 配置文件读取

* 读取以\t为分隔符的csv文件
* 格式：第一行为Key，第二行为该列类型，第三行为列描述
* 支持类型 int number string json link
* link类型为外链id，可以在lua中直接访问对应链接表
* 相关实现代码为Example/Resources/Lua/ConfigService.lua

## Demo查看方法

将Extend目录拷贝到xLua\Assets目录即可
