local EventDispatcher = require("base.EventDispatcher")
---@class base.GlobalEventDispatcher
local M = {}
local global

function M.Init()
	global = EventDispatcher.new() 
end

function M.clear()
	global = nil
end

---@return base.EventDispatcher
function M.GetGlobalDispatcher()
	return global
end

return M