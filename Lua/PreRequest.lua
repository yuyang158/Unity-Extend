require('util')

function Global_ShowLogFile()
	local path = CS.UnityEngine.Application.persistentDataPath .. "/error.log"
	local f = io.open(path, "rb")
	local file = f:read("a")
	f:close()
	return file
end

function Global_DebugFunction(lua)
	local _, ret = xpcall(function()
		local func = load(lua)
		return func()
	end, debug.traceback)
	return tostring(ret)
end

local SM = require "ServiceManager"
local ConfigService = require "ConfigService"
local TS = require "TickService"

local CmdService = require("CommandService")
local UIService = require "UI.UIService"
local GlobalVMService = require "mvvm.GlobalVMService"
local MockService = require("base.MockService")
local EventBindingService = require("base.EventBindingService")

SM.RegisterService(SM.SERVICE_TYPE.CONFIG, ConfigService)
SM.RegisterService(SM.SERVICE_TYPE.TICK, TS)
SM.RegisterService(SM.SERVICE_TYPE.CONSOLE_COMMAND, CmdService)
SM.RegisterService(SM.SERVICE_TYPE.UI, UIService)
SM.RegisterService(SM.SERVICE_TYPE.GLOBAL_VM, GlobalVMService)
SM.RegisterService(SM.SERVICE_TYPE.MOCK, MockService)
SM.RegisterService(SM.SERVICE_TYPE.EVENT_BINDING, EventBindingService)

return {
	init = function()
		UIService.AfterSceneLoaded()
		-- UIService.Show("Login", function()
		-- end)
	end,
	shutdown = function()
		SM.Shutdown()
	end
}