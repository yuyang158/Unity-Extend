local M = {}
local tinsert = table.insert
local setmetatable, getmetatable = setmetatable, getmetatable
local util = require('util')

function M.new()
    return setmetatable({
        deps = {},
        collect = {}
    }, M)
end

function M:Record(path)
    if not self.deps[path] then
        self.deps[path] = true
        tinsert(self.collect, path)
    end
end

function M:Fetch(t, computedKey)
    local tbl = getmetatable(t).__raw
    if #self.collect == 0 then
        return
    end
    for _, key in ipairs(self.collect) do
        util.gen_callback_func(tbl, key, function()
            local notifications = tbl.__notifications[computedKey]
            if notifications and #notifications > 0 then
                for _, notification in ipairs(notifications) do
                    notification(t)
                end
            end
        end)
    end
    
    self.collect = {}
end


return M