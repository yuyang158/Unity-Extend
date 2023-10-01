local tpack, tunpack, tinsert, tremove, ipairs, assert = table.pack, table.unpack, table.insert, table.remove, ipairs, assert

---@class base.EventDispatcher
local M = class()

function M:ctor()
	self.wait4Remove = {}
	self.listeners = {}
end

---@param typ string
---@param func function
function M:AddEventListener(typ, func, ...)
	local packed = tpack(...)
	local typListeners = self.listeners[typ]
	if not typListeners then
		typListeners = {}
		self.listeners[typ] = typListeners
	end

	self._InsertNewListener(typListeners, func, packed, 0)
end

---@param typ string
---@param func function
function M:AddPriorityEventListener(typ, priority, func, ...)
	local packed = tpack(...)
	local typListeners = self.listeners[typ]
	if not typListeners then
		typListeners = {}
		self.listeners[typ] = typListeners
	end

	self._InsertNewListener(typListeners, func, packed, priority)
end

function M._InsertNewListener(listeners, func, packed, priority)
	local index = 1
	for i, v in ipairs(listeners) do
		if v.priority <= priority then
			index = i
			break
		end
	end
	assert(func and type(func) == "function")
	tinsert(listeners, index, {func = func, packed = packed, priority = priority})
end

---@param typ string
function M:DispatchEvent(typ, ...)
	local typListeners = self.listeners[typ]
	if not typListeners then
		return
	end

	self.dispatching = typ
	for _, listener in ipairs(typListeners) do
		if listener.packed.n > 0 then
			listener.func(tunpack(listener.packed), ...)
		else
			listener.func(...)
		end
	end
	self.dispatching = nil

	if #self.wait4Remove > 0 then
		for _, v in ipairs(self.wait4Remove) do
			self:RemoveEventListener(v[1], v[2])
		end
		self.wait4Remove = {}
	end
end

---@param typ string
---@param func function
function M:RemoveEventListener(typ, func)
	if self.dispatching == typ then
		tinsert(self.wait4Remove, {typ, func})
		return
	end
	local typListeners = self.listeners[typ]
	if not typListeners then
		warn("Unknown event type", typ)
		return
	end
	for i, v in ipairs(typListeners) do
		if v.func == func then
			tremove(typListeners, i)
			return
		end
	end
	warn("Remove function not found : ", typ)
end

return M