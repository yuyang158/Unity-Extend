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
			green = "Sprites/green",
			array = {
				{text = "a"},
				{text = "b"},
				{text = "c"},
				{text = "d"},
				{text = "e"},
				{text = "aa"},
				{text = "bsd"},
				{text = "cas"},
				{text = "ddasas"},
				{text = "aasdsd"},
				{text = "bsdc"},
				{text = "cvd"},
				{text = "dngh"},
				{text = "a111"},
				{text = "b22222"},
				{text = "c3333"},
				{text = "d4444"},
				{text = "e5555"},
				{text = "aa66666"},
				{text = "bsd7777"},
				{text = "cas8888"},
				{text = "ddasas9999999"},
				{text = "aasdsd11111"},
				{text = "bsdc22222"},
				{text = "cvd33333"},
				{text = "dngh4444"}
			}
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
	assert(false)
end
return M