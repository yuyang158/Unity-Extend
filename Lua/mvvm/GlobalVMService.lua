local assert = assert
---@class GlobalVMService
local M = {}
local binding = require("mvvm.binding")
local util = require("util")

local vms = {}
function M.Init()
end

function M.Register(name, context)
	local vm = binding.build(context)
	assert(not vms[name])
	vms[name] = vm
	return vm
end

function M.GetVM(path)
	local paths = util.parse_path(path)
	local current = vms
	for _, v in ipairs(paths) do
		current = assert(current[v], v)
	end
	return current
end

function M.Clear()
	vms = {}
end

return M