local M = class()
local mvvm = require("mvvm")
local SM = require("ServiceManager")

function M:ctor(go)
    local binding = go:GetComponent("LuaBinding")
    binding:Bind(self)

    local doc1 = mvvm.new_doc("test1", {aa = "abc", bb = { {a = 1} }})
    local doc = mvvm.new_doc("test", {a = 1, b = "a", c = {1,2,3}})
    ---@type TickService
    local service = SM.GetService(SM.SERVICE_TYPE.TICK)
    local timeLast = 0
    service.Register(function(tick)
        timeLast = timeLast + tick
        if timeLast > 1 then
            timeLast = 0
            doc.a = doc.a + 1
            doc.c[2] = math.random( 1, 100 )
        end
    end)
end

return M