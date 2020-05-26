local M = {}
local layers = {}
local bgFx = {}
local table, assert, typeof, pairs, ipairs = table, assert, typeof, pairs, ipairs
---@type CS.Extend.Asset.AssetService
local AssetService = CS.Extend.Asset.AssetService.Get()
local Object = CS.UnityEngine.Object
local UILayer = CS.Extend.UI.UILayer
local sequence = require("base.action.sequence")
local sortedElements = {}
local UIViewBaseType = typeof(CS.Extend.UI.UIViewBase)

---@type CS.Extend.UI.UIViewConfiguration
local UIViewConfiguration

function M.Init()
	UIViewConfiguration = CS.Extend.UI.UIViewConfiguration.Load()
	local ref = AssetService:Load("UILayers", typeof(CS.UnityEngine.GameObject))
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

function M._AddElement(element)
	for i, v in ipairs(sortedElements) do
		if v.view.Canvas.sortingOrder > element.view.Canvas.sortingOrder then
			table.insert(sortedElements, i, element)
		end
	end
	table.insert(sortedElements, element)
end

function M._Index(element)
	for i, v in ipairs(sortedElements) do
		if v == element then
			return i
		end
	end
	return -1
end

function M.Show(viewName, callback)
	local configuration = UIViewConfiguration:GetOne(viewName)
	local seq = sequence.new()
	local behaviour = seq:build()

	local context = {}
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

	if configuration.FullScreen then
		behaviour:custom(function()
			for i = 1, M._Index(context) - 1 do
				local element = sortedElements[i]
				element.view:SetVisible(false)
			end
		end)
	else
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
end

---@param view CS.Extend.UI.UIViewBase
function M:Hide(view)
	local callback
	callback = function()
		view:Hidden("-", callback)
		AssetService:Recycle(view)
	end
	view:Hidden("+", callback)
	view:Hide()
end

return M