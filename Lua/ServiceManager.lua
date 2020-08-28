local M = {}
local services = {}

M.SERVICE_TYPE = {
    CONFIG = 1,
    TICK = 2,
    CONSOLE_COMMAND = 3,
    UI = 4,
    GLOBAL_VM = 5,
    MOCK = 6
}

function M.GetService(typ)
    return services[typ]
end

function M.UnregisterService(typ)
    services[typ] = nil
end

function M.RegisterService(typ, service)
    assert(service)
    service.Init()
    services[typ] = service
end

function M.Shutdown()
    for _, typ in ipairs(M.SERVICE_TYPE) do
        local service = services[typ]
        if service.clear then
            service.clear()
        end
    end
end

_ServiceManager = M

return M