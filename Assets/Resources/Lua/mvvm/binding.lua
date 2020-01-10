local getmetatable, setmetatable, type, pairs, assert, rawget = getmetatable, setmetatable, type, pairs, assert, rawget
local M = {}
local mark = {}
local dep = require('mvvm.dep')
local util = require('util')

local function empty_function()
end

local function append_key(path, key)
    if not path or path == '' then return key end
    if type(key) ~= 'number' and type(key) ~= 'string' then error('not support key type' .. type(key)) end
    return (type(key) == 'number') and (path .. '[' .. key .. ']') or (path .. '.'..key)
end

local currentdep

local function build_computed(computed, raw, source)
    if not computed then
        return raw
    end

    local _computed = {
        __getters = {},
        __setters = {},
        __deps = {}
    }

    for k, v in pairs(computed) do
        assert(type(k) == "string" or type(k) == "number", k)
        if type(v) == "function" then
            _computed.__getters[k] = v
        elseif type(v) == "table" then
            _computed.__getters[k] = v.getter
            _computed.__setters[k] = v.setter
        else
            error("only support value type : function, table")
        end
        _computed.__deps[k] = dep.new()
    end
    
    return setmetatable(raw, {
        __index = function(_, k)
            local val = rawget(raw. k)
            if val then
                return val 
            end
            local getter = _computed.__getters[k]
            if not getter then return end
            currentdep = _computed.__deps[k]
            local ret = getter(source)
            currentdep:fetch(source)
            return ret
        end,
        __newindex = function(_, k, value)
            local setter = _computed.__setters[k] or empty_function
            setter(source, value)
        end
    })
end

local function build_data(data, path)
    local _data = {}
    for k, v in pairs(data) do
        assert(type(k) == 'string' or type(k) == 'number', k)
        if type(v) == 'table' then
            _data[k] = build_data(v, append_key('', k))
        else
            _data[k] = v
        end
        data[k] = nil
    end
    
    local callbacks = {}
    return setmetatable(data, {
        __index = function(_, k)
            if currentdep then currentdep:record(append_key(path, k)) end
            return _data[k]
        end,
        __newindex = function(_, k, v)
            local callback = callbacks[k]
            callback(data, v)
        end
    })
end

function M.bind(source)
    if type(source) ~= "table" then error("type need to be table") return end
    
    local meta = getmetatable(source)
    if meta and meta.__mark == mark then return end
    
    local _data = build_data(source.data, '')
    source.data = nil

    build_computed(source.computed, _data)
    source.computed = nil

    meta = {
        __deps = {},
        __raw = _data,
        __index = function(_, k)
            local ret = rawget(_data, k)
            if not ret then
                return _data[k]
            end
            if currentdep then currentdep:record(append_key(path, k)) end
            return ret
        end,
        __newindex = function(_, k, v)
            if type(v) == "table" then
                v = M.bind(v, append_key(path, k))
            end
        end
    }
    return setmetatable(source, meta)
end



return M