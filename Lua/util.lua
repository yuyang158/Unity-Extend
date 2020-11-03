local setmetatable, getmetatable, rawset, rawget, pairs, type, ipairs = setmetatable, getmetatable, rawset, rawget, pairs, type, ipairs
local insert, concat, remove = table.insert, table.concat, table.remove
local find, format, sub, gsub = string.find, string.format, string.sub, string.gsub
local tonumber, tostring = tonumber, tostring
local next = next

string.replace = function(s, pattern, repl)
	local i, j = find(s, pattern, 1, true)
	if i and j then
		local ret = {}
		local start = 1
		while i and j do
			insert(ret, sub(s, start, i - 1))
			insert(ret, repl)
			start = j + 1
			i, j = find(s, pattern, start, true)
		end
		insert(ret, sub(s, start))
		return concat(ret)
	end
	return s
end

string.replace_with_table = function(s, t)
	return gsub(s, "%$(%w+)", t)
end

string.split = function(s, delimiter)
	local result = {}
	local from = 1
	local f, t = find(s, delimiter, from, true)
	while f do
		insert(result, sub(s, from, f - 1))
		from = t + 1
		f, t = find(s, delimiter, from, true)
	end
	insert(result, sub(s, from))
	return result
end

function table.empty(t)
	return next(t) == nil
end

function table.array_each(t, callback)
	for i = 1, #t do
		callback(t[i], i)
	end
end

function table.table_each(t, callback)
	for k, v in pairs(t) do
		callback(v, k)
	end
end

function table.index_of(t, val)
	for i = 1, #t do
		if val == t[i] then
			return i
		end
	end
	return -1
end

function table.index_of_predict(t, func)
	for i = 1, #t do
		if func(t[i]) then
			return i
		end
	end
	return -1
end

function table.swap_remove(t, index)
	if not index then
		index = 1
	end
	t[index] = t[#t]
	return remove(t, #t)
end

local function dump(t)
	local log = ""
	local print_r_cache = {}
	local function sub_print_r(t, indent)
		if (print_r_cache[tostring(t)]) then
			log = log .. indent .. "*" .. tostring(t)
		else
			print_r_cache[tostring(t)] = true
			if type(t) == "table" then
				for pos, val in pairs(t) do
					if type(val) == "table" then
						log = log .. indent .. "[" .. pos .. "] => " .. tostring(t) .. " {\n"
						sub_print_r(val, indent .. string.rep(" ", string.len(pos) + 8))
						log = log .. indent .. string.rep(" ", string.len(pos) + 6) .. "}\n"
					elseif (type(val) == "string") then
						log = log .. indent .. "[" .. pos .. '] => "' .. val .. '"\n'
					else
						log = log .. indent .. "[" .. pos .. "] => " .. tostring(val) .. '\n'
					end
				end
			else
				log = log .. indent .. tostring(t) .. "\n"
			end
		end
	end
	if type(t) == "table" then
		log = log .. tostring(t) .. " {\n"
		sub_print_r(t, "  ")
		log = log .. "} \n"
	else
		sub_print_r(t, "  ")
	end
    return log
end

table.print_r = function(t)
    print(dump(t))
end

local traceback = debug.traceback
local xpcall_catch = function(f, ...)
	local ok, err = xpcall(f, traceback, ...)
	if not ok then
		error(err)
	end

	return ok, err
end

local function parse_path(path)
	if not path or path == '' then error('invalid path:' .. tostring(path)) end
	local result = {}
	local i, n = 1, #path
	while i <= n do
		local s, e, split1, key, split2 = find(path, "([%.%[])([^%.^%[^%]]+)(%]?)", i)
		if not s or s > i then
			insert(result, sub(path, i, s and s - 1))
		end
		if not s then break end
		if split1 == '[' then
			if split2 ~= ']' then error('invalid path:' .. path) end
			key = tonumber(key)
			if not key then error('invalid path:' .. path) end
			insert(result, key)
		else
			insert(result, key)
		end
		i = e + 1
	end
	return result
end

local function gen_callback_func(tbl, path, callback)
	local keys = parse_path(path)
	local final
	local finalKey
	for index, key in ipairs(keys) do
		if #keys == index then
			finalKey = key
			break
		end
		final = tbl[key]
	end
	final.__notifications = final.__notifications or {}
	local notifications = final.__notifications[finalKey] or {}
	insert(notifications, callback)
	final.__notifications[finalKey] = notifications
end

return {
	parse_path = parse_path,
	gen_callback_func = gen_callback_func,
	xpcall_catch = xpcall_catch
}