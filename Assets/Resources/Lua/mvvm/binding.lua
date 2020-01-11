local getmetatable, setmetatable, type, pairs, assert, ipairs, next = getmetatable, setmetatable, type, pairs, assert, ipairs, next
local M = {}
local mark = {}
local dep = require('mvvm.dep')
local util = require('util')
local tinsert, tremove = table.insert, table.remove

local function empty_function()
end

local function append_key(path, key)
    if not path or path == '' then return key end
    if type(key) ~= 'number' and type(key) ~= 'string' then error('not support key type' .. type(key)) end
    return (type(key) == 'number') and (path .. '[' .. key .. ']') or (path .. '.'..key)
end

local currentdep

local function build_computed(computed, source)
    if not computed then
        return
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
        _computed.__deps[k] = dep.new(k)
        computed[k] = nil
    end

    return setmetatable(computed, {
        __getters = _computed.__getters,
        __setters = _computed.__setters,
        __index = function(_, k)
            local getter = _computed.__getters[k]
            if not getter then return end
            currentdep = _computed.__deps[k]
            local ret = getter(source)
            local tmp = currentdep
            currentdep = nil
            tmp:fetch(source)
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
    local methods = {
        watch = function(_, p, cb)
            local keys = util.parse_path(p)
            if #keys == 1 then
                data.__watch(p, cb)
                return
            end
            local last = data
            for index, key in ipairs(keys) do
                if index == #keys then
                    last.__watch(key, cb)
                    return
                end
                last = last[key]
            end
        end,
        __watch = function(k, cb)
            local cbs = callbacks[k] or {}
            tinsert(cbs, cb)
            callbacks[k] = cbs
        end,
        detach = function(_, k, cb)
            local cbs = callbacks[k]
            if not cbs then 
                return
            end
            for i, v in ipairs(cbs) do
                if v == cb then
                    tremove(cbs, i)
                    break
                end
            end
        end,
        __get = function(_, k)
            if currentdep then
                currentdep:record(append_key(path, k))
            end
            return _data[k]
        end,
        __set = function(_, k, v)
            assert(currentdep == nil, append_key(path, k))
            if _data[k] == v then
                return
            end
            _data[k] = type(v) == "table" and build_data(v, append_key(path, k)) or v
            local cbs = callbacks[k]
            if not cbs then return end
            for _, cb in ipairs(cbs) do
                cb(data, v)
            end
        end
    }
    return setmetatable(data, {
        __index = function(_, k)
            return methods[k] or methods.__get(data, k)
        end,
        __newindex = function(_, k, v)
            return methods.__set(data, k, v)
        end,
        __pairs = function()
            return next, _data, nil
        end
    })
end

function M.bind(source)
    if type(source) ~= "table" then error("type need to be table") return end
    
    local meta = getmetatable(source)
    if meta and meta.__mark == mark then return end

    local data = build_data(source.data, '')
    local computed = build_computed(source.computed, source)
    
    source.data = nil
    source.computed = nil

    local computedmeta = getmetatable(computed)
    local computed_cbs = {}
    local index = {
        watch = function(_, path, cb)
            if computed and computedmeta.__getters[path] then
                local cbs = computed_cbs[path] or {}
                tinsert(cbs, cb)
                computed_cbs[path] = cbs
                return
            end
            data:watch(path, cb)
        end,
        computed_trigger = function(_, key)
            local val = computed[key]
            local cbs = computed_cbs[key]
            if not cbs then return end
            for _, callback in ipairs(cbs) do
                callback(source, val)
            end
        end,
        get = function(_, k)
            if computed then
                local val = computed[k]
                if val then return val end
            end
            return data:__get(k)
        end,
        set = function(_, k, v)
            if computedmeta and computedmeta.__setters[k] then
                computed[k] = v
                return
            end
            data:__set(k, v)
        end
    }
    return setmetatable(source, {
        __index = function(_, k)
            return index[k] or index.get(source, k) 
        end,
        __newindex = function(_, k, v)
            index.set(source, k, v)
        end
    })
end



return M