# LoopScrollRect使用介绍

## 优势
* 降低初始化时实例化个数，减少卡顿(可和PoolCacheGo结合使用)

## 使用方法
* 添加LoopVerticalScrollRect或者LoopHorizontalScrollRect组件，添加LuaMVVMLoopScroll组件
* 将滑动的prefab与LoopScrollRect的CellAsset关联起来
* 绑定逻辑与LuaMVVMForEach和LuaMVVMSystemScroll一致

## DEMO
* Assets/Scene/Editor/Demo/LoopScrollRect.unity