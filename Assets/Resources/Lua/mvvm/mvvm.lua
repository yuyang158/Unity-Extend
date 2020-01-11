local binding = require('mvvm.binding')

local M = {}

function M.BuildDataSource(source)
    binding.bind(source)
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