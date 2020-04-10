local M = {}
local layers = {}
local table = table
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
	AssetService = CS.AssetService.Get()
end

function M.Show()
	
end

return M