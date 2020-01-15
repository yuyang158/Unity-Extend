---@class SocketClient
local M = class()
local AutoReconnectTcpClient = CS.Extend.Network.SocketClient.AutoReconnectTcpClient 
function M:ctor()
    self.client = AutoReconnectTcpClient(self)
end

function M:Connect(host, port)
    self.client:Connect(host, port)
end

function M:OnStatusChanged(status)
    
end

return M