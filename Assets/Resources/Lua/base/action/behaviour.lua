local GlobalCoroutineRunnerService = CS.Extend.Common.GlobalCoroutineRunnerService.Get()

---@class behaviour
local M = class()

---@param sequence sequence
function M:ctor(sequence)
	self.sequence = sequence
end

function M:wait_for_second(seconds)
	self.sequence:add({
		active = function()
			GlobalCoroutineRunnerService:WaitSecond(seconds, function()
				self.sequence:next()
			end)
		end
	})
	return self
end

function M:view_show(callback)
	self.sequence:add({
		active = function(view)
			if callback then
				callback()
			end
			self.sequence:next(view)
			view:Show()
		end
	})
	return self
end

function M:view_hide()
	self.sequence:add({
		active = function(view)
			self.sequence:next(view)
			view:Hide()
		end
	})
	return self
end

function M:wait_view_shown(on_shown)
	self.sequence:add({
		active = function(view)
			local callback
			callback = function()
				if on_shown then
					on_shown()
				end
				view:Shown("-", callback)
				self.sequence:next(view)
			end
			view:Shown("+", callback)
		end
	})
	return self
end

function M:wait_view_hidden()
	self.sequence:add({
		active = function(view)
			local callback
			callback = function()
				view:Hidden("-", callback)
				self.sequence:next()
			end
			view:Hidden("+", callback)
		end
	})
	return self
end


function M:instantiate(asset, parent, callback) 
	self.sequence:add({
		active = function()
			local go = asset:Instantiate(parent)
			local param = callback(go)
			self.sequence:next(param)
		end
	})
	return self
end

---@param callback function
function M:custom(callback)
	self.sequence:add({
		active = function(...)
			callback(...)
			self.sequence:next(...)
		end
	})
	return self
end

function M:start()
	self.sequence:next()
end

return M