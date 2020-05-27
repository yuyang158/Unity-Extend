local M = {}
local tinsert = table.insert
local setmetatable = setmetatable

function M.new(key)
	local meta = {
		__index = function(_, k)
			return M[k]
		end
	}
	return setmetatable({
		deps = {},
		collect = {},
		key = key
	}, meta)
end

function M:record(path)
	if not self.deps[path] then
		self.deps[path] = true
		tinsert(self.collect, path)
	end
end

function M:fetch(binding)
	if #self.collect == 0 then
		return
	end

	for _, key in ipairs(self.collect) do
		binding:watch(key, function()
			binding:computed_trigger(self.key)
		end)
	end

	self.collect = {}
end

return M