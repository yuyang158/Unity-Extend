local breakSocketHandle,debugXpCall = require("LuaDebug")("localhost",7003)

local SM = require "ServiceManager"
local CS = require "ConfigService"
local next = next

function string.split(self, delimiter)
    local result = { }
    local from = 1
    local f, t = string.find(self, delimiter, from, true)
    while f do
        table.insert(result, string.sub(self, from, f - 1))
        from = t + 1
        f, t = string.find(self, delimiter, from, true)
    end
    table.insert(result, string.sub(self, from))
    return result
end

function table.empty(t)
    return next(t) == nil
end

SM.RegisterService(SM.SERVICE_TYPE.CONFIG, CS)