local assert, table, ipairs, next = assert, table, ipairs, next
local util = require "util"
local M = {}

---@param tasks function[]
---@param callback function
function M.waterfall(tasks, callback)
	assert(#tasks > 0)
	local taskIndex = 1
	local advance; do
		function advance(err, ...)
			if err then
				callback(err)
				return
			end
			taskIndex = taskIndex + 1
			local func = tasks[taskIndex]
			if not func then
				util.xpcall_catch(callback, nil, ...)
			end
			local pack = table.pack(..., advance)
			local ok, error = util.xpcall_catch(func, table.unpack(pack))
			if not ok then
				callback(error)
			end
		end
	end
	
	local ok, err = util.xpcall_catch(tasks[taskIndex], advance)
	if not ok then
		callback(err)
	end
end

---@param tasks function[]
---@param callback function
function M.parallel(tasks, callback)
	assert(#tasks > 0)
	local results = {}
	local callbackCount = 0
	for i, func in ipairs(tasks) do
		local ok, err = util.xpcall_catch(func, function(err, result)
			if err then
				callback(err)
				return
			end
			results[i] = result
			callbackCount = callbackCount + 1
			if #tasks == callbackCount then
				util.xpcall_catch(callback, nil, results)
			end
		end)
		if not ok then
			callback(err)
		end
	end
end

---@param coll any
---@param iteratee function
---@param callback function
function M.eachSeries(coll, iteratee, callback)
	local index, element = next(coll)
	if not index then
		return
	end
	local advance; do
		function advance(err, ...)
			if err then
				callback(err)
				return
			end
			index, element = next(coll, index)
			if not index then
				util.xpcall_catch(callback)
				return
			end
			
			local ok, error = util.xpcall_catch(iteratee, element, index, advance)
			if not ok then
				util.xpcall_catch(callback, error)
			end
		end
	end
	
	local ok, err = util.xpcall_catch(iteratee, element, index, advance)
	if not ok then
		util.xpcall_catch(callback, err)
	end
end

---@param coll any
---@param iteratee function
---@param callback function
function M.each(coll, iteratee, callback)
	local index = 1
	local results = {}
	local total = table.count(coll)
	local advance; do
		function advance(err, result)
			if err then
				callback(err)
				return
			end
			index = index + 1
			results[index] = result
			if index > total then
				util.xpcall_catch(callback, results)
			end
		end
	end
	for key, value in pairs(coll) do
		local ok, error = util.xpcall_catch(iteratee, value, key, advance)
		if not ok then
			util.xpcall_catch(callback, error)
			return
		end
	end
end

---@param n integer
function M.times(n, iteratee, callback)
	local index = 1
	local results = {}
	local advance; do
		function advance(err, result)
			if err then
				callback(err)
				return
			end
			results[index] = result
			index = index + 1
			if index > n then
				util.xpcall_catch(callback, results)
				return
			end

			local ok, error = util.xpcall_catch(iteratee, index, advance)
			if not ok then
				util.xpcall_catch(callback, error)
			end
		end
	end
	local ok, error = util.xpcall_catch(iteratee, index, advance)
	if not ok then
		util.xpcall_catch(callback, error)
	end
end

return M