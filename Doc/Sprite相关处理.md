# Sprite相关处理

## RendererSpriteAssetAssignment 组件

### 作用

为SpriteRenderer中的sprite属性赋值，方便通过路径加载sprite资源。

### 方法

绑定组件，设置属性SpritePath

## ImageSpriteAssetAssignment 组件

### 作用

为UI的Image组件中的sprite属性赋值，方便通过路径加载sprite资源。

### 方法

绑定组件，设置属性SpritePath

## IconAssetAssignment 组件

### 作用

为UI的Image组件中的sprite属性赋值，方便通过路径加载sprite资源。减少DrawCall
只能加载Assets\Resources\UI\Icon下经特殊转换的资源

### 方法

绑定组件，设置属性IconPath