local AssetService = CS.Extend.Asset.AssetService
local GameObjectType = "LoadGameObjectAsync"
local util = require("util")

---@class base.asset.CoroutineAssetLoader
local M = class()

---@return base.asset.CoroutineAssetLoader
---@param performFunc CoroutineLoadPerform
function M.Create(performFunc)
	return M.new(performFunc)
end

---@alias CoroutineLoadPerform fun(this: base.asset.CoroutineAssetLoader)
---@param performFunc CoroutineLoadPerform
function M:ctor(performFunc)
	self.co = coroutine.create(performFunc)
	self.loadedAssets = {}
	util.catch_resume(self.co, self)
end

---@return CS.Extend.Asset.AssetReference
function M:LoadAsset(path, assetType)
	local service = AssetService.Get()
	local handle = service[assetType](service, path)
	local asset
	handle:OnComplete("+", function()
		asset = handle.Result
		util.catch_resume(self.co)
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
				util.catch_resume(self.co)
			end
		end)
	end
	coroutine.yield(self.co)
	pathOrAsset:Dispose()
	return gameObjects
end

return M