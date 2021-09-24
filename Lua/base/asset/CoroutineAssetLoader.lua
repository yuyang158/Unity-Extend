local AssetService = CS.Extend.Asset.AssetService
local GameObjectType = typeof(CS.UnityEngine.GameObject)

---@class base.asset.CoroutineAssetLoader
local M = class()

---@return base.asset.CoroutineAssetLoader
function M.Create(performFunc)
	return M.new(performFunc)
end

function M:ctor(performFunc)
	self.co = coroutine.create(performFunc)
	self.loadedAssets = {}
	local ok, err = coroutine.resume(self.co, self)
	if not ok then
		error(err)
	end
end

---@return CS.Extend.Asset.AssetReference
function M:LoadAsset(path, assetType)
	local handle = AssetService.Get():LoadAsync(path, assetType)
	local asset
	handle:OnComplete("+", function()
		asset = handle.Result
		coroutine.resume(self.co)
	end)
	coroutine.yield(self.co)
	self.loadedAssets[path] = asset
	return asset
end

---@return CS.UnityEngine.GameObject[]
function M:InstantiatePrefab(pathOrAsset, count, parent)
	count = count or 1
	if type(pathOrAsset) == "string" then
		pathOrAsset = self:LoadAsset(pathOrAsset, GameObjectType)
	end

	local gameObjects = {}
	for _ = 1, count do
		local asyncContext = pathOrAsset:InstantiateAsync(parent, false)
		asyncContext:Callback("+", function(go)
			table.insert(gameObjects, go)
			if #gameObjects == count then
				coroutine.resume(self.co)
			end
		end)
	end
	coroutine.yield(self.co)
	pathOrAsset:Dispose()
	return gameObjects
end

return M