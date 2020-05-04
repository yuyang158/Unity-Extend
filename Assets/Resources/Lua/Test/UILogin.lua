---@class Test.UILogin
local M = class()
local SM = require "ServiceManager"

function M:awake()
	
end

function M:OnMessageButtonClicked()
	local uiService = SM.GetService(SM.SERVICE_TYPE.UI)
	uiService.Show("SingleMessageBox", function()
		
	end)
end
return M