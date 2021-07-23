---@class MockService
local M = {}

---@class MockContext table key protocName value args

---@type table<string, MockContext>
local responseMocks = {}
---@type table<string, MockContext>
local requestMocksAfterRequest = {}

function M.Init()
end

---@param context MockContext
function M.MockClientRequestResponse(protocName, context)
	assert(not responseMocks[protocName])
	responseMocks[protocName] = context
end

---@param context MockContext
function M.MockServerRequestAfterRequest(protocName, context)
	requestMocksAfterRequest[protocName] = context
end

---@param client ProtocClient
function M.OnClientRequest(protocName, client)
	local context = responseMocks[protocName]
	if context then
		for key, value in pairs(context) do
			client:_OnResponse(key, value)
		end
		return true
	end

	return false
end

---@param client ProtocClient
function M.OnServerResponse(protocName, client)
	M.OnServerRequest(protocName, client)
end

---@param client ProtocClient
function M.OnServerRequest(protocName, client)
	local context = requestMocksAfterRequest[protocName]
	if context then
		for key, value in pairs(context) do
			client:_OnServerRequest(key, value)
		end
	end
end

return M