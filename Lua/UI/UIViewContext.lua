---@class UI.UIViewContext
---@field view CS.Extend.UI.UIViewBase
---@field bg CS.Extend.UI.UIViewBase
local M = class()
local sequence = require("base.action.sequence")
local UIViewBaseType = typeof(CS.Extend.UI.UIViewBase)
---@type CS.Extend.Asset.AssetService
local AssetService = CS.Extend.Asset.AssetService
local UILayer = CS.Extend.UI.UILayer
---@type CS.Extend.UI.CloseOption
local CloseOption = CS.Extend.UI.CloseOption
local SM = require("ServiceManager")

---@param configuration CS.Extend.UI.UIViewConfiguration.Configuration
function M:ctor(configuration)
	self.configuration = configuration
	self.viewName = configuration.Name
	self.status = "none"
end

function M:Load(callback, layers)
	self.status = "loading"
	local seq = sequence.new(function()
		self.status = "loaded"
		callback(nil, self.view)
	end, function(err)
		error(err)
		self:Destroy()
		callback(err)
	end)
	local behaviour = seq:build()
	if self.configuration.Transition and self.configuration.Transition.GUIDValid then
		behaviour:instantiate_async(self.configuration.Transition, layers[UILayer.MostTop].transform, function(go)
			local view = go:GetComponent(UIViewBaseType)
			view.Canvas.overrideSorting = true
			view.Canvas.sortingOrder = layers[UILayer.MostTop].currentOrder
			self.transition = view
			return view
		end):view_show():wait_view_shown()
	end

	local layer = layers[self.configuration.AttachLayer]
	local sortingOrder = layer.currentOrder + 2
	layer.currentOrder = sortingOrder
	if not self.configuration.FullScreen then
		if self.configuration.BackgroundFx and self.configuration.BackgroundFx.GUIDValid then
			behaviour:instantiate_async(self.configuration.BackgroundFx, layer.transform, function(go)
				local bg = go:GetComponent(UIViewBaseType)
				bg.Canvas.overrideSorting = true
				bg.Canvas.sortingOrder = sortingOrder - 1
				go:SetActive(false)
				self.bg = bg
				return bg
			end)
		end
	end

	table.insert(layer.elements, self)
	behaviour:instantiate_async(self.configuration.UIView, layer.transform, function(go)
		local view = go:GetComponent(UIViewBaseType)
		view.Canvas.overrideSorting = true
		view.Canvas.sortingOrder = sortingOrder
		self.view = view
		

		if not self.configuration.FullScreen and self.configuration.CloseMethod == CloseOption.AnyWhere then
			self.tapScreenHandle = CS.ScreenTouchUtil.RequestTapCallback(function()
				self:Close()
			end, 1, false)
		end

		if self.configuration.CloseMethod == CloseOption.Button then
			local t = go.transform:Find(self.configuration.CloseButtonPath)
			self.closeGO = t.gameObject
			local EventBinding = SM.GetService(SM.SERVICE_TYPE.EVENT_BINDING)
			EventBinding.AddEventListener("OnClick", self.closeGO, M.Close, self)
		end
		return view
	end)
	behaviour:start()
end

function M:Show()
	assert(self.status == "loaded")
	self.status = "show"
	self.view:Show()
	local uiService = SM.GetService(SM.SERVICE_TYPE.UI)
	uiService._AddElement(self)

	if self.transition then
		self.transition:Hide(function()
			AssetService.Recycle(self.transition)
			self.configuration.Transition:Dispose()
		end)
	end
	if self.bg then
		self.bg:SetActive(true)
		self.bg:Show()
	end
end

function M:Hide()
	assert(self.status == "show")
	self.status = "hide"
	assert(SM.GetService(SM.SERVICE_TYPE.UI).GetContext(self.viewName) == nil, self.viewName)

	self.view:Hidden("+", function()
		self:Destroy()
	end)
	if self.bg then
		self.bg:Hide()
	end
	self.view:Hide()
end

function M:Close()
	---@type CS.Extend.LuaBinding
	local binding = self.view:GetComponent(typeof(CS.Extend.LuaBinding))
	if binding then
		local closeFunc = binding.LuaInstance.Close
		if closeFunc then
			closeFunc(binding.LuaInstance)
			return
		end
	end
	local uiService = SM.GetService(SM.SERVICE_TYPE.UI)
	uiService.Hide(self)
end

function M:Destroy()
	assert(SM.GetService(SM.SERVICE_TYPE.UI).GetContext(self.viewName) == nil, self.viewName)
	self.status = "destroyed"
	if self.bg then
		AssetService.Recycle(self.bg)
		self.configuration.BackgroundFx:Dispose()
	end

	if self.view then
		AssetService.Recycle(self.view)
		self.configuration.UIView:Dispose()
	end

	if self.tapScreenHandle then
		self.tapScreenHandle:Dispose()
		self.tapScreenHandle = nil
	end

	if self.closeGO then
		local EventBinding = SM.GetService(SM.SERVICE_TYPE.EVENT_BINDING)
		EventBinding.RemoveEventListener("OnClick", self.closeGO, M.Close)
		self.closeGO = nil
	end
end

function M:SetVisible(visible)
	self.view:SetVisible(visible)

	if self.bg then
		self.bg:SetVisible(visible)
	end
end

return M