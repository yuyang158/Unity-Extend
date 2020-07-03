---@class MockService
local M = {}

local responseMocks = {}
local requestMocksAfterResponse = {}
local requestMocksAfterRequest = {}

function M.Init()
end

function M.MockClientRequestResponse(requestName, data)
	assert(not responseMocks[requestName])
	responseMocks[requestName] = data
end

function M.MockServerRequestAfterResponse(requestName, data)
	requestMocksAfterResponse[requestName] = data
end

function M.MockServerRequestAfterRequest(requestName, data)
	requestMocksAfterRequest[requestName] = data
end

---@param client SprotoClient
function M.OnClientRequest(requestName, client, session)
	local data = responseMocks[requestName]
	if data then
		client:_OnResponse(session, data)
		return true
	end
end

---@param client SprotoClient
function M.OnServerResponse(requestName, client)
	local data = requestMocksAfterResponse[requestName]
	if data then
		assert(data.name)
		client:_OnServerRequest(data.name, data.args)
	end
end

---@param client SprotoClient
function M.OnServerRequest(requestName, client)
	local data = requestMocksAfterRequest[requestName]
	if data then
		assert(data.name)
		client:_OnServerRequest(data.name, data.args)
	end
end

return M