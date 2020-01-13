local setmetatable, getmetatable, rawset, rawget, pairs, type, ipairs = setmetatable, getmetatable, rawset, rawget, pairs, type, ipairs
local insert = table.insert
local find, format, sub, gsub = string.find, string.format, string.sub, string.gsub
local tonumber, tostring = tonumber, tostring


local next = next
function string.split(self, delimiter)
    local result = { }
    local from = 1
    local f, t = find(self, delimiter, from, true)
    while f do
        insert(result, sub(self, from, f - 1))
        from = t + 1
        f, t = find(self, delimiter, from, true)
    end
    insert(result, sub(self, from))
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

local function parse_path(path)
    if not path or path == '' then error('invalid path:' .. tostring(path)) end
    --print('start to parse ' .. path)
    local result = {}
    local i, n = 1, #path
    while i <= n do
        local s, e, split1, key, split2 = find(path, "([%.%[])([^%.^%[^%]]+)(%]?)", i)
        if not s or s > i then
            --print('"'.. sub(path, i, s and s - 1).. '"')
            insert(result, sub(path, i, s and s - 1))
        end
        if not s then break end
        if split1 == '[' then
            if split2 ~= ']' then error('invalid path:' .. path) end
            key = tonumber(key)
            if not key then error('invalid path:' .. path) end
            --print(key)
            insert(result, key)
        else
            --print('"'.. key .. '"')
            insert(result, key)
        end
        i = e + 1
    end
    --print('finish parse ' .. path)
    return result
end

local function gen_callback_func(tbl, path, callback)
    local keys = parse_path(path)
    local final
    local finalKey
    for index, key in ipairs(keys) do
        if #keys == index then
            finalKey = key
            break
        end
        final = tbl[key]
    end
    final.__notifications = final.__notifications or {}
    local notifications = final.__notifications[finalKey] or {}
    insert(notifications, callback)
    final.__notifications[finalKey] = notifications
end

return {
    parse_path = parse_path,
    gen_callback_func = gen_callback_func
}