local binding = require('mvvm.binding')
local getmetatable, setmetatable = getmetatable, setmetatable

local M = {}

function M.BuildDataSource(source)
    local bind = binding.bind(source)
    return setmetatable({}, {
        __index = bind.get,
        __newindex = bind.set
    }), bind
end

function M.SetupLuaNotification(t, path, callback)
    t:watch(path, function(_, v)
        callback(v)
    end)
end

function M.RawSetLuaDataSource(t, k, v)
    t:set(k, v)
end

return M