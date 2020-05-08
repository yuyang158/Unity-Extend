local M = {}
local layers = {}
local bgFx = {}
local table, assert, typeof = table, assert, typeof
---@type CS.Extend.Asset.AssetService
local AssetService = CS.Extend.Asset.AssetService.Get()
local Object = CS.UnityEngine.Object

---@type CS.Extend.UI.UIViewConfiguration
local UIViewConfiguration
local UIViewBaseType = typeof(CS.Extend.UI.UIViewBase)

function M.Init()
	UIViewConfiguration = CS.Extend.UI.UIViewConfiguration.Load()
	local GameObject = CS.UnityEngine.GameObject
	local uiCam = GameObject.Find("UICamera"):GetComponent(typeof(CS.UnityEngine.Camera))
	local ref = AssetService:Load("UILayers", typeof(CS.UnityEngine.GameObject))
	local go = ref:Instantiate()
	go.name = "UI"
	ref:Dispose()
	Object.DontDestroyOnLoad(go)
	local transform = go.transform
	for i = 0, transform.childCount - 1 do
		local childLayer = transform:GetChild(i)
		local canvas = childLayer:GetComponent(typeof(CS.UnityEngine.Canvas))
		canvas.worldCamera = uiCam
		local name = childLayer.name
		local layer = {
			layerTransform = childLayer,
			layerGO = childLayer.gameObject,
			name = name,
			elements = {},
			baseSortingOrder = canvas.sortingOrder,
			canvas = canvas,
			layerIndex = i
		}
		canvas.enabled = false
		local layerEnum = assert(CS.Extend.UI.UILayer.__CastFrom(name), name)
		layers[layerEnum] = layer
	end
end

local function hideFullScreen(layer, shownView)
	for elementIndex = 1, #layer.elements do
		local view = layer.elements[elementIndex].view
		if view ~= shownView then
			view:SetVisible(false)
		end
	end
	for i = layer.layerIndex - 1, 0, -1 do
		local layerEnum = CS.Extend.UI.UILayer.__CastFrom(i)
		local currentLayer = layers[layerEnum]
		for elementIndex = 1, #currentLayer.elements do
			local view = currentLayer.elements[elementIndex].view
			view:SetVisible(false)
		end
	end
end

---@param configuration CS.Extend.UI.UIViewConfiguration.Configuration
local function loadView(configuration, callback)
	local layer = layers[configuration.AttachLayer]
	if #layer.elements == 0 then
		layer.canvas.enabled = true
	end
	local go = configuration.UIView:Instantiate(layer.layerTransform)
	---@type CS.Extend.UI.UIViewBase
	local view = go:GetComponent(UIViewBaseType)
	local hiddenCb
	hiddenCb = function()
		view:Hidden("-", hiddenCb)
		configuration.UIView:Dispose()
		local index = table.index_of_predict(layer.elements, function(element)
			return element.go == go
		end)
		if index > 0 then
			table.remove(layer.elements, index)
		end
		Object.Destroy(go)
	end
	view:Hidden("+", hiddenCb)
	table.insert(layer.elements, {go = go, view = view})
	if callback then
		callback(go, view)
	end

	if configuration.FullScreen then
		local shownCb
		shownCb = function()
			view:Shown("-", shownCb)
			hideFullScreen(layer, view)
		end
		view:Shown("+", shownCb)
	end

	view.canvas.overrideSorting = true
	view.canvas.sortingOrder = layer.baseSortingOrder + #layer.elements * 2
	view:Show()
	return view
end

---@param assetRef CS.Extend.Asset.AssetReference
---@param foreView CS.Extend.UI.UIViewBase
local function loadBgFx(assetRef, foreView)
	local view = bgFx[assetRef]
	if not view then
		local go = assetRef:Instantiate(foreView.transform.parent)
		view = go:GetComponent(UIViewBaseType)
		view:Show()
	end
	
	view.canvas.overrideSorting = true
	view.canvas.sortingOrder = foreView.canvas.sortingOrder - 1
	return view
end

function M.Show(viewName, callback)
	local configuration = UIViewConfiguration:GetOne(viewName)
	if configuration.FullScreen then
		if configuration.Transition and configuration.Transition.GUIDValid then
			loadView({
				UIView = configuration.Transition,
				FullScreen = true,
				AttachLayer = CS.Extend.UI.UILayer.Transition
			}, function(_, view)
				local onShown
				onShown = function()
					view:Shown("-", onShown)
					view:Hide()
					loadView(configuration, callback)
				end
				view:Shown("+", onShown)
			end)
		else
			loadView(configuration, callback)
		end
	else
		local view = loadView(configuration, callback)
		if configuration.BackgroundFx and configuration.BackgroundFx.GUIDValid then
			loadBgFx(configuration.BackgroundFx, view)
		end
	end
end

return M