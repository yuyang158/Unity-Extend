---@class Test.UILogin
local M = class()
local binding = require("mvvm.binding")

function M:start()
	local LuaMVVMBindingType = typeof(CS.Extend.LuaMVVM.LuaMVVMBinding)
	self.mvvmBinding = self.__CSBinding:GetComponent(LuaMVVMBindingType)
	self.context = {
		data = {
			title = "TITLE",
			progress = 0.5,
			on = true,
			input = "Place holder",
			red = "Sprites/red",
			green = "Sprites/green"
		}
	}
	binding.build(self.context)
	self.mvvmBinding:SetDataContext(self.context)
end

function M:OnMessageButtonClicked()
	--[[local uiService = SM.GetService(SM.SERVICE_TYPE.UI)
	uiService.Show("SingleMessageBox", function()
		
	end)]]

	self.context.red = "Sprites/green"
	self.context.green = "Sprites/red"

	self.context.progress = math.random()
	self.context.on = not self.context.on
end
return M