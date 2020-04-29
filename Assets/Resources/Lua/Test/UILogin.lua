---@class Test.UILogin
local M = class()
local UILayerService = require "UI.UILayerService"

function M:awake()
	
end

function M:OnMessageButtonClicked()
	UILayerService.Show("SingleMessageBox")
end
return M