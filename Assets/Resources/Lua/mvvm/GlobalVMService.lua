local assert = assert
---@class GlobalVMService
local M = {}
local binding = require("mvvm.binding")

local vms = {}
function M.Init()
end

function M.Register(name, context)
	local vm = binding.build(context)
	assert(not vms[name])
	vms[name] = vm
	return vm
end

function M.GetVM(name)
	return vms[name]
end

function M.Clear()
	vms = {}
end

return M