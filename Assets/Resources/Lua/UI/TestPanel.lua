---@class UI.TestPanel
---@field slot CS.Extend.AssetService.AssetReference @CS.UnityEngine.Sprite
---@field slots UI.ItemSlot[]
---@field txt CS.UnityEngine.UI.Text
---@field btn CS.UnityEngine.UI.Button
---@field num number
---@field str string
---@field stateSwitcher CS.Extend.Switcher.StateSwitcher
local M = class()
local mvvm = require("mvvm")
local AssetService = CS.Extend.AssetService.AssetService

function M:ctor()
end

function M:awake()
    self.data = {
        text = "1",
        toggle = true,
        items = {
            { sprite = mvvm.placeholder, count = "1" },
            { sprite = mvvm.placeholder, count = "2" },
            { sprite = mvvm.placeholder, count = "3" }
        }
    }

    self.mvvmBinding = self.__CSBinding:GetComponent(typeof(CS.Extend.LuaMVVM.LuaMVVMBinding))
    self.mvvmBinding:SetDataContext(self.data)
end

function M:OnClick()
    local meta = getmetatable(self.data)
    assert(meta and meta.__newindex)
    self.data.text = tostring(math.tointeger(self.data.text) + 1)
    self.data.toggle = not self.data.toggle
    self.data.items[2].count = tostring(math.random(1, 10))
end
return M