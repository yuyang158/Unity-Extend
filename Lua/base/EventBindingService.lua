local M = {}
local tpack, tunpack, tinsert, tremove, tostring, assert = table.pack, table.unpack, table.insert, table.remove, tostring, assert
local LuaBindingEventBaseType = typeof(CS.Extend.LuaBindingEvent.LuaBindingEventBase)
local callbacks = {}
local csBridgeCallbacks = {}
local id = 1

function M.Init()
	
end

function M.clear()
	callbacks = {}
	csBridgeCallbacks = {}
end

function M.Dispatch(_id, pointData)
	local cb = csBridgeCallbacks[_id]
	if not cb then
		warn("not found event for id:", _id)
		return
	end
	tinsert(cb.pack, pointData)
	cb.pack.n = cb.pack.n + 1
	cb.func(tunpack(cb.pack))
end

function M.AddEventListener(eventName, go, func, ...)
	assert(callbacks[func]==nil)
	callbacks[tostring(func) .. go:GetInstanceID()] = id
	csBridgeCallbacks[id] = {pack = tpack(...), func = func}
	local bindingEvent = go:GetComponent(LuaBindingEventBaseType)
	if not bindingEvent then
		error("Event binding error : ", go.name, eventName)
	end
	bindingEvent:AddEventListener(eventName, id)
	id = id + 1
end

function M.RemoveEventListener(eventName, go, func)
	local key = tostring(func) .. go:GetInstanceID()
	if not callbacks[key] then
		return
	end
	local _id = callbacks[key]
	callbacks[key] = nil
	csBridgeCallbacks[_id] = nil
	local bindingEvent = go:GetComponent(LuaBindingEventBaseType)
	bindingEvent:RemoveEventListener(eventName, _id)
end

return M