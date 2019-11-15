---@class TickService
local M = {}
local table = table
local ticker = {}

function M.Init()
end

function M.Register(func, ...)
    local packed = table.pack(...)
    packed.n = nil
    ticker[func] = packed
end

function M.Tick(deltaTime)
    for func, packed in pairs(ticker) do
        if next(packed) then
            func(table.unpack(packed), deltaTime)
        else
            func(deltaTime)
        end        
    end
end

function M.Unregister(func)
    ticker[func] = nil
end

return M