---@class SprotoClient : EventDispatcher
local M = class(require("base.EventDispatcher"))
local AutoReconnectTcpClient = CS.Extend.Network.SocketClient.AutoReconnectTcpClient
local sproto = require 'sproto.sproto'
local tunpack, tpack, next, spack, assert = table.unpack, table.pack, next, string.pack, assert
local SM = require "ServiceManager"
---@type MockService
local MockService = SM.GetService(SM.SERVICE_TYPE.MOCK)

M.Event = {
	StatusChanged = "StatusChanged"
}

function M:ctor(c2sPath, s2cPath)
	self.client = AutoReconnectTcpClient(self)
	local assetService = CS.Extend.Asset.AssetService.Get()
	local textAssetType = typeof(CS.UnityEngine.TextAsset)
	local proto = {
		c2s = assetService:Load(c2sPath, textAssetType):GetTextAsset(),
		s2c = assetService:Load(s2cPath, textAssetType):GetTextAsset()
	}
	local sprotoparser = require "sproto.sprotoparser"
	proto.c2s = sprotoparser.parse(proto.c2s.text)
	proto.s2c = sprotoparser.parse(proto.s2c.text)

	self.host = sproto.new(proto.s2c):host('package')
	self.request = self.host:attach(sproto.new(proto.c2s))
	self.session = 1
	self.callbackTimeout = 2
	self.wait4Responses = {}
	self.responseCommonCallback = {}
	self.serverRequest = {}
end

function M:Connect(host, port)
	self.client:Connect(host, port)
end

function M:RegisterServerRequestCallback(name, callback, ...)
	local params = tpack(...)
	assert(not self.serverRequest[name], name)
	assert(callback)
	self.serverRequest[name] = {
		callback = callback,
		params = params
	}
end

function M:UnregisterServerRequestCallback(name)
	self.serverRequest[name] = nil
end

function M:RegisterResponseCommonCallback(name, callback, ...)
	local params = tpack(...)
	self.responseCommonCallback[name] = {
		callback = callback,
		params = params
	}
end

function M:UnregisterResponseCommonCallback(name)
	self.responseCommonCallback[name] = nil
end

function M:Destroy()
	self.client:Destroy()
end

function M:SetCallbackTimeout(timeInSec)
	self.callbackTimeout = timeInSec
end

function M:Send(name, args, callback, ...)
	if self.clientStatus ~= AutoReconnectTcpClient.Status.CONNECTED then
		warn("SOCKET NOT CONNECTED")
		return
	end

	self.session = self.session + 1
	if MockService.OnClientRequest(name, self, self.session) then
		return
	end

	local req = self.request(name, args, self.session)
	local package = spack(">s2", req)
	self.client:Send(package)

	local commonContext = self.responseCommonCallback[name]
	if callback or commonContext then
		local params = tpack(...)
		self.wait4Responses[self.session] = {
			name = name,
			callback = callback or commonContext.callback,
			params = params or commonContext.params,
			time = 0
		}
	end
	return self.session
end

function M:CancelResponseWaiting(session)
	self.wait4Responses[session] = nil
end

function M:DirectSend(buffer)
	if self.clientStatus ~= AutoReconnectTcpClient.Status.CONNECTED then
		warn("SOCKET NOT CONNECTED")
		return
	end
	self.client:Send(buffer)
end

function M:OnStatusChanged(status)
	self.clientStatus = status
	if status == AutoReconnectTcpClient.Status.RECONNECT then
		self.wait4Responses = {}
		self.session = 1
	end
	self:DispatchEvent(M.Event.StatusChanged, status)
end

function M:_OnServerRequest(name, args)
	local serverReq = self.serverRequest[name]
	if not serverReq then
		warn("UNHANDLED SERVER REQUEST", name)
		return
	end
	if serverReq.params.n == 0 then
		serverReq.callback(args)
	else
		serverReq.callback(tunpack(serverReq.params), args)
	end
end

function M:OnRecvPackage(buffer)
	local t, name, args = self.host:dispatch(buffer)
	if t == "REQUEST" then
		self:_OnServerRequest(name, args)
	elseif t == "RESPONSE" then
		self:_OnResponse(name, args)
	end
end

function M:_OnResponse(name, args)
	local response = self.wait4Responses[name]
	if not response then
		return
	end

	local commonContext = self.responseCommonCallback[response.name]
	if commonContext and response.callback ~= commonContext.callback then
		if commonContext.params.n == 0 then
			commonContext.callback(args)
		else
			commonContext.callback(tunpack(commonContext.params), args)
		end
	end
	self.wait4Responses[name] = nil
	if response.params.n == 0 then
		response.callback(args)
	else
		response.callback(tunpack(response.params), args)
	end

	MockService.OnServerResponse(response.name, self)
end

function M:OnUpdate()
	if not next(self.wait4Responses) then
		return
	end

	for _, wait4Response in pairs(self.wait4Responses) do
		wait4Response.time = wait4Response.time + 0.1
		if wait4Response.time > self.callbackTimeout then
			warn("WAIT FOR RESPONSE TIMEOUT:", wait4Response.name)
			self.client.TcpStatus = AutoReconnectTcpClient.Status.DISCONNECT
		end
	end
end

return M
