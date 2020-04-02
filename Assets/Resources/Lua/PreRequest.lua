package.cpath = package.cpath .. string.format(';%s%s/.Rider2019.3/config/plugins/intellij-emmylua/classes/debugger/emmy/windows/x64/?.dll',
		os.getenv("HOMEDRIVE"), os.getenv("HOMEPATH"))
local dbg = require('emmy_core')
dbg.tcpConnect('localhost', 9988)

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
local CS = require "ConfigService"
local TS = require "TickService"
local CmdService = require("CommandService")

SM.RegisterService(SM.SERVICE_TYPE.CONFIG, CS)
SM.RegisterService(SM.SERVICE_TYPE.TICK, TS)
SM.RegisterService(SM.SERVICE_TYPE.CONSOLE_COMMAND, CmdService)

return function()
	CS.clear()
	dbg.stop()
end