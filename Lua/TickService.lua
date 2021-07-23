local pairs, next, table, xpcall, setmetatable = pairs, next, table, xpcall, setmetatable
local util = require("util")
local uv = require "luv"

---@class TickService
local M = {}
local tickers = {}
local tickerToAdd = {}

function M.Init()
    setmetatable(tickers, {__mode = "k"})
end

function M.Register(func, ...)
    local packed = table.pack(...)
    tickerToAdd[func] = packed
end

function M.Tick(deltaTime)
    uv.run("nowait")

    for func, pack in pairs(tickerToAdd) do
        tickers[func] = pack
    end
    tickerToAdd = {}

    for func, packed in pairs(tickers) do
        if packed.n ~= 0 then
            util.xpcall_catch(func, table.unpack(packed), deltaTime)
        else
            util.xpcall_catch(func, deltaTime)
        end
    end
end

---@param seconds number 超时时间
---@param repeatTimes integer 重复次数， -1无限重复
---@return function 调用后移除
function M.Timeout(seconds, repeatTimes, callback, ...)
    local timer = uv.new_timer()
    local start, interval
    if type(seconds) == "table" then
        start = math.floor(seconds.start * 1000)
        interval = math.floor(seconds.interval * 1000)
    else
        start = math.floor(seconds * 1000)
        interval = math.floor(seconds * 1000)
    end
    local args = table.pack(...)
    timer:start(start, interval, function()
        local ok, complete = util.xpcall_catch(callback, table.unpack(args))
        if not ok or complete then
            timer:close()
            return
        end

        if repeatTimes > 0 then
            repeatTimes = repeatTimes - 1
            if repeatTimes == 0 then
                timer:close()
            end
        end
    end)

    return function()
        timer:close()
    end
end

function M.Unregister(func)
    tickerToAdd[func] = nil
    tickers[func] = nil
end

return M