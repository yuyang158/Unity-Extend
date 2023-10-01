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
	cb.func(tunpack(cb.pack), pointData)
end

function M.AddEventListener(eventName, component, func, ...)
	assert(not callbacks[func])
	callbacks[tostring(func) .. component:GetInstanceID()] = id
	csBridgeCallbacks[id] = {pack = tpack(...), func = func}
	local bindingEvent = component
	if not bindingEvent then
		error("Event binding error : ", component.name, eventName)
		return
	end
	bindingEvent:AddEventListener(eventName, id)
	id = id + 1
end

function M.RemoveEventListener(eventName, component, func)
	local key = tostring(func) .. component:GetInstanceID()
	if not callbacks[key] then
		return
	end
	local _id = callbacks[key]
	callbacks[key] = nil
	csBridgeCallbacks[_id] = nil
	local bindingEvent = component
	bindingEvent:RemoveEventListener(eventName, _id)
end

return M