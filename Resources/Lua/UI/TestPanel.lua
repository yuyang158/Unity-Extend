---@class UI.TestPanel
---@field slot CS.Extend.AssetService.AssetReference
---@field slots UI.ItemSlot[]
---@field txt CS.UnityEngine.UI.Text
---@field img CS.UnityEngine.UI.Image
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
    local typ = typeof(CS.UnityEngine.Sprite)
    self.slot:LoadAsync(typ):OnComplete("+", function()
        local sprite = self.slot:GetSprite()
        print(sprite)
        self.img.sprite = sprite
    end)
    
    local meta = getmetatable(self.data)
    assert(meta and meta.__newindex)
    self.data.text = tostring(math.tointeger(self.data.text) + 1)
    self.data.toggle = not self.data.toggle
end
return M