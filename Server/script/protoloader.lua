local skynet = require "skynet"
local sprotoparser = require "sprotoparser"
local sprotoloader = require "sprotoloader"

skynet.start(function()
	local c2s = io.open("../proto/c2s.sproto")
	c2s = sprotoparser.parse(c2s:read("a"))

	local s2c = io.open("../proto/s2c.sproto")
	s2c = sprotoparser.parse(s2c:read("a"))

	sprotoloader.save(c2s, 1)
	sprotoloader.save(s2c, 2)
	-- don't call skynet.exit() , because sproto.core may unload and the global slot become invalid
end)