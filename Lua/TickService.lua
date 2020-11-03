local pairs, next, table = pairs, next, table
---@class TickService
local M = {}
local tickers = {}
local timeouts = {}

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
            func(table.unpack(packed), deltaTime)
        else
            func(deltaTime)
        end
    end

    local index = 1
    while index <= #timeouts do
        local v = timeouts[index]
        v.time = v.time - deltaTime
        if v.time < 0 then
            v.callback()
            table.swap_remove(timeouts, index)
        else
            index = index + 1
        end
    end

    for _, v in ipairs(timeouts) do
        
    end
end

function M.Timeout(time, callback)
    table.insert(timeouts, {
        time = time,
        callback = callback
    })
end

function M.Unregister(func)
    tickers[func] = nil
end

return M