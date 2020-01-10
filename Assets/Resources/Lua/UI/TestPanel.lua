---@class UI.TestPanel
---@field slot CS.Extend.AssetService.AssetReference @CS.UnityEngine.Sprite
---@field slots UI.ItemSlot[]
---@field txt CS.UnityEngine.UI.Text
---@field btn CS.UnityEngine.UI.Button
---@field num number
---@field str string
---@field stateSwitcher CS.Extend.Switcher.StateSwitcher

local M = class()
local mvvm = require("mvvm/mvvm")
-- local AssetService = CS.Extend.AssetService.AssetService

function M:ctor()
end

function M:awake()
    self.mvvmBinding = self.__CSBinding:GetComponent(typeof(CS.Extend.LuaMVVM.LuaMVVMBinding))
end

function M:start()
    self.vm = {
        data = {
            text = "1",
            toggle = true,
            a = 1,
            b = 5,
            items = {
                { sprite = "Sprites/red", count = "1" },
                { sprite = "Sprites/green", count = "2" },
                { sprite = "Sprites/red", count = "3" }
            }
        },
        computed = {
            sum = function(data)
                return data.a + data.b
            end
        }
    } 
    local vm, context = mvvm.BuildDataSource(self.vm)
    self.vm = vm
    self.mvvmBinding:SetDataContext(context)
end

function M:OnClick()
    self.vm.text = tostring(math.tointeger(self.vm.text) + 1)
    self.vm.toggle = not self.vm.toggle
    self.vm.items[2].count = math.random(1, 10)
    self.vm.a = self.vm.a + 1
end
return M