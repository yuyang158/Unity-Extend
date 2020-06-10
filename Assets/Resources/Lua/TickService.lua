local pairs, next, table = pairs, next, table
---@class TickService
local M = {}
local tickers = {}

function M.Init()
    setmetatable(tickers, {__mode = "kv"})
end

function M.Register(func, ...)
    local packed = table.pack(...)
    packed.n = nil
    tickers[func] = packed
end

function M.Tick(deltaTime)
    for func, packed in pairs(tickers) do
        if next(packed) then
            func(deltaTime, table.unpack(packed))
        else
            func(deltaTime)
        end
    end
end

function M.Unregister(func)
    tickers[func] = nil
end

return M