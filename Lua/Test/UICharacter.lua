---@class Test.UICharacter : LuaBinding
local M = class()
local LuaMVVMBindingType = typeof(CS.Extend.LuaMVVM.LuaMVVMBinding)
local binding = require("mvvm.binding")
local SM = require "ServiceManager"

function M:awake()
	local vm = {
		data = {
			name = "Jack",
			id = math.random(100000),
			level = math.random(100),
			title = (math.random(100) % 2 == 0) and "None" or "ASDF",
			guildName = "Top 1",
			camp = "123123123",
			friend = "Test",
			exp = 300,
			levelExp = 900,
			power = math.random(123123),
			equipLevel = math.random(999),
			basicProp = {
				value1 = math.random(999),
				value2 = math.random(999),
				value3 = math.random(999),
				value4 = math.random(999)
			}
		}
	}

	binding.build(vm)
	local b = self.__CSBinding:GetComponent(LuaMVVMBindingType)
	b:SetDataContext(vm)
end

function M:ShowHeroView()
	local uiService = SM.GetService(SM.SERVICE_TYPE.UI)
	uiService.Show("HeroView", function(viewGO)
		local mvvmBinding = viewGO:GetComponent(LuaMVVMBindingType)
		local vm = {
			data = {
				heroes = {
					{name = "aaaa", icon = "Sprites/red"},
					{name = "bbb", icon = "Sprites/green"},
					{name = "cccc", icon = "Sprites/red"},
					{name = "ddddd", icon = "Sprites/red"},
					{name = "eeeee", icon = "Sprites/green"},
					{name = "fff", icon = "Sprites/red"},
					{name = "ggg", icon = "Sprites/green"},
					{name = "h", icon = "Sprites/green"},
					{name = "ii", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"},
					{name = "jjjjj", icon = "Sprites/red"}
				}
			}
		}
		binding.build(vm)
		mvvmBinding:SetDataContext(vm)
	end)
end

return M