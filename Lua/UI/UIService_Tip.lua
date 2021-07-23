local M = require("UI.UIService")
local binding = require("mvvm.binding")

---@type CS.Extend.Asset.AssetService
local AssetService = CS.Extend.Asset.AssetService
local GameObjectType = typeof(CS.UnityEngine.GameObject)
local LuaMVVMBindingType = typeof(CS.Extend.LuaMVVM.LuaMVVMBinding)
local EventSystem = CS.UnityEngine.EventSystems.EventSystem
---@type CS.UnityEngine.RectTransformUtility
local RectTransformUtility = CS.UnityEngine.RectTransformUtility
local Camera = CS.UnityEngine.Camera
local cs_coroutine = require("cs_coroutine")
local yield = coroutine.yield
local WaitForEndOfFrame = CS.UnityEngine.WaitForEndOfFrame

M.TipType = {
	ItemDescription = "UI/Module/Module_Common",
	SkillDescription = "UI/Module/Module_Skill_Explain"
}
local tapAnyHandler
local currentTip

function M.ShowTip(tipType, dataContext, options)
	local tipLayer = M.GetLayerRoot("Tip")

	local loadHandle = AssetService.Get():LoadAsync(tipType, GameObjectType)
	loadHandle:OnComplete("+", function()
		local screenPos
		if options.screenPosition then
			screenPos = options.screenPosition
		else
			screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, options.worldPosition)
		end
		local ref = loadHandle.Result
		local go = ref:Instantiate(tipLayer, false)
		ref:Dispose()
		local mvvm = go:GetComponent(LuaMVVMBindingType)
		mvvm:SetDataContext(dataContext)
		local root = go.transform
		currentTip = root

		local position = CS.LuaBattleTransformUtility.GetScreenPositionToRectTransformLocalPosition(screenPos, tipLayer)
		root.anchoredPosition = position

		cs_coroutine.start(function(parent)
			yield(WaitForEndOfFrame())

			---@type CS.UnityEngine.RectTransform
			local sizeTransform = parent:GetChild(0)
			local childScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, sizeTransform.position)
			local size = sizeTransform.sizeDelta
			local pivot = sizeTransform.pivot
			local Screen = CS.UnityEngine.Screen
			local offsetStart = size.x * pivot.x
			local offsetEnd = size.x * (1 - pivot.x)
			if childScreenPosition.x + offsetEnd > Screen.width then
				childScreenPosition.x = Screen.width - offsetEnd
			elseif childScreenPosition.x < offsetStart then
				childScreenPosition.x = offsetStart
			end
			offsetStart = size.y * pivot.y
			offsetEnd = size.y * (1 - pivot.y)
			if childScreenPosition.y + offsetEnd > Screen.height then
				childScreenPosition.y = Screen.height - offsetEnd
			elseif childScreenPosition.y < offsetStart then
				childScreenPosition.y = offsetStart
			end
			local childPosition = CS.LuaBattleTransformUtility.GetScreenPositionToRectTransformLocalPosition(childScreenPosition, parent)
			sizeTransform.anchoredPosition = childPosition
		end, root)
		
		tapAnyHandler = CS.ScreenTouchUtil.RequestAnyTouchCallback(function()
			if M._FindSelectableInParent(go) then
				return
			end
			M.HideTip()
		end, false)
	end)
end

function M._FindSelectableInParent(go)
	local selected = EventSystem.current.currentSelectedGameObject
	if not selected then
		return false
	end

	if selected == go then
		return true
	end
	
	local parent = selected.transform
	local t = go.transform

	while parent do
		if parent == t then
			return true
		end
		parent = parent.parent
	end
	return false
end

function M.HideTip()
	if tapAnyHandler then
		tapAnyHandler:Dispose()
		tapAnyHandler = nil
	end

	if not currentTip then
		return
	end

	AssetService.Recycle(currentTip)
	currentTip = nil
end

return M