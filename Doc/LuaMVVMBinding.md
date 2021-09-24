# LuaMVVMBinding使用介绍

## 设计模式
[设计模式介绍](https://zhuanlan.zhihu.com/p/38108311)

## 优势
* 可视化Lua变量及组件属性绑定
* 组件通过Unity GUID关联而非路径，轻松应对UI结构变化
* 支持短Lua expression绑定，更加灵活
* 减少C#与Lua间的相互通信，提升性能

## 使用方法
1. GameObject上添加LuaMVVMBinding组件
2. 组件中依次添加需要绑定的组件
3. 设置绑定组件对应的属性，例如文本、文本颜色等
4. 设定绑定逻辑
   * ONE_WAY: 将Lua变量赋值给C#组件属性，并对Lua变量变化进行监听
   * TWO_WAY: 将Lua变量赋值给C#组件属性。在Lua变量变化时对C#组件属性进行更形，C#组件属性变化时对Lua变量进行更新。
      * C#属性变化需要使用如下方式才能实现
      * 实现IUnityPropertyChanged接口并派生MonoBehaviour
      * 将实现的类添加到C#组件所在GameObject
   * ONE_WAY_TO_SOURCE: 将C#组件属性值赋值给Lua变量。C#组件属性变化时对Lua变量进行更新（更新要求同上）。
   * ONE_TIME: 将Lua变量赋值给C#组件属性。
   * EVENT: 将Lua函数绑定至C#事件。
      * 实现本功能需要添加组件并派生自LuaBindingEventBase，在合适的时机触发TriggerPointerEvent
      * 目前已经有的事件
        1. LuaBindingClickEvent 点击事件
        2. LuaBindingDragEvent 拖拽事件
        3. LuaBindingUpDownMoveEvent 按下抬起移动事件
        4. LuaMVVMLoopScroll LoopScroll到底事件
        5. LuaMVVMSystemScroll 内置Scroll事件
    
5. 绑定变量名，右侧括号位所填内容是否位expression。如果为expression，里面的内容等价于Lua函数：
```lua
-- this为数据绑定的根节点， current为ForEach类型的当前节点
function Expression(this, current)
    
end
```

## 其他辅助组件
### LuaMVVMForEach 
#### 功能

1. 对于节点下复数个LuaMVVMBinding，依次赋值
2. 在节点下没有节点时，异步创建Asset对应Prefab并赋值对应Lua数组中的值

### LuaMVVMLoopScroll

类似LuaMVVMForEach但针对于LoopScroll

### LuaMVVMSystemScroll

类似LuaMVVMForEach但针对于Scroll

## Demo

Assets/Scene/Editor/Demo/LuaMVVMBinding.unity