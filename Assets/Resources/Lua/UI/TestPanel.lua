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
            items = {
                { sprite = "Sprites/red", count = "1" },
                { sprite = "Sprites/green", count = "2" },
                { sprite = "Sprites/red", count = "3" }
            }
        }
    } 
    mvvm.BuildDataSource(self.vm)
    self.mvvmBinding:SetDataContext(self.vm)
end

function M:OnClick()
    self.vm.text = tostring(math.tointeger(self.data.text) + 1)
    self.vm.toggle = not self.vm.toggle
    self.vm.items[2].count = tostring(math.random(1, 10))
end
return M