local SM = require "ServiceManager"
---@type CS.Extend.Asset.AssetService
local AssetService = CS.Extend.Asset.AssetService

---@class LuaBinding
---@field public __CSBinding CS.Extend.LuaBinding
---@field public name string
---@field public fullname string
local M = class()
---@alias AssetReferenceLoadCallback fun(ref: CS.Extend.Asset.AssetReference):void
---@alias GameObjectLoadCallback fun(go: CS.UnityEngine.GameObject):void

function M:Schedule(interval, repeatTimes, delay, callback, ...)
	if not self.timeouts then
		self.timeouts = {}
	end

	local timeoutClose = self.timeouts[callback]
	if timeoutClose then
		timeoutClose()
	end
	local TickService = SM.GetTickService()
	local closeFunc = TickService.Timeout({ start = delay, interval = interval }, repeatTimes, callback, ...)
	self.timeouts[callback] = closeFunc
end

function M:ScheduleOnce(delay, callback, ...)
	if not self.timeouts then
		self.timeouts = {}
	end
	if self.timeouts[callback] then
		error("Callback has already registered.")
		return
	end
	local TickService = SM.GetTickService()
	local closeFunc = TickService.Timeout({ start = delay, interval = delay }, 1, function(...)
		self.timeouts[callback] = nil
		callback(...)
	end, ...)
	self.timeouts[callback] = closeFunc
end

function M:Unschedule(callback)
	if not self.timeouts then
		self.timeouts = {}
	end
	local closeFunc = self.timeouts[callback]
	if closeFunc then
		closeFunc()
		self.timeouts[callback] = nil
	else
		error("Callback not registered.")
	end
end

function M:StartTick()
	assert(self.Tick, "no function named: Tick")
	assert(not self.ticking)
	local TickService = SM.GetTickService()
	TickService.Register(self.Tick, self)
	self.ticking = true
end

function M:StopTick()
	if not self.ticking then
		error("ticking == false")
		return
	end
	local TickService = SM.GetTickService()
	TickService.Unregister(self.Tick)
	self.ticking = false
end

function M:_LoadAssetAsync(path, funcName, callback)
	local cancelContext = {cancel = false}
	local service = AssetService.Get()
	local loadHandle = service[funcName](service, path)
	loadHandle:OnComplete("+", function(handle)
		local ref = handle.Result
		if cancelContext.cancel then
			ref:Dispose()
			return
		end
		if callback(ref, cancelContext) then
			ref:Dispose()
		end
		local index = table.index_of(self.loadedRefHandles, loadHandle)
		table.swap_remove(self.loadedRefHandles, index)
	end)
	if not self.loadedRefHandles then
		self.loadedRefHandles = {}
	end
	table.insert(self.loadedRefHandles, loadHandle)
	return cancelContext
end

---@param path string
---@param callback AssetReferenceLoadCallback
function M:LoadAnimationClipAsync(path, callback)
	return self:_LoadAssetAsync(path, "LoadAnimationClipAsync", callback)
end

---@param path string
---@param callback AssetReferenceLoadCallback
function M:LoadScriptableObjectAsync(path, callback)
	return self:_LoadAssetAsync(path, "LoadScriptableObjectAsync", callback)
end

---@param path string
---@param callback AssetReferenceLoadCallback
function M:LoadAnimatorControllerAsync(path, callback)
	return self:_LoadAssetAsync(path, "LoadAnimatorControllerAsync", callback)
end

---@param path string
---@param callback AssetReferenceLoadCallback
function M:LoadTextAsync(path, callback)
	return self:_LoadAssetAsync(path, "LoadTextAsync", callback)
end

---@param path string
---@param callback AssetReferenceLoadCallback
function M:LoadPrefabCallback(path, callback)
	return self:_LoadAssetAsync(path, "LoadGameObjectAsync", callback)
end

---@param path string
---@param callback AssetReferenceLoadCallback
function M:LoadMaterialAsync(path, callback)
	return self:_LoadAssetAsync(path, "LoadMaterialAsync", callback)
end

---@param path string
---@param callback AssetReferenceLoadCallback
function M:LoadAudioClipAsync(path, callback)
	return self:_LoadAssetAsync(path, "LoadAudioClipAsync", callback)
end

local traceback = debug.traceback
---@param path string
---@param callback GameObjectLoadCallback
---@param disposeRef boolean
function M:LoadGameObjectAsync(path, callback, disposeRef)
	disposeRef = disposeRef == nil and true or disposeRef
	return self:_LoadAssetAsync(path, "LoadGameObjectAsync", function(ref, cancelContext)
		local loadHandle = ref:InstantiateAsync()
		if not self.loadedRefHandles then
			self.loadedRefHandles = {}
		end
		table.insert(self.loadedRefHandles, loadHandle)
		loadHandle:Callback("+", function(go)
			local index = table.index_of(self.loadedRefHandles, loadHandle)
			table.swap_remove(self.loadedRefHandles, index)

			if cancelContext.cancel then
				AssetService.Recycle(go)
				ref:Dispose()
				return
			end
			local ok, err = xpcall(callback, traceback, go, ref)
			if not ok then
				error(err, path)
			end
			if disposeRef then
				ref:Dispose()
			end
		end)
		return false
	end)
end

---@param assetRef CS.Extend.Asset.AssetReference
---@param callback GameObjectLoadCallback
function M:InstantiateFromRefAsync(assetRef, callback)
	assert(assetRef)
	local loadHandle = assetRef:InstantiateAsync()
	if not self.loadedRefHandles then
		self.loadedRefHandles = {}
	end
	table.insert(self.loadedRefHandles, loadHandle)
	local cancelContext = {cancel = false}
	loadHandle:Callback("+", function(go)
		local index = table.index_of(self.loadedRefHandles, loadHandle)
		table.swap_remove(self.loadedRefHandles, index)
		if cancelContext.cancel then
			AssetService.Recycle(go)
			return
		end
		callback(go)
	end)
	return cancelContext
end

function M:recycle()
	if self.ticking then
		self:StopTick()
	end

	if self.timeouts then
		for _, close in pairs(self.timeouts) do
			close()
		end
		self.timeouts = nil
	end

	if self.loadedRefHandles then
		for _, loadHandle in ipairs(self.loadedRefHandles) do
			loadHandle.Cancel = true
		end
		self.loadedRefHandles = nil
	end
end
LuaBindingClass = M
return M