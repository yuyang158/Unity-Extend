# 使用说明

> waterfall
```LUA
local waterfall = require("async").waterfall
```

waterfall(tasks, [callback])

依次执行tasks函数数组，每个函数将其结果传递给数组中的下一个。 但是，如果任何任务将错误传递给它们自己的回调，则不会执行下一个函数，并且callback将立即调用。

Example
```LUA
waterfall({
    function(cb)
        local value = 1
        print(value)
        cb(nil, value + 1)
    end,
    function(value, cb)
        print(value)
        cb(nil, value + 1)
    end
}, function(err, value)
    print(value)
end)
--- 1 2 3
```

> parallel
```LUA
local parallel = require("async").parallel
```

parallel(tasks, [callback])

并行运行tasks集合，而无需等待上一个功能完成。 如果任何函数将错误传递给其回调，则将立即使用错误的值调用callback。 任务完成后，结果将作为数组传递到callback。

```LUA
parallel({
    function(cb)
        cb(nil, 1)
    end,
    function(cb)
        cb(nil, 2)
    end
}, function(err, values)
    print(values)
end)
--- 1 2
```

> eachSeries
```LUA
local eachSeries = require("async").eachSeries
```

eachSeries(coll, iteratee, [callback])

将函数iteratee串行应用于coll中的每个项目。 调用iteratee时将使用列表中的一个项目，并在完成时进行回调。 如果iteratee将错误传递给其回调，则立即调用callback。

```LUA
local coll = {file1 = "aaaaa", file2 = "bbbbb"}
eachSeries(coll, function(value, _, callback)
    io.open(value)
    callback()
end, function(err)

end)
```

> times
```LUA
local times = require("async").times
```

```LUA
times(5, function(n, callback)
    local handle = AssetService.Get():LoadAsync("XXXXX/YYYYY")
    handle:OnComplete("+", function()
        callback(nil, handle.Asset)
    end)
end, function(err, results)
    print(results)
end)
```