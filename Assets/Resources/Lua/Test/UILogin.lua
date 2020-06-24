---@class Test.UILogin
---@field allowEmptyUsername boolean
---@field image CS.UnityEngine.UI.Image
---@field go CS.UnityEngine.GameObject
---@field button CS.UnityEngine.UI.Button
---@field lobbyName string
local M = class()
local binding = require("mvvm.binding")
local SprotoClient = require("sproto.SprotoClient")
local SM = require "ServiceManager"

function M:start()
	local LuaMVVMBindingType = typeof(CS.Extend.LuaMVVM.LuaMVVMBinding)
	self.mvvmBinding = self.__CSBinding:GetComponent(LuaMVVMBindingType)
	self.context = {
		data = {
			username = "",
			network = {
				status = "wait for connection"
			},
			values = {}
		},
		computed = {
			sum = function(this)
				local total = 0
				for _, v in ipairs(this.values) do
					total = total + v
				end
				return total
			end
		}
	}
	binding.build(self.context)
	self.mvvmBinding:SetDataContext(self.context)
	self.tcpClient = SprotoClient.new("Config/c2s", "Config/s2c")
	self.tcpClient:AddEventListener(SprotoClient.Event.StatusChanged, function(status)
		if status == CS.Extend.Network.SocketClient.AutoReconnectTcpClient.Status.CONNECTED then
			self.context.status = "connected"
			self.tcpClient:Send("login", {username = self.context.username}, function(response)
				---@type GlobalVMService
				local globalVM = SM.GetService(SM.SERVICE_TYPE.GLOBAL_VM)
				globalVM.Register("user", {data = response})
			end)
		end
	end)
end

function M:OnLoginClicked()
	--[[local system = CS.Extend.GameSystem.Get()
	local host = system.SystemSetting:GetString("GAME", "ServerHost")
	local port = system.SystemSetting:GetInt("GAME", "ServerPort")
	self.tcpClient:Connect(host, port)]]

	if #self.context.values > 5 then
		table.remove(self.context.values)
	else
		table.insert(self.context.values, math.random(1, 10))
	end
end
return M