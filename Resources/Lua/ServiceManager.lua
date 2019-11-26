local M = {}
local services = {}

M.SERVICE_TYPE = {
    CONFIG = 1,
    TICK = 2
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
_ServiceManager = M

return M