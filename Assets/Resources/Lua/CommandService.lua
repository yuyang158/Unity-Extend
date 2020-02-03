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

function M.test_1()
    print("test")
end

function M.test_2(a, b)
    print("test 2", a, b)
end
return M