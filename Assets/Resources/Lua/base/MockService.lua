---@class MockService
local M = {}

local responseMocks = {}
local requestMocksAfterResponse = {}
local requestMocksAfterRequest = {}

function M.Init()
end

function M.MockClientRequestResponse(requestName, data, filter)
	assert(not responseMocks[requestName])
	responseMocks[requestName] = {data = data, filter = filter}
end

function M.MockServerRequestAfterResponse(requestName, data, filter)
	requestMocksAfterResponse[requestName] = {data = data, filter = filter}
end

function M.MockServerRequestAfterRequest(requestName, data, filter)
	requestMocksAfterRequest[requestName] = {data = data, filter = filter}
end

---@param client SprotoClient
function M.OnClientRequest(requestName, client, session, args)
	local context = responseMocks[requestName]
	if context then
		if context.filter and not context.filter(args) then
			return
		end
		client:_OnResponse(session, context.data)
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
		assert(context.data.name)
		client:_OnServerRequest(context.data.name, context.data.args)
	end
end

---@param client SprotoClient
function M.OnServerRequest(requestName, client, args)
	local context = requestMocksAfterRequest[requestName]
	if context then
		if context.filter and not context.filter(args) then
			return
		end
		assert(context.data.name)
		client:_OnServerRequest(context.data.name, context.data.args)
	end
end

return M