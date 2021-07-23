---@class base.unique_table
local M = class()

function M:ctor()
	self.kToV = {}
	self.vToK = {}
end

function M:add(k, v)
	self.kToV[k] = v
	self.vToK[v] = k
end

function M:find_value(k)
	return self.kToV[k]
end

function M:find_key(v)
	return self.vToK[v]
end

function M:each(func)
	for k, v in pairs(self.kToV) do
		if func(k, v) then
			break
		end
	end
end

function M:remove_by_key(k)
	local v = self.kToV[k]
	if not v then
		return
	end
	self.kToV[k] = nil
	self.vToK[v] = nil
end

function M:remove_by_value(v)
	local k = self.vToK[v]
	if not k then
		return
	end
	self.kToV[k] = nil
	self.vToK[v] = nil
end

return M