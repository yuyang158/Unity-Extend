local rawset, rawget, ipairs, getmetatable, setmetatable, assert, type = rawset, rawget, ipairs, getmetatable, setmetatable, assert, type
local tinsert = table.insert
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

function M.BuildDataSource(source)
    
end

function M.SetupLuaNotification(t, path, callback)
    
end

function M.RawSetLuaDataSource(t, k, v)
    
end

return M