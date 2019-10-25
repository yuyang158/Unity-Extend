local M = class()
M.binding = {
	btn = CS.UnityEngine.UI.Button,
	text = CS.UnityEngine.UI.Text
}

function M:ctor(go)
	self.go = go
	self.counter = 0
end

function M:awake()
	self.text.text = tostring(self.counter)
	self.btn.onClick:AddListener(function ()
		self.counter = self.counter + 1
		self.text.text = tostring(self.counter)
	end)
end

return M