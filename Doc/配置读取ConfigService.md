# 配置读取ConfigService说明

- [配置读取ConfigService说明](#配置读取configservice说明)
	- [功能概述](#功能概述)
	- [支持类型](#支持类型)
	- [具体使用](#具体使用)
		- [表结构](#表结构)
			- [TableA](#tablea)
			- [TableB](#tableb)
		- [Lua使用代码](#lua使用代码)
	- [进阶用法](#进阶用法)
		- [表的后处理](#表的后处理)

## 功能概述
为Lua提供读取配置表相关功能

## 支持类型
1. int : 整数类型
2. number ：数值类型，常用于浮点数
3. string : 文本类型
4. json : Lua表类型
5. link ：外链类型，例如卡牌表引用天赋表
6. linkjson : 数组型的外链
7. boolean : 布尔类型
8. translate ： 多语言类型，使用上与string类型无差别
9. asset : AssetReference资源

## 具体使用

### 表结构

#### TableA
| Id  | Value  | Description | TableB      | ComplexValue |
| --- | ---    | ------      | --------    | ----------   |
| int | number | string      | link        | json         |
| 1   | 122.5  | abcd        | 101         | {"id" = 1001, "count" = 5} |

#### TableB

| Id  | SomeValue  |
| --- | ---        |
| int | string     |
| 101 | ABCD       |

### Lua使用代码
```LUA
local SM = require("ServiceManager")
local ConfigService = SM.GetService(SM.SERVICE_TYPE.CONFIG)

--- 获取整张表，用于遍历等
local tB = ConfigService.GetConfig("TableB")

--- 获取某一行
local tARow = ConfigService.GetConfigRow("TableA", 1)
print(tARow.TableB.SomeValue) -- 输出ABCD
print(tARow.ComplexValue.id) -- 输出1001
```

## 进阶用法

### 表的后处理
对于一些表，我们可能需要在运行时进行处理后才能正常使用，例如策划配置了奖励信息。但是其中有一些冗余的配置。可以使用后处理将冗余替换掉

```LUA
local SM = require("ServiceManager")
local ConfigService = SM.GetService(SM.SERVICE_TYPE.CONFIG)

ConfigService.RegisterPostProcess("ActivityAward", "parsedAwards", function(row)
	local parsedAwards = {}
	for _, award in pairs(row.inevitable) do
		if award.itemId ~= 0 then
			t_insert(parsedAwards, {itemId = award.itemId, itemNum = award.itemNum})
		end
	end
	return parsedAwards
end)
```