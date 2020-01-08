package.cpath = package.cpath .. ';C:/Users/yang.yu/.Rider2019.2/config/plugins/intellij-emmylua/classes/debugger/emmy/windows/x64/?.dll'
local dbg = require('emmy_core')
dbg.tcpConnect('localhost', 9967)
local next = next

function string.split(self, delimiter)
    local result = { }
    local from = 1
    local f, t = string.find(self, delimiter, from, true)
    while f do
        table.insert(result, string.sub(self, from, f - 1))
        from = t + 1
        f, t = string.find(self, delimiter, from, true)
    end
    table.insert(result, string.sub(self, from))
    return result
end

function table.empty(t)
    return next(t) == nil
end

local function print_r ( t )
    local print_r_cache={}
    local function sub_print_r(t,indent)
        if (print_r_cache[tostring(t)]) then
            print(indent.."*"..tostring(t))
        else
            print_r_cache[tostring(t)]=true
            if (type(t)=="table") then
                for pos,val in pairs(t) do
                    if (type(val)=="table") then
                        print(indent.."["..pos.."] => "..tostring(t).." {")
                        sub_print_r(val,indent..string.rep(" ",string.len(pos)+8))
                        print(indent..string.rep(" ",string.len(pos)+6).."}")
                    elseif (type(val)=="string") then
                        print(indent.."["..pos..'] => "'..val..'"')
                    else
                        print(indent.."["..pos.."] => "..tostring(val))
                    end
                end
            else
                print(indent..tostring(t))
            end
        end
    end
    if (type(t)=="table") then
        print(tostring(t).." {")
        sub_print_r(t,"  ")
        print("}")
    else
        sub_print_r(t,"  ")
    end
    print()
end

table.print_r = print_r

local SM = require "ServiceManager"
local CS = require "ConfigService"
local TS = require "TickService"

SM.RegisterService(SM.SERVICE_TYPE.CONFIG, CS)
SM.RegisterService(SM.SERVICE_TYPE.TICK, TS)

TS.Register(function()
end)