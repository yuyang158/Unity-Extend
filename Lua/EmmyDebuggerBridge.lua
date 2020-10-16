local dbg
return {
	init = function(searchPath)
		package.cpath = package.cpath .. searchPath
		dbg = require "emmy_core"
	end,
	connect = function(port)
		if not dbg then
			return
		end
		dbg.tcpConnect("localhost", port)
	end,
	listen = function(port)
		if not dbg then
			return
		end
		dbg.tcpListen("localhost", port)
	end,
	["break"] = function()
		if not dbg then
			return
		end
		dbg.breakHere()
	end
}