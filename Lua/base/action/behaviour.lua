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
			view:Shown("+", function()
				if on_shown then
					on_shown()
				end
				self.sequence:next(view)
			end)
		end
	})
	return self
end

function M:wait_view_hidden()
	self.sequence:add({
		active = function(view)
			view:Hidden("+", function()
				self.sequence:next()
			end)
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

function M:instantiate_async(asset, parent, callback)
	self.sequence:add({
		active = function()
			local ctx = asset:InstantiateAsync(parent)
			local instantiateCallback; instantiateCallback = function(go)
				local param = callback(go)
				self.sequence:next(param)
				ctx:Callback("-", instantiateCallback)
			end
			ctx:Callback("+", instantiateCallback)
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

local SM = require("ServiceManager")

---@param callback function
function M:tick_and_wait(callback)
	self.sequence:add({
		active = function(...)
			self.totalTime = 0
			local TickService = SM.GetService(SM.SERVICE_TYPE.TICK)
			TickService.Register(self.tick, self)
			self.tickCallback = callback
		end
	})
	return self
end

function M:repeat_times(times, seq)
	assert(times > 0)
	self.sequence:add({
		active = function(...)
			local index = 1
			seq.on_complete = function()
				if index < times then
					index = index + 1
					seq:reset()
					seq:next()
				else
					self.sequence:next()
				end
			end
			seq:next()
		end
	})
	
	return self
end

function M:interrupt(callback)
	self.sequence:add({
		active = function(...)
			if callback() then
				self.sequence:interrupt()
			end
			self.sequence:next(...)
		end
	})
	return self
end

function M:tick(deltaTime)
	if not self.tickCallback then
		if self.sequence:finished() then
			local TickService = SM.GetService(SM.SERVICE_TYPE.TICK)
			TickService.Unregister(self.tick)
		end
		return
	end
	self.totalTime = self.totalTime + deltaTime
	if self.tickCallback(deltaTime, self.totalTime) then
		self.sequence:next()
	end
end

function M:start()
	self.sequence:next()
end

return M