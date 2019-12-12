---@class UI.TestPanel
---@field slots UI.ItemSlot[]
---@field txt CS.UnityEngine.UI.Text
---@field btn CS.UnityEngine.UI.Button
---@field num number
---@field str string
---@field stateSwitcher CS.Extend.Switcher.StateSwitcher
local M = class()

function M:ctor()
    
end

function M:awake()
    self.data = {
        text = "1",
        toggle = true
    }
    self.__CSBinding:SetDataContext(self.data)
end

function M:OnClick(evt)
    local meta = getmetatable(self.data)
    assert(meta and meta.__newindex)
    self.data.text = tostring(math.tointeger(self.data.text) + 1)
    self.data.toggle = not self.data.toggle
end
return M