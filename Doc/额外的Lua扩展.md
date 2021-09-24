# 额外的Lua扩展

## 文本操作
1. string.replace 文本替换

```lua
string.replace("112233", "1", "4")
"442233"
```

2. string.replace_with_table 使用table进行替换
```lua
string.replace_with_table("text replace $w1, $w2", {w1 = 2, w2 = 4})
"text replace 2, 4"
```

3. string.format_text 策划表文本替换, 类似于C#的string.Format
```lua
string.format_text("text replace $parameter1$, $parameter2$, $parameter1$", {"A", "B"})
"text replace A, B, A"
```

4. string.split 文本切分类似于C# string.Split
```lua
string.split("C;D;E", ";")
{"C", "D", "E"}
```

5. string.splitToNumber 将切分后的结果转换为number
```lua
string.splitToNumber("1;2;3", ";")
{1, 2, 3}
```

6. string.fromhex 

7. string.tohex

## table操作

1. table.empty 返回table是否为空
```lua
table.empty({})
true
```

2. table.array_each 遍历数组
```lua
table.array_each({1, 2, 3}, function(element, index)
    
end)
```

2. table.table_each 遍历数组
```lua
table.table_each({a = 1, b = 2}, function(v, k)
    
end)
```

3. table.index_of 索引查找
```lua
table.index_of({1, 2, 3}, 2)
2
```

4. table.index_of_predict 带额外匹配方法的索引查找
```lua
table.index_of({1, 2, 3}, function(v)
    return v == 2
end)
2
```

5. table.swap_remove 无序数组删除方法，能用尽量用这个
```lua
table.swap_remove({4, 2, 3}, 1)
{2, 3}
```

6. table.count table元素数量
```lua
table.count({a = 1, b = 2})
2
```

7. table.sum 数组求和
```lua
table.sum({1, 2, 4})
table.sum({1, 2, 3, 4}, function(v, k)
    return v % 2 == 0 and 0 or v
end)

table.sum({{id = 1, count = 100}, {id = 2, count = 200}}, "count")
```

8. table.print_r Console中输出表内容
```lua
table.print_r({a = 1, b = 2})
```

9. table.assign 浅拷贝
```lua
local source = {c = 3}
table.assign(source, {a = 1, b = 2})
{a = 1, b = 2, c = 3}

local source = nil
table.assign(source, {a = 1, b = 2})
{a = 1, b = 2}
```

10. table.add_range 向数组插入另一个数组
```lua
table.add_range({1, 3, 5}, {2, 4})
{1, 3, 5, 2, 4}
```

## 数学库

1. math.round 四舍五入
```lua
math.round(5.4)
5
```