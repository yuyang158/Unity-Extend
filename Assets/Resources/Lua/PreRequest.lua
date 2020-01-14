package.cpath = package.cpath .. string.format(';%s%s/.Rider2019.3/config/plugins/intellij-emmylua/classes/debugger/emmy/windows/x64/?.dll', 
        os.getenv("HOMEDRIVE"), os.getenv("HOMEPATH"))
local dbg = require('emmy_core')
dbg.tcpConnect('localhost', 9967)

require('util')

local SM = require "ServiceManager"
local CS = require "ConfigService"
local TS = require "TickService"
local sproto = require("sproto/sproto")
print("sproto", sproto)


SM.RegisterService(SM.SERVICE_TYPE.CONFIG, CS)
SM.RegisterService(SM.SERVICE_TYPE.TICK, TS)

TS.Register(function()
end)