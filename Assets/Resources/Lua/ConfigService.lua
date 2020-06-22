---@class ConfigService
local M = {}
local ConfigUtil = CS.Extend.LuaUtil.ConfigUtil
local configs = {}
local math, tonumber, table, ipairs, setmetatable, assert, string = math, tonumber, table, ipairs, setmetatable, assert, string
local json = require "json"
local i18n

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
	end,
	["translate"] = function()

	end
}

---@param columnType string
---@param data table
local function convert_column_data(data, columnType, colName)
	if not data or #data == 0 then
		assert(columnType == "translate")
	end
	return assert(columnDataConverter[columnType], columnType)(data, colName)
end

local function load_config_data(filename)
	local textData = ConfigUtil.LoadConfigFile(filename)
	local config = configs[filename] or {}

	local keymap = {}
	for i, v in ipairs(textData.keys) do
		keymap[v] = i
	end

	for _, row in ipairs(textData.rows) do
		local id = row[1]
		local convertedRow = { id }
		for i = 2, #row do
			local typ = textData.types[i]
			local key = textData.keys[i]

			if typ == "translate" then
				local i18nConf = i18n[string.format("%s:%s:%s", filename, id, key)]
				table.insert(convertedRow, assert(i18nConf[M.currentLanguage]))
			else
				table.insert(convertedRow, convert_column_data(row[i], typ, key))
			end
		end

		config[id] = setmetatable(convertedRow, {
			__index = function(t, k)
				local index = assert(keymap[k], k)
				return t[index]
			end
		})
	end

	configs[filename] = config
end

function M.Init()
	load_config_data("i18n")
	i18n = configs.i18n
	load_config_data("excel1")
	load_config_data("excel2")
end

function M.Reload(name)
	load_config_data(name)
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

M.currentLanguage = "zh-s"
function M.ChangeLanguage(lang)
	M.currentLanguage = lang
end

function M.clear()
	configs = nil
end

return M