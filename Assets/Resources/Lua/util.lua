local setmetatable, getmetatable, rawset, rawget, pairs, type, ipairs = setmetatable, getmetatable, rawset, rawget, pairs, type, ipairs
local insert = table.insert
local find, format, sub, gsub = string.find, string.format, string.sub, string.gsub
local tonumber, tostring = tonumber, tostring


local next = next
string.replace = function(s, pattern, repl)
    local i,j = string.find(s, pattern, 1, true)
    if i and j then
        local ret = {}
        local start = 1
        while i and j do
            table.insert(ret, string.sub(s, start, i - 1))
            table.insert(ret, repl)
            start = j + 1
            i,j = string.find(s, pattern, start, true)
        end
        table.insert(ret, string.sub(s, start))
        return table.concat(ret)
    end
    return s
end

string.replace_with_table = function(s, t)
    return gsub(s, "%$(%w+)", t)
end

string.split = function(s, delimiter)
    local result = {}
    local from = 1
    local f, t = find(s, delimiter, from, true)
    while f do
        insert(result, sub(s, from, f - 1))
        from = t + 1
        f, t = find(s, delimiter, from, true)
    end
    insert(result, sub(s, from))
    return result
end

function table.empty(t)
    return next(t) == nil
end

local function print_r(t)
    local log = ""
    local print_r_cache={}
    local function sub_print_r(t,indent)
        if (print_r_cache[tostring(t)]) then
            log = log .. indent.."*"..tostring(t)
        else
            print_r_cache[tostring(t)]=true
            if type(t)=="table" then
                for pos, val in pairs(t) do
                    if type(val)=="table" then
                        log = log .. indent.."["..pos.."] => ".. tostring(t) .." {\n"
                        sub_print_r(val,indent..string.rep(" ",string.len(pos)+8))
                        log = log .. indent..string.rep(" ",string.len(pos)+6).."}\n"
                    elseif (type(val)=="string") then
                        log = log .. indent.."["..pos..'] => "'..val..'"\n'
                    else
                        log = log .. indent.."["..pos.."] => ".. tostring(val) .. '\n'
                    end
                end
            else
                log = log .. indent .. tostring(t) .. "\n"
            end
        end
    end
    if type(t)=="table" then
        log = log .. tostring(t).." {\n"
        sub_print_r(t,"  ")
        log = log .. "} \n"
    else
        sub_print_r(t,"  ")
    end
    print(log)
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