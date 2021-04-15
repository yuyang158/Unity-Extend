---@class ConfigService
local M = {}
local ConfigUtil = CS.Extend.LuaUtil.ConfigUtil
local AssetReference = CS.Extend.Asset.AssetReference
local configs = {}
local math, tonumber, table, ipairs, setmetatable, assert, string = math, tonumber, table, ipairs, setmetatable, assert, string
local rapidjson = require "json"

local linkTypeMetaTable = {
	__index = function(t, k)
		local record = M.GetConfigRow(t.configName, t.id)
		return record[k]
	end
}

local relations = {
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
		return assert(rapidjson.decode(data))
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

	end,
	["asset"] = function(data)
		return AssetReference(data)
	end
}

---@param columnType string
---@param data table
local function convert_column_data(data, columnType, colName)
	-- if not data or #data == 0 then
	-- 	assert(columnType == "translate")
	-- end
	return assert(columnDataConverter[columnType], columnType)(data, colName)
end

local function load_config_data(filename, base, i18n)
	local baseConf
	if base then
		baseConf = assert(configs[base], base)
	end

	local textData = ConfigUtil.LoadConfigFile(filename)
	if not textData then
		return
	end
	local config = configs[filename] or {}
	local keymap = {}
	for i, key in ipairs(textData.keys) do
		keymap[key] = i
	end
	local i18nFile = string.find(filename, "_i18n")
	for _, row in ipairs(textData.rows) do
		local id = i18nFile and row[1] or tonumber(row[1])
		local convertedRow = { id }
		for i = 2, #row do
			local typ = textData.types[i]
			local key = textData.keys[i]

			if typ == "translate" then
				local i18nConf = i18n[string.format("%s:%d", key, id)]
				local text = i18nConf and assert(i18nConf[M.currentLanguage]) or ""
				text = text:replace("\\n", "\n")
				table.insert(convertedRow, text)
			else
				table.insert(convertedRow, convert_column_data(row[i], typ, key))
			end
		end

		config[id] = setmetatable(convertedRow, {
			__keymap = keymap,
			__index = function(t, k)
				local index = keymap[k]
				if not index then
					if baseConf then
						return baseConf[k]
					else
						warn("Not found key : ", k, "in table", filename)
					end
				else
					return t[index]
				end
			end
		})
	end

	configs[filename] = config
	return config
end

function M.Init()
end

function M.Reload(name)
	load_config_data(name)
end

---@param name string
function M.GetConfig(name)
	if not configs[name] then
		local i18n = load_config_data(name .. "_i18n")
		if relations[name] then
			load_config_data(name, M.GetConfig(relations[name]), i18n)
		else
			load_config_data(name, nil, i18n)
		end
	end
	return assert(configs[name], name)
end

---@param name string
---@param id number
function M.GetConfigRow(name, id)
	local config = assert(M.GetConfig(name))
	return config[id]
end

M.currentLanguage = "cn"
function M.ChangeLanguage(lang)
	M.currentLanguage = lang
end

function M.clear()
	configs = nil
end

return M