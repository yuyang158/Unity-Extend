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
			local ret = getter(source, source.compute_roots[k])
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

local function build_data(data, path, self_callback, root)
	-- ignore table with metatable
	if getmetatable(data) then
		return data
	end
	
	local _data = {}
	local callbacks = {}
	for k, v in pairs(data) do
		local cb = {}
		callbacks[k] = cb
		assert(type(k) == 'string' or type(k) == 'number', k)
		if type(v) == 'table' then
			_data[k] = build_data(v, append_key(path, k), cb, root)
		else
			_data[k] = v
		end
		data[k] = nil
	end

	local methods = {
		watch = function(_, p, cb, expression)
			if expression then
				root.watch(root, p, cb)
				return
			end
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
			if _data[k] == nil then return false end
			local cbs = callbacks[k]
			tinsert(cbs, cb)
			return true
		end,
		detach = function(_, k, cb)
			local cbs = callbacks[k]
			if not cbs then
				return root:detach(k, cb, true)
			end
			for i, v in ipairs(cbs) do
				if v == cb then
					tremove(cbs, i)
					return true
				end
			end
		end,
		__get = function(_, k)
			local val = _data[k]
			if current_dep and val ~= nil then
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
			local cbs
			if not callbacks[k] then
				cbs = {}
				callbacks[k] = cbs
			else
				cbs = callbacks[k]
			end
			_data[k] = type(v) == "table" and build_data(v, append_key(path, k), cbs, root) or v
			for _, cb in ipairs(cbs) do
				cb(v)
			end
			return oldVal == nil or v == nil
		end,
		__notify = function(k, v)
			local cbs = callbacks[k]
			for _, cb in ipairs(cbs) do
				cb(v)
			end
		end,
		__notify_self = function()
			for _, cb in ipairs(self_callback) do
				cb(data)
			end
		end,
		setup_temp_getter = function(key, func)
			root.compute_roots[key] = data
			return root.setup_temp_getter(key, func)
		end
	}
	return setmetatable(data, {
		__index = function(_, k)
			return methods[k] or methods.__get(data, k)
		end,
		__newindex = function(_, k, v)
			if methods.__set(data, k, v) then
				data.__notify_self()
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

	source.compute_roots = {}
	local data = build_data(source.data, "", nil, source)
	local computed = build_computed(source.computed, source)
	local mixins  = source.mixins

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
		detach = function(_, path, cb, compute)
			if compute_meta.__getters[path] or compute then
				compute_meta.__getters[path] = nil
				compute_meta.__deps[path] = nil
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
				callback(val)
			end
		end,
		get = function(_, k)
			local val = computed[k]
			if val ~= nil then return val end
			val = data:__get(k)
			if val == nil and mixins then
				for _, mixin in ipairs(mixins) do
					val = mixin[k]
					if val ~= nil then return val end
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
			assert(type(func) == "function", type(func))
			local getter = compute_meta.__getters[key]
			if not getter then
				compute_meta.__getters[key] = func
				compute_meta.__deps[key] = dep.new(key)
			end
			assert(source[key] ~= nil, key)
			return source[key]
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

_BindingEnv = {}
function M.SetEnvVariable(param)
	table.assign(_BindingEnv, param)
end

return M