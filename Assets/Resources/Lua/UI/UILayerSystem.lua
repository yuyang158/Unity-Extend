local M = {}
local layers = {}
local table = table

function M.Init(transform)
	for i = 0, transform.childCount - 1 do
		local childLayer = transform:GetChild(i)
		table.insert(layers, {
			layerTransform = childLayer,
			layerGO = childLayer.gameObject,
			name = childLayer.name
		})
		childLayer.gameObject:SetActive(false)
	end
end

function M.Show()
	
end

return M