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

---@param view CS.Extend.UI.UIViewBase
function M:view_show(view)
	self.sequence:add({
		active = function()
			view:Show()
			self.sequence:next()
		end
	})
	return self
end

---@param view CS.Extend.UI.UIViewBase
function M:view_hide(view)
	self.sequence:add({
		active = function()
			view:Hide()
			self.sequence:next()
		end
	})
	return self
end

---@param view CS.Extend.UI.UIViewBase
function M:wait_view_shown(view)
	local callback
	callback = function()
		self.sequence:next()
		view.Shown("-", callback)
	end
	self.sequence:add({
		active = function()
			view.Shown("+", callback)
		end
	})
	return self
end

---@param view CS.Extend.UI.UIViewBase
function M:wait_view_hidden(view)
	local callback
	callback = function()
		self.sequence:next()
		view.Hidden("-", callback)
	end
	self.sequence:add({
		active = function()
			view.Hidden("+", callback)
		end
	})
	return self
end

function M:instantiate(asset, parent, callback) 
	self.sequence:add({
		active = function()
			local go = asset:Instantiate(parent)
			callback(go)
		end
	})
	return self
end

function M:custom(callback)
	self.sequence:add({
		active = callback
	})
	return self
end

function M:start()
	self.sequence:next()
end

return M