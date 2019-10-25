local tracedoc = require("tracedoc")
local M = {}
local docs = {}

function M.new_doc(name, init)
    local doc = tracedoc.new(init)
    docs[name] = doc
    tracedoc.commit(doc, {})
    return doc
end

function M.release(name)
    docs[name] = nil
end

function M.get_doc(path)
    local splitPath = path:split(".")
    local node = docs

    for _, subPath in ipairs(splitPath) do
        node = docs[subPath]
    end

    return node
end

function M.fetch_all()
    local changes = {}
    for name, doc in pairs(docs) do
        local change = tracedoc.commit(doc, {}, name .. ".")
        changes[name] = change
    end

    return changes
end

return M