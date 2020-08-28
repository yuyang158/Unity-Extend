---@class MockService
local M = {}

---@class MockContext
---@field data table @fake data
---@field filter function @whether trigger fake data by received package
---@field dataGen function @fake data provide function
---@field name string @server request name

---@type table<string, MockContext>
local responseMocks = {}
---@type table<string, MockContext>
local requestMocksAfterResponse = {}
---@type table<string, MockContext>
local requestMocksAfterRequest = {}

function M.Init()
end

---@param context MockContext
function M.MockClientRequestResponse(requestName, context)
	assert(not responseMocks[requestName])
	responseMocks[requestName] = context
end

---@param context MockContext
function M.MockServerRequestAfterResponse(requestName, context)
	requestMocksAfterResponse[requestName] = context
end

---@param context MockContext
function M.MockServerRequestAfterRequest(requestName, context)
	requestMocksAfterRequest[requestName] = context
end

---@param client SprotoClient
function M.OnClientRequest(requestName, client, session, args)
	local context = responseMocks[requestName]
	if context then
		if context.filter and not context.filter(args) then
			return
		end
		assert(context.data ~= nil or context.dataGen ~= nil)
		if context.data then
			client:_OnResponse(session, context.data)
		else
			client:_OnResponse(session, context.dataGen(args))
		end
		return true
	end
end

---@param client SprotoClient
function M.OnServerResponse(requestName, client, args)
	local context = requestMocksAfterResponse[requestName]
	if context then
		if context.filter and not context.filter(args) then
			return
		end

		assert(context.data ~= nil or context.dataGen ~= nil)
		if context.data then
			client:_OnServerRequest(context.name, context.data)
		else
			client:_OnServerRequest(context.name, context.dataGen(args))
		end
	end
end

---@param client SprotoClient
function M.OnServerRequest(requestName, client, args)
	local context = requestMocksAfterRequest[requestName]
	if context then
		if context.filter and not context.filter(args) then
			return
		end
		assert(context.data ~= nil or context.dataGen ~= nil)
		if context.data then
			client:_OnServerRequest(context.name, context.data)
		else
			client:_OnServerRequest(context.name, context.dataGen(args))
		end
	end
end

return M