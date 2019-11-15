local mvvm = require("mvvm")
local doc = mvvm.new_doc("test", {text = "", toggle = false, gold = 10})
local time = CS.UnityEngine.Time
local LuaUtil = CS.XLua.LuaUtil

local SM = require "ServiceManager"

local M = class()
function M:ctor(go)
    self.go = go
    self.time = 0
    self.counter = 0
end

function M:awake()
    self.green = LuaUtil.UnityExtension4XLua.LoadSprite("green")
    self.red = LuaUtil.UnityExtension4XLua.LoadSprite("red")

    ---@type ConfigService
    local configService = SM.GetService(SM.SERVICE_TYPE.CONFIG)
    local rowData = configService.GetConfigRow("excel1", "E1_3")
    print(rowData.test1)
    print(rowData.test2)
    print(rowData.test3)
    print(rowData.excel2.test1)
end

function M:update()
    self.time = self.time + time.deltaTime
    if self.time > 1 then
        self.counter = self.counter + 1
        doc.text = collectgarbage("count")
        doc.toggle = not doc.toggle
        self.time = 0

        if doc.image == self.red then
            doc.image = self.green
        else
            doc.image = self.red
        end
    end
end

return M