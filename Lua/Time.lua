---@class Time
local M, Meta = class()
local chronos = require("chronos")
local os, floor, ceil, format, max = os, math.floor, math.ceil, string.format, math.max
local start = chronos.nanotime()
local utc = os.time()

function os.timezone()
    local now = os.time()
    return os.difftime(now, os.time(os.date("!*t", now)))
end

function M:ctor(time)
	self.time = time or 0
end

function M.SyncServerTime(serverTime)
	utc = serverTime
	start = chronos.nanotime()
end

function M.FromUTCMillisecond(milliseconds)
	return M.new(milliseconds * 0.001)
end

function M.FromUTCSeconds(seconds)
	return M.new(seconds)
end

function M.Now()
	return M.FromUTCSeconds(utc + chronos.nanotime() - start)
end

function M:GetTotalSeconds()
	return self.time
end

function M:GetTotalMinutes()
	return self.time / 60
end

function M:GetTotalHours()
	return self.time / 60 / 60
end

function M:GetTimeTable(timezone)
	local time = floor(self.time)
	if not timezone then
		return os.date("*t", time)
	end
	local timeInterval = os.time(os.date("!*t", time)) + timezone * 3600 + (os.date("*t", time).isdst and -1 or 0) * 3600
	return os.date("*t", timeInterval)
end

function M:GetTimeHourTable()
	local time = max(self.time, 0)
	local hour = floor(time / 60 / 60)
	local min = floor(time / 60 % 60)
	local second = ceil(time % 60)
	return {
		hour = hour,
		min = min,
		second = second
	}
end

-- To HH:MM:SS String
function M:ToHH_MM_SS()
	local t = self:GetTimeHourTable()
	return format("%02d:%02d:%02d", t.hour, t.min, t.second)
end

function Meta.__add(l, r)
	return M.new(l.time + r.time)
end

function Meta.__sub(l, r)
	return M.new(l.time - r.time)
end

function Meta.__eq(l, r)
	return l.time == r.time
end

function Meta.__lt(l, r)
	if type(l) == "number" then
		return l < r.time
	elseif type(r) == "number" then
		return l.time < r
	else
		return l.time < r.time
	end
end

function Meta.__le(l, r)
	return l.time <= r.time
end

function Meta.__mul(l, r)
	if type(r) == "number" then
		return M.FromUTCSeconds(l.time * r)
	else
		return M.FromUTCSeconds(r.time * l)
	end
end

return M