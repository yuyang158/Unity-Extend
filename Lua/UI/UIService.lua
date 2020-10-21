---@class UI.UIService
local M = {}
local layers = {}
local table, assert, typeof, pairs, ipairs = table, assert, typeof, pairs, ipairs
local AssetService = CS.Extend.Asset.AssetService
---@type CS.Extend.Asset.AssetService
local AssetServiceInstance = AssetService.Get()
local Object = CS.UnityEngine.Object
local UILayer = CS.Extend.UI.UILayer
local sequence = require("base.action.sequence")
local sortedElements = {}
local UIViewBaseType = typeof(CS.Extend.UI.UIViewBase)

---@type CS.Extend.UI.UIViewConfiguration
local UIViewConfiguration

function M.Init()
	UIViewConfiguration = CS.Extend.UI.UIViewConfiguration.Load()
	local ref = AssetServiceInstance:Load("UILayers", typeof(CS.UnityEngine.GameObject))
	local go = ref:Instantiate()
	go.name = "UI"
	ref:Dispose()
	Object.DontDestroyOnLoad(go)
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
			baseSortingOrder = canvas.sortingOrder,
			canvas = canvas,
			layerIndex = i
		}
		canvas.enabled = false
		local layerEnum = assert(UILayer.__CastFrom(name), name)
		layers[layerEnum] = layer
	end

	M.SetUICamera(CS.UnityEngine.Camera.main)
end

function M.SetUICamera(camera)
	for _, layer in pairs(layers) do
		layer.canvas.worldCamera = camera
	end
end

local topFullScreenViewOrder = 0
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
	if element.configuration.FullScreen and order > topFullScreenViewOrder then
		topFullScreenViewOrder = order
		for i = 1, insertIndex - 1 do
			local context = sortedElements[i]
			context.view:SetVisible(false)
			if context.bg then
				context.bg:SetVisible(false)
			end
		end
	end
end

function M._RemoveElement(element)
	local index
	for i, v in ipairs(sortedElements) do
		if v == element then
			table.remove(sortedElements, i)
			index = i
		end
	end
	assert(index)
	if element.configuration.FullScreen and element.view.Canvas.sortingOrder >= topFullScreenViewOrder then
		for i = index - 1, 1, -1 do
			local context = sortedElements[i]
			context.view:SetVisible(true)
			if context.bg then
				context.bg:SetVisible(true)
			end
			if context.configuration.FullScreen then
				topFullScreenViewOrder = context.view.Canvas.sortingOrder
				break
			end
		end
	end
end

function M.Show(viewName, callback)
	local configuration = UIViewConfiguration:GetOne(viewName)
	local context = { configuration = configuration }
	local seq = sequence.new(function()
		M._AddElement(context)
	end, function(err)
		error(err)
		if context.transition then
			AssetService.Recycle(context.transition)
		end
		if context.view then
			AssetService.Recycle(context.view)
		end
		if context.bg then
			AssetService.Recycle(context.bg)
		end
	end)
	local behaviour = seq:build()
	if configuration.Transition and configuration.Transition.GUIDValid then
		behaviour:instantiate(configuration.Transition, layers[UILayer.MostTop].transform, function(go)
			local view = go:GetComponent(UIViewBaseType)
			view.Canvas.overrideSorting = true
			view.Canvas.sortingOrder = layers[UILayer.MostTop].baseSortingOrder
			context.transition = view
			return view
		end):view_show():wait_view_shown()
	end

	local layer = layers[configuration.AttachLayer]
	behaviour:instantiate(configuration.UIView, layer.transform, function(go)
		local view = go:GetComponent(UIViewBaseType)
		view.Canvas.overrideSorting = true
		view.Canvas.sortingOrder = layer.baseSortingOrder + #layer.elements * 2
		context.view = view
		callback(view)
		return view
	end):view_show(function()
		if context.transition then
			context.transition:Hide()
			context.transition = nil
		end
	end)

	if not configuration.FullScreen then
		if configuration.BackgroundFx and configuration.BackgroundFx.GUIDValid then
			behaviour:instantiate(configuration.BackgroundFx, layer.transform, function(go)
				local bg = go:GetComponent(UIViewBaseType)
				bg.Canvas.overrideSorting = true
				bg.Canvas.sortingOrder = context.view.Canvas.sortingOrder - 1
				context.bg = bg
				return bg
			end):view_show(context.bg)
		end
	end
	behaviour:start()
	return context
end

function M.Hide(context)
	context.view:Hide()
	if context.bg then
		context.bg:Hide()
	end
	M._RemoveElement(context)
end
return M