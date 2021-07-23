local insert, xpcall, traceback = table.insert, xpcall, debug.traceback
---@class sequence
local M = class()
local behaviour = require("base.action.behaviour")

function M:ctor(on_complete, on_error)
	self.actions = {}
	self.on_complete = on_complete
	self.on_error = on_error
	self.current = 0
end

---@return behaviour
function M:build()
	return behaviour.new(self)
end

function M:next(...)
	self.current = self.current + 1
	local action = self.actions[self.current]
	if not action then
		if self.on_complete then
			self.on_complete(...)
		end
		return
	end

	local ok, ret = xpcall(action.active, traceback, ...)
	if not ok then
		if self.on_error then
			self.on_error(ret)
		else
			error(ret)
		end
	end
end

function M:add(action)
	insert(self.actions, action)
end

function M:reset()
	self.current = 0
end

function M:interrupt()
	self.current = 9999
end

function M:finished()
	return self.actions[self.current] == nil
end

return M