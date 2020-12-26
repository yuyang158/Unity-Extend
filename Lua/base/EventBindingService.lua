local M = {}
local tpack, tunpack, tinsert, tremove, tostring, assert = table.pack, table.unpack, table.insert, table.remove, tostring, assert
local LuaBindingEventBaseType = typeof(CS.Extend.LuaBindingEvent.LuaBindingEventBaseType)
local callbacks = {}
local csBridgeCallbacks = {}
local id = 1

function M.Init()
	
end

function M.Dispatch(_id)
	local cb = csBridgeCallbacks[_id]
	if not cb then
		warn("not found event for id:", _id)
		return
	end
	if cb.pack.n == 0 then
		cb.func()
	else
		cb.func(tunpack(cb.pack))
	end
end


function M.AddEventListener(eventName, go, func, ...)
	assert(callbacks[func])
	callbacks[tostring(func) .. go:GetInstanceID()] = id
	csBridgeCallbacks[id] = {pack = tpack(...), func = func}
	local bindingEvent = go:GetComponent(LuaBindingEventBaseType)
	bindingEvent:AddEventListener(eventName, id)
	id = id + 1
end

function M.RemoveEventListener(eventName, go, func)
	local key = tostring(func) .. go:GetInstanceID()
	local _id = assert(callbacks[key], key)
	callbacks[key] = nil
	csBridgeCallbacks[_id] = nil
	local bindingEvent = go:GetComponent(LuaBindingEventBaseType)
	bindingEvent:RemoveEventListener(eventName, id)
end

return M