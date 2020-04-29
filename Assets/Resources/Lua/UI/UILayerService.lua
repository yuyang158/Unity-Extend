local M = {}
local layers = {}
local table = table
---@type CS.Extend.Asset.AssetService
local AssetService

---@type CS.Extend.UI.UIViewConfiguration
local UIViewConfiguration
local UIViewBaseType = typeof(CS.Extend.UI.UIViewBase)

function M.Init()
	UIViewConfiguration = CS.Extend.UI.UIViewConfiguration.Load()

	local go = CS.UnityEngine.GameObject.Find("UI")
	CS.UnityEngine.Object.DontDestroyOnLoad(go)
	local transform = go.transform
	for i = 0, transform.childCount - 1 do
		local childLayer = transform:GetChild(i)
		local name = childLayer.name
		local layer = {
			layerTransform = childLayer,
			layerGO = childLayer.gameObject,
			name = name,
			elements = {}
		}
		-- childLayer.gameObject:SetActive(false)
		local layerEnum = assert(CS.Extend.UI.UILayer.__CastFrom(name), name)
		layers[layerEnum] = layer
	end
	AssetService = CS.Extend.Asset.AssetService.Get()
end

function M.Show(viewName)
	local configuration = UIViewConfiguration:GetOne(viewName)
	
	local layer = layers[configuration.AttachLayer]
	if #layer.elements == 0 then
		layer.layerGO:SetActive(true)
	end
	local go = configuration.UIView:Instantiate(layer.layerTransform)
	---@type CS.Extend.UI.UIViewBase
	local view = go:GetComponent(UIViewBaseType)
	view:Show()
	local hiddenCb
	hiddenCb = function()
		view:Hidden("-", hiddenCb)
		configuration.UIView:Dispose()
		CS.UnityEngine.Object.Destroy(go)
	end
	view:Hidden("+", hiddenCb)
	table.insert(layer.elements, go)

	return go
end

return M