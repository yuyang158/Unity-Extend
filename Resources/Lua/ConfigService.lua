---@class ConfigService
local M = {}
local ConfigUtil = CS.Extend.LuaUtil.ConfigUtil
local configs = {}
local math = math
local tonumber = tonumber
local json = require "json"

local configRowMetaTable = {
    __index = function(t, k)
        local index = t.source.keymap[k]
        return t.values[index]
    end
}

local linkTypeMetaTable = {
    __index = function(t, k)
        local record = M.GetConfigRow(t.configName, t.id)
        return record[k]
    end
}

local columnDataConverter = {
    ["int"] = function(data)
        return assert(math.tointeger(data))
    end,
    ["number"] = function(data)
        return assert(tonumber(data))
    end,
    ["string"] = function(data)
        return data
    end,
    ["json"] = function(data)
        return json.decode(data)
    end,
    ["link"] = function(data, configName)
        local v = {
            id = data,
            configName = configName
        }

        return setmetatable(v, linkTypeMetaTable)
    end,
    ["boolean"] = function(data)
        return data == "1"
    end
}

---@ param columnType string
---@ param columnData table
local function convert_column_data(columnData, columnType, key)
    if not columnData or #columnData == 0 then
        return
    end
    return columnDataConverter[columnType](columnData, key)
end

local function load_config_data(filename)
    local textData = ConfigUtil.LoadConfigFile(filename)
    local config = {
        keymap = {}
    }

    for i, v in ipairs(textData.keys) do
        config.keymap[v] = i
    end

    for _, row in ipairs(textData.rows) do
        local id = row[1]
        local convertedRow = { id }
        for i = 2, #row do
            local typ = textData.types[i]
            local key = textData.keys[i]
            table.insert(convertedRow, convert_column_data(row[i], typ, key))
        end

        local parsedData = { source = config, values = convertedRow }
        config[id] = setmetatable(parsedData, configRowMetaTable)
    end

    configs[filename] = config
end

function M.Init()
    load_config_data("excel1")
    load_config_data("excel2")
end

---@param name string
function M.GetConfig(name)
    return assert(configs[name], name)
end

---@param name string
---@param id string
function M.GetConfigRow(name, id)
    local config = assert(configs[name], name)
    return config[id]
end

return M