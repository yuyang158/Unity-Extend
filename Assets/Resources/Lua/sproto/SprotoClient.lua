---@class SocketClient
local M = class()
local AutoReconnectTcpClient = CS.Extend.Network.SocketClient.AutoReconnectTcpClient
local sproto = require 'sproto.sproto'
local tunpack, tpack, next, spack = table.unpack, table.pack, next, string.pack
function M:ctor()
    self.client = AutoReconnectTcpClient(self)
    local assetService = CS.Extend.AssetService.AssetService.Get()
    local textAssetType = typeof(CS.UnityEngine.TextAsset)
    local proto = {
        c2s = assetService:Load('Config/c2s', textAssetType):GetTextAsset(),
        s2c = assetService:Load('Config/s2c', textAssetType):GetTextAsset()
    }
    local sprotoparser = require "sproto.sprotoparser"
    proto.c2s = sprotoparser.parse(proto.c2s.text)
    proto.s2c = sprotoparser.parse(proto.s2c.text)
    
    self.host = sproto.new(proto.s2c):host('package')
    self.request = self.host:attach(sproto.new(proto.c2s))
    self.session = 1
    self.callbackTimeout = 2
    self.wait4Responses = {}
end

function M:Connect(host, port)
    self.client:Connect(host, port)
end

function M:Close()
    self.client:Close()
end

function M:SetCallbackTimeout(timeInSec)
    self.callbackTimeout = timeInSec
end

function M:Send(name, args, callback, ...)
    self.session = self.session + 1
    local req = self.request(name, args, self.session)

    local package = spack(">s2", req)
    self.client:Send(package)
    if callback then
        local params = tpack(...)
        self.wait4Responses[self.session] = {
            callback = callback,
            params = params,
            time = 0
        }
    end
end

function M:DirectSend(buffer)
    self.client:Send(buffer)
end

function M:OnStatusChanged(status)
    print(status)
end

function M:OnRecvPackage(buffer)
    local t, name, args = self.host:dispatch(buffer)
    if t == "REQUEST" then
        print(t, name, args)
    elseif t == "RESPONSE" then
        local response =  self.wait4Responses[name]
        if not response then
            return
        end
        if response.params.n == 0 then
            response.callback(args)
        else
            response.callback(tunpack(response.params), args)
        end
    end
end

function M:OnUpdate(delta)
    if not next(self.wait4Responses) then
        return
    end

    for _, wait4Response in pairs(self.wait4Responses) do
        wait4Response.time = wait4Response.time + delta
        if wait4Response.time > self.callbackTimeout then
            -- self.client.TcpStatus = 
        end
    end
end

return M