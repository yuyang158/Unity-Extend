local rawset = rawset
local rawget = rawget
local ipairs = ipairs
local getmetatable = getmetatable
local setmetatable = setmetatable
local assert = assert
local tinsert = table.insert

local meta = {
    __newindex = function(t, k, v)
        if t[k] == v then
            return
        end
        rawset(t.__values, k, v)
        if t.__notifications then
            local notifications = t.__notifications[k]
            if not notifications then
                rawset(t, k, v)
                return
            end
            rawset(t.__values, k, v)
            for _, callback in ipairs(notifications) do
                callback(v)
            end
        end
    end,
    __index = function(t, k)
        return rawget(t, k) or t.__values[k]
    end
}

local M = {
    placeholder = {}
}
function M.SetupLuaNotification(t, k, callback)
    local oldMeta = getmetatable(t)
    if oldMeta ~= meta then
        assert(oldMeta == nil)
        t = setmetatable(t, meta)
        rawset(t, "__notifications", {})
        rawset(t, "__values", {})
    end
    
    local val = t[k]
    t[k] = nil
    t.__values[k] = val
    local notifications = t.__notifications[k]
    if not notifications then
        notifications = {}
        t.__notifications[k] = notifications
    end
    tinsert(notifications, callback)
end

function M.RawSetLuaDataSource(t, k, v)
    assert(t.__values)
    assert(t.__values[k] ~= nil)
    rawset(t.__values, k, v)
end

return M