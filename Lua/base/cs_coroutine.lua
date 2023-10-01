local util = require 'util'
local GlobalCoroutineRunnerService = CS.Extend.Common.GlobalCoroutineRunnerService.Get()

return {
	start = function(...)
		return GlobalCoroutineRunnerService:StartCoroutine(util.cs_generator(...))
	end,

	stop = function(coroutine)
		GlobalCoroutineRunnerService:StopCoroutine(coroutine)
	end
}
