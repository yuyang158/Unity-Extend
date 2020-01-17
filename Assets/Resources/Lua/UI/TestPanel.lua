---@class UI.TestPanel
---@field slot CS.Extend.AssetService.AssetReference @CS.UnityEngine.Sprite
---@field slots UI.ItemSlot[]
---@field txt CS.UnityEngine.UI.Text
---@field btn CS.UnityEngine.UI.Button
---@field num number
---@field str string
---@field stateSwitcher CS.Extend.Switcher.StateSwitcher

local LuaSM = require('ServiceManager')

local M = class()
local binding = require("mvvm.binding")
local SprotoClient = require("sproto.SprotoClient")
-- local AssetService = CS.Extend.AssetService.AssetService

function M:ctor()
end

function M:awake()
    self.mvvmBinding = self.__CSBinding:GetComponent(typeof(CS.Extend.LuaMVVM.LuaMVVMBinding))
    self.sprotoClient = SprotoClient.new("Config/c2s", "Config/s2c")
    self.sprotoClient:Connect("45.77.33.200", 4445)
end

function M:destroy()
    self.sprotoClient = nil
    local tick = LuaSM.GetService(LuaSM.SERVICE_TYPE.TICK)
    tick.Unregister(M.Tick, self)
end

function M:start()
    self.vm = {
        data = {
            text = "1",
            toggle = true,
            a = 1,
            b = 5,
            c = {
                d = 20
            },
            items = {
                { sprite = "Sprites/red", count = "1" },
                { sprite = "Sprites/green", count = "2" },
                { sprite = "Sprites/red", count = "3" }
            }
        },
        computed = {
            sum = function(data)
                return string.format("sum : %.2f", data.a + data.b + data.c.d)
            end
        }
    } 
    binding.build(self.vm)
    self.mvvmBinding:SetDataContext(self.vm)

    ---@type TickService
    local tick = LuaSM.GetService(LuaSM.SERVICE_TYPE.TICK)
    tick.Register(M.Tick, self)
end

function M:Tick(deltaTime)
    self.vm.c.d = self.vm.c.d + deltaTime
end

function M:OnClick()
    self.vm.text = tostring(math.tointeger(self.vm.text) + 1)
    self.vm.toggle = not self.vm.toggle
    self.vm.items[2].count = math.random(1, 10)
    self.vm.a = self.vm.a + 1
    -- self.vm.c.d = math.random(100, 1000) / 73

    self.sprotoClient:Send("set", {
        what = "abc",
        value = tostring(self.vm.c.d)
    })
    self.sprotoClient:Send("get", {what="abc"}, function(args)
        table.print_r(args)
    end)
end
return M