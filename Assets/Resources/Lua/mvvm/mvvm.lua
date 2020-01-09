local rawset, rawget, ipairs, getmetatable, setmetatable, assert, type = rawset, rawget, ipairs, getmetatable, setmetatable, assert, type
local tinsert = table.insert
local dep = require('mvvm.dep')
local util = require('util')

local M = {
    placeholder = {}
}

local metaFlag = {}
local function append_key(path, key)
    if not path or path == '' then return key end
    if type(key) ~= 'number' and type(key) ~= 'string' then error('not support key type' .. type(key)) end
    return (type(key) == 'number') and (path .. '[' .. key .. ']') or (path .. '.'..key)
end

local depLog
function M.BuildDataSource(tbl, path)
    assert(type(tbl) == "table")
    local _tbl = {}
    for k, v in pairs(tbl.data) do
        assert(type(k) == "string" or type(k) == "number")
        _tbl.data[k] = type(v) == "table" and M.BuildDataSource(v, append_key(path, k)) or v
        tbl.data[k] = nil
    end
    _tbl.computed = tbl.computed
    tbl.computed = nil
    
    local depCollector
    if getmetatable(tbl) == metaFlag then return end
    return setmetatable(tbl, {
        __flag = metaFlag,
        __raw = _tbl,
        __index = function(self, k)
            if _tbl.computed and _tbl.computed[k] then
                local computed = _tbl.computed[k]
                _tbl.deps = _tbl.deps or {}
                depCollector = _tbl.deps[k]
                if not depCollector then
                    depCollector = dep.new()
                    _tbl.deps[k] = depCollector
                end
                local ret
                if type(computed) == "function" then
                    ret = computed(self)
                else
                    if not computed.getter then 
                        return
                    end
                    assert(type(computed.getter) == "function", k)
                    ret = computed.getter(self)
                    depCollector:Fetch(self)
                    
                    depCollector:Clear()
                    depCollector = nil
                end
                
                return ret
            end
            if depLog then
                depLog(append_key(path, k))
            end
            if not _tbl.data then 
                return
            end
            if depCollector then
                depCollector:Record(append_key(path, k))
            end
            return _tbl.data[k]
        end,
        __newindex = function(self, k, v)
            if _tbl.computed and _tbl.computed[k] then
                local compute = _tbl.computed[k]
                assert(type(compute) == "table", k)
                assert(compute.setter)
                compute.setter(self, v)
                return
            end

            _tbl.data = _tbl.data or {}
            if type(v) == "table" then
                _tbl.data[k] = M.BuildDataSource(v, append_key(path, k))
            else
                _tbl.data[k] = v
            end
             
            local notifications = rawget(_tbl, "__notifications")
            if notifications and #notifications > 0 then
                for _, notify in ipairs(notifications) do
                    notify(self, v)
                end
            end
        end
    })
end

function M.SetupLuaNotification(t, path, callback)
    local meta = getmetatable(t)
    local tbl = meta.__raw
    local func = function(_, val)
        callback(val)
    end
    util.gen_callback_func(tbl, path, func)
end

function M.RawSetLuaDataSource(t, k, v)
    local keys = util.parse_path(k)
    local final = t
    for index, key in ipairs(keys) do
        if index == #keys then
            local tbl = getmetatable(final)
            if tbl.computed and tbl.computed[key] then
                t[key] = v
            else
                rawset(tbl.data, key, v)
            end
            return
        end
        final = t[key]
    end
end

return M