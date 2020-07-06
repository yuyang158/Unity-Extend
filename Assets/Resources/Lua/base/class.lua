---@class integer
function class(super)
	local class_type = {}
	class_type.ctor = false
	class_type.super = super
	class_type.new = function(...)
		local obj = {}
		setmetatable(obj, {
			__index = class_type,
			__gc = function()
				print("RELEASE", obj)
			end
		})
		do
			local create
			create = function(c, ...)
				if c.super then
					create(c.super, ...)
				end
				if c.ctor then
					c.ctor(obj, ...)
				end
			end

			create(class_type, ...)
		end
		return obj
	end

	if super then
		setmetatable(class_type, { __index = function(_, k)
			local ret = super[k]
			class_type[k] = ret
			return ret
		end })
	end

	return class_type
end