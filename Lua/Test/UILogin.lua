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
local LuaMVVMBindingType = typeof(CS.Extend.LuaMVVM.LuaMVVMBinding)

function M:start()
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
	
	warn("CLICK ...")

	if #self.context.values > 5 then
		table.remove(self.context.values)
	else
		table.insert(self.context.values, math.random(1, 10))
	end
	
	local go = CS.UnityEngine.GameObject.Find("CharacterView")
	local vm = {
		data = {
			name = "Jack",
			id = math.random(100000),
			level = math.random(100),
			title = (math.random(100) % 2 == 0) and "None" or "ASDF",
			guildName = "Top 1",
			camp = "123123123",
			friend = "Test",
			exp = 300,
			levelExp = 900,
			power = math.random(123123),
			equipLevel = math.random(999),
			basicProp = {
				value1 = math.random(999),
				value2 = math.random(999),
				value3 = math.random(999),
				value4 = math.random(999)
			}
		}
	}
	binding.build(vm)
	local b = go:GetComponent(LuaMVVMBindingType)
	b:SetDataContext(vm)
end
return M