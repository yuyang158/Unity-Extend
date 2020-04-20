local M = {}
local layers = {}
local table = table
---@type CS.Extend.AssetService.AssetService
local AssetService

function M.Init(transform)
	for i = 0, transform.childCount - 1 do
		local childLayer = transform:GetChild(i)
		local layer = {
			layerTransform = childLayer,
			layerGO = childLayer.gameObject,
			name = childLayer.name
		}
		table.insert(layers, layer)
		childLayer.gameObject:SetActive(false)
	end
	AssetService = CS.Extend.AssetService.AssetService.Get()
end

function M.Show(viewName, ...)
	local packed = table.pack(...)
	
end

return M