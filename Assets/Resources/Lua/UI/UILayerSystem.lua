local M = {}
local layers = {}

function M.Init(transform)
	for i = 0, transform.childCount - 1 do
		local childLayer = transform:GetChild(i)
		table.insert(layers, {
			layerTransform = childLayer,
			name = childLayer.name
		})
	end
end

return M