local M = {}
local comments = {
	test = "for test"
}

function M.Init()
end

function M.help()
	for k, v in pairs(comments) do
		print(k .. ":", v)
	end
end

function M.test_1(p1)
	local arr = {}
	for i = 0, p1.Length - 1 do
		table.insert(arr, p1[i])
	end
	error("test", table.unpack(arr))
end

function M.test_2(a, b)
	print("test 2", a, b)
end

function M.AssetDump()
	CS.Extend.Asset.AssetService.Get():Dump()
end

return M