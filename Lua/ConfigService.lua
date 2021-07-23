---@class ConfigService
local M = {}
local ConfigUtil = CS.Extend.LuaUtil.ConfigUtil
local AssetReference = CS.Extend.Asset.AssetReference
local ColorUtility = CS.UnityEngine.ColorUtility
local configs = {}
local math, tonumber, table, ipairs, setmetatable, assert, string, load = math, tonumber, table, ipairs, setmetatable, assert, string, load
local insert, rawget = table.insert, rawget
local HexToColor = HexToColor
local rapidjson = require "rapidjson"
local formulaEnv = {}

local linkTypeMetaTable = {
	__index = function(t, k)
		local row = rawget(t, "row")
		if row then
			return row[k]
		end
		local record = M.GetConfigRow(t.configName, t.id)
		t.row = record
		return record[k]
	end
}

local relations = {}

local postprocessor = {}

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
	["color"] = function(data)
		local success, color = ColorUtility.TryParseHtmlString("#" .. data)
		assert(success, data)
		return color
	end,
	["link"] = function(data, configName)
		local id = tonumber(data)
		if id <= 0 then
			return false
		end
		return setmetatable({id = id, configName = configName}, linkTypeMetaTable)
	end,
	["linkjson"] = function(data, configName)
		local ids = assert(rapidjson.decode(data))
		local array = {}
		for _, id in ipairs(ids) do
			insert(array, id > 0 and setmetatable({id = id, configName = configName}, linkTypeMetaTable) or false)
		end
		return array
	end,
	["boolean"] = function(data)
		return data == "1"
	end,
	["translate"] = function()

	end,
	["asset"] = function(data)
		return AssetReference(data)
	end,
	["formula"] = function(data, configName)
		return load("return " .. data, configName, "t", formulaEnv)
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

local function load_config_data(filename, extends, i18n)
	local keyToExtend
	if extends then
		keyToExtend = {}
		for index, v in ipairs(extends) do
			local meta = getmetatable(v)
			local extendKeymap = meta.__keymap
			for k, i in pairs(extendKeymap) do
				if i ~= 1 then
					assert(not keyToExtend[k], k)
					keyToExtend[k] = index
				end
			end
		end
	end
	local processors = postprocessor[filename]
	local textData = ConfigUtil.LoadConfigFile(filename)
	if not textData then
		return
	end
	local keymap = {}
	local config = configs[filename] or setmetatable({}, {__keymap = keymap})
	local max
	for i, key in ipairs(textData.keys) do
		keymap[key] = i
		max = i
	end

	if processors then
		for i, v in ipairs(processors) do
			keymap[v.key] = max + i
		end
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

		local meta = {
			__index = function(t, k)
				if keyToExtend then
					local extendIndex = keyToExtend[k]
					if extendIndex then
						local extendRow = assert(extends[extendIndex][id], id)
						return extendRow[k]
					end
				end
				local index = keymap[k]
				if not index then
					warn("Not found key : ", k, "in table", filename)
				else
					return t[index]
				end
			end
		}
		convertedRow = setmetatable(convertedRow, meta)
		if processors then
			for _, v in ipairs(processors) do
				table.insert(convertedRow, v.processor(convertedRow))
			end
		end
		meta.__newindex = function()
			error("Config don`t have setter")
		end
		
		config[id] = convertedRow
	end

	configs[filename] = config
	return config, keymap
end

function M.Init()
	local extendsInfo = M.GetConfig("extendsInfo")
	for _, extends in pairs(extendsInfo) do
		relations[extends.parent] = string.split(extends.childs, ",")
	end
	M.GetConfig("Enumer")
	M.GetConfig("CStringConfig")
	M.GetConfig("QualityColorConfig")
end

function M.Reload(name)
	configs[name] = nil
	M.GetConfig(name)
end

---@param name string
function M.GetConfig(name)
	if not configs[name] then
		local extends
		local relateConfNames = relations[name]
		if relateConfNames then
			extends = {}
			for i, confName in ipairs(relateConfNames) do
				local i18n = load_config_data(confName .. "_i18n")
				local extend = load_config_data(confName, nil, i18n)
				extends[i] = extend
			end
		end
		
		local i18n = load_config_data(name .. "_i18n")
		load_config_data(name, extends, i18n)
	end
	return assert(configs[name], name)
end

---@param name string
---@param id number
function M.GetConfigRow(name, id)
	assert(id)
	local config = assert(M.GetConfig(name), name)
	return config[id]
end

M.currentLanguage = "cn"
function M.ChangeLanguage(lang)
	M.currentLanguage = lang
end

function M.GetEnumerValue(id)
	return assert(configs.Enumer[id], id).value
end

function M.GetEnumerNumberValue(id)
	return tonumber(assert(configs.Enumer[id], id).value)
end

---@return string
function M.GetStringValue(id)
	return assert(configs.CStringConfig[id], id).content
end

function M.GetColor(id, alpha)
	local hex = assert(configs.QualityColorConfig[id], id).rareColor
	return HexToColor(hex, alpha)
end

function M.UnLoad(name)
	configs[name] = nil
end

function M.RegisterPostProcess(tsvName, key, processor)
	local processors = postprocessor[tsvName]
	if not processors then
		processors = {}
		postprocessor[tsvName] = processors 
	end
	table.insert(processors, {key = key, processor = processor})
end

function M.SetFormulaVariables(variables)
	table.assign(formulaEnv, variables)
end

function M.clear()
	configs = nil
end

return M