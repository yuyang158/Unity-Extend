---@class UI.UIService
local M = {}
local layers = {}
local table, assert, typeof, pairs, ipairs = table, assert, typeof, pairs, ipairs
local AssetService = CS.Extend.Asset.AssetService
---@type CS.Extend.Asset.AssetService
local AssetServiceInstance = AssetService.Get()
local UILayer = CS.Extend.UI.UILayer
---@type CS.Extend.UI.CloseOption
local CloseOption = CS.Extend.UI.CloseOption
---@type UI.UIViewContext[]
local sortedElements = {}
---@type table<string, UI.UIViewContext>
local contexts = {}
local UIViewContext = require("UI.UIViewContext")
local SM = require("ServiceManager")

---@type CS.Extend.UI.UIViewConfiguration
local UIViewConfiguration
local UIRoot

function M.Init()
	UIViewConfiguration = CS.Extend.UI.UIViewConfiguration.Load()
	local ref = AssetServiceInstance:Load("UILayers", typeof(CS.UnityEngine.GameObject))
	local go = ref:Instantiate()
	UIRoot = go
	go.name = "UI"
	ref:Dispose()
	CS.UnityEngine.Object.DontDestroyOnLoad(go)
	local transform = go.transform
	for i = 0, transform.childCount - 1 do
		local childLayer = transform:GetChild(i)
		local canvas = childLayer:GetComponent(typeof(CS.UnityEngine.Canvas))
		local name = childLayer.name
		local layer = {
			transform = childLayer,
			go = childLayer.gameObject,
			name = name,
			elements = {},
			currentOrder = canvas.sortingOrder,
			canvas = canvas,
			layerIndex = i
		}
		--canvas.enabled = false
		local layerEnum = assert(UILayer.__CastFrom(name), name)
		layers[layerEnum] = layer
	end
end

function M.GetLayerRoot(name)
	local layerEnum = assert(UILayer.__CastFrom(name), name)
	return layers[layerEnum].transform
end

function M.GetLoadedUI(name)
	local context = contexts[name]
	if not context then
		return
	end
	if context.status ~= "loaded" and context.status ~= "show" then
		return
	end
	return context.view
end

function M.AfterSceneLoaded()
	M.SetUICamera(CS.UnityEngine.Camera.main)
end

function M.SetUICamera(camera)
	for _, layer in pairs(layers) do
		layer.canvas.worldCamera = camera
	end
	local t = M.GetLayerRoot("Scene")
	t.forward = -camera.transform.forward
end

local topFullScreenViewOrder = 0
---@param element UI.UIViewContext
function M._AddElement(element)
	local order = element.view.Canvas.sortingOrder
	local insertIndex
	for i, v in ipairs(sortedElements) do
		if v.view.Canvas.sortingOrder > order then
			table.insert(sortedElements, i, element)
			insertIndex = i
			break
		end
	end
	if not insertIndex then
		table.insert(sortedElements, element)
		insertIndex = #sortedElements
	end
	if element.configuration.FullScreen then
		if order > topFullScreenViewOrder then
			topFullScreenViewOrder = order
			for i = 1, insertIndex - 1 do
				local context = sortedElements[i]
				context.view:SetVisible(false)
				if context.bg then
					context.bg:SetVisible(false)
				end
			end
		end
	else
		if element.configuration.CloseMethod == CloseOption.Outside then
			assert(element.bg)
			local EventBinding = SM.GetService(SM.SERVICE_TYPE.EVENT_BINDING)
			local close; close = function()
				EventBinding.RemoveEventListener("OnClick", element.bg.gameObject, close)
				---@type CS.Extend.LuaBinding
				local binding = element.view:GetComponent(typeof(CS.Extend.LuaBinding))
				if binding then
					local closeFunc = binding.LuaInstance.Close
					if closeFunc then
						closeFunc(binding.LuaInstance)
						return
					end
				end

				M.Hide(element)
			end
			EventBinding.AddEventListener("OnClick", element.bg.gameObject, close)
		end
	end
end

function M._RemoveElement(element)
	local index
	for i, v in ipairs(sortedElements) do
		if v == element then
			table.remove(sortedElements, i)
			index = i
			break
		end
	end
	assert(index, element)
	--[[ local showingCount = #sortedElements
	if showingCount == 0 then
		return
	end
	if showingCount == index - 1 then
		local topElement = sortedElements[showingCount]
		if not topElement.configuration.FullScreen then
			if topElement.configuration.CloseMethod == CloseOption.AnyWhere then

			end
		end
	end]]

	if element.configuration.FullScreen and element.view.Canvas.sortingOrder >= topFullScreenViewOrder then
		for i = index - 1, 1, -1 do
			local context = sortedElements[i]
			context.view:SetVisible(true)
			if context.bg then
				context.bg:SetVisible(true)
			end
			if context.configuration.FullScreen then
				topFullScreenViewOrder = context.view.Canvas.sortingOrder
				return
			end
		end
		topFullScreenViewOrder = 0
	end
end

function M.Load(viewName, callback)
	if contexts[viewName] then
		warn("ui view is exist : ", viewName)
		return
	end

	local configuration = UIViewConfiguration:GetOne(viewName)
	local context = UIViewContext.new(configuration)
	context:Load(function(err, go)
		callback(err, go)
	end, layers)
	contexts[viewName] = context
	return context
end

function M.Show(viewName, callback)
	local context;
	context = M.Load(viewName, function(err, go)
		context:Show()
		callback(err, go)
	end)
	return context
end

function M.Hide(context)
	if type(context) == "string" then
		local viewName = context
		context = contexts[viewName]
		if not context then
			warn("View " .. viewName .. "not exist")
			return
		end
		contexts[viewName] = nil
	else
		contexts[context.viewName] = nil
	end
	local layer = layers[context.configuration.AttachLayer]
	for i, v in ipairs(layer.elements) do
		if v == context then
			table.remove(layer.elements, i)
			break
		end
	end

	context:Hide()
	M._RemoveElement(context)
end

function M.clear()
	for _, context in pairs(contexts) do
		context:Destroy()
	end
	AssetService.Recycle(UIRoot)
	contexts = nil
end

return M