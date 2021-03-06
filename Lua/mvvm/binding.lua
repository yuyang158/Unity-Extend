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
	return (type(key) == 'number') and (path .. '[' .. key .. ']') or (path .. '.' .. key)
end
local parent
local parent_key
local current_dep
local function build_computed(computed, source)
	local _computed = {
		__getters = {},
		__setters = {},
		__deps = {}
	}

	if computed then
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
	else
		computed = {}
	end

	return setmetatable(computed, {
		__getters = _computed.__getters,
		__setters = _computed.__setters,
		__deps = _computed.__deps,
		__index = function(_, k)
			local getter = _computed.__getters[k]
			if not getter then return end
			current_dep = _computed.__deps[k]
			local ret = getter(source)
			local tmp = current_dep
			current_dep = nil
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
				return data.__watch(p, cb)
			end
			local last = data
			for index, key in ipairs(keys) do
				if index == #keys then
					return last.__watch(key, cb)
				end
				last = last[key]
				if not last then return false end
			end
		end,
		__watch = function(k, cb)
			if not _data[k] then return false end
			local cbs = callbacks[k]
			if not cbs then
				cbs = {}
				callbacks[k] = cbs
			end
			tinsert(cbs, cb)
			return true
		end,
		detach = function(_, k, cb)
			local cbs = callbacks[k]
			if not cbs then
				return false
			end
			for i, v in ipairs(cbs) do
				if v == cb then
					tremove(cbs, i)
					return true
				end
			end
		end,
		__get = function(_, k)
			parent_key = k
			local val = _data[k]
			if current_dep and val then
				current_dep:record(append_key(path, k))
			end
			return val
		end,
		__set = function(_, k, v)
			assert(current_dep == nil, append_key(path, k))
			local oldVal = _data[k]
			if oldVal == v then
				return
			end
			
			_data[k] = type(v) == "table" and build_data(v, append_key(path, k)) or v
			local cbs = callbacks[k]
			if not cbs then return oldVal == nil or v == nil end
			for _, cb in ipairs(cbs) do
				cb(data, v)
			end
			return oldVal == nil or v == nil
		end,
		__notify = function(k, v)
			local cbs = callbacks[k]
			if not cbs then return end
			for _, cb in ipairs(cbs) do
				cb(data, v)
			end
		end
	}
	return setmetatable(data, {
		__index = function(_, k)
			parent = data
			return methods[k] or methods.__get(data, k)
		end,
		__newindex = function(_, k, v)
			if methods.__set(data, k, v) then
				local val = parent:__get(parent_key)
				parent.__notify(parent_key, val)
			end
		end,
		__pairs = function()
			return next, _data, nil
		end,
		__len = function()
			return #_data
		end
	})
end

function M.build(source)
	if type(source) ~= "table" then
		error("type need to be table")
		return
	end

	local meta = getmetatable(source)
	if meta and meta.__mark == mark then return end

	local data = build_data(source.data, "")
	local computed = build_computed(source.computed, source)
	local mixins = source.mixins

	source.data = nil
	source.computed = nil
	source.mixins = nil

	local compute_meta = getmetatable(computed)
	local computed_cbs = {}
	local index = {
		watch = function(_, path, cb)
			if compute_meta.__getters[path] then
				local cbs = computed_cbs[path] or {}
				tinsert(cbs, cb)
				computed_cbs[path] = cbs
			else
				if not data:watch(path, cb) and mixins then
					for _, v in ipairs(mixins) do
						if v:watch(path, cb) then break end
					end
				end
			end
		end,
		detach = function(_, path, cb)
			if compute_meta.__getters[path] then
				local cbs = computed_cbs[path]
				if not cbs then return end
				for i, v in ipairs(cbs) do
					if v == cb then
						tremove(cbs, i)
						break
					end
				end
			else
				if not data:detach(path, cb) and mixins then
					for _, v in ipairs(mixins) do
						v:detach(path, cb)
					end
				end
			end
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
			local val = data:__get(k)
			if not val and mixins then
				for _, mixin in ipairs(mixins) do
					val = mixin[k]
					if val then return val end
				end
			end
			return val
		end,
		set = function(_, k, v)
			if compute_meta.__setters[k] then
				computed[k] = v
				return
			end
			data:__set(k, v)
		end,
		setup_temp_getter = function(key, func)
			assert(type(func) == "function")
			local getter = compute_meta.__getters[key]
			if getter then return end
			compute_meta.__getters[key] = func
			compute_meta.__deps[key] = dep.new(key)
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