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

function M:record(path)
    if not self.deps[path] then
        self.deps[path] = true
        tinsert(self.collect, path)
    end
end

function M:fetch(t)
    if #self.collect == 0 then
        return
    end
    
    
    self.collect = {}
end


return M