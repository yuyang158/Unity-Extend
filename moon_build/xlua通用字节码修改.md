> 当前Lua版本5.4.3
+ Lua原码修改
    1. xlua_build/lua-5.4.3/src/loslib.c, 注掉L142-153,L411
```c 
//static int os_execute (lua_State *L) {
//  const char *cmd = luaL_optstring(L, 1, NULL);
//  int stat;
//  errno = 0;
//  stat = system(cmd);
//  if (cmd != NULL)
//    return luaL_execresult(L, stat);
//  else {
//    lua_pushboolean(L, stat);  /* true if there is a shell */
//    return 1;
//  }
//}
```
```c
static const luaL_Reg syslib[] = {
    {"clock",     os_clock},
    {"date",      os_date},
    {"difftime",  os_difftime},
    //{"execute",   os_execute},
    {"exit",      os_exit},
    {"getenv",    os_getenv},
    {"remove",    os_remove},
    {"rename",    os_rename},
    {"setlocale", os_setlocale},
    {"time",      os_time},
    {"tmpname",   os_tmpname},
    {NULL, NULL}
};
```
---
+ luac编译Makefile, xlua_build/luac/CMakeLists.txt
1. 修改目标原码路径@L27
```makefile
#set(LUA_SRC_PATH ../lua-5.3.5/src)
set(LUA_SRC_PATH ../lua-5.4.3/src)
```
2. 删除LUA_LIB引用文件lbitlib.c，lua5.4.x中不存在@L43
```makefile
#set ( LUA_LIB ... ${LUA_SRC_PATH}/lbaselib.c ${LUA_SRC_PATH}/lbitlib.c ${LUA_SRC_PATH}/lcorolib.c ... )
set ( LUA_LIB ... ${LUA_SRC_PATH}/lbaselib.c ${LUA_SRC_PATH}/lcorolib.c ... )
```
3. lua-5.4.x中luaconf.h文件名发生变化，需要修改脚本@L32
```makefile
#configure_file ( ${LUA_SRC_PATH}/luaconf.h.in ${CMAKE_CURRENT_BINARY_DIR}/luaconf.h )
configure_file ( ${LUA_SRC_PATH}/luaconf.h ${CMAKE_CURRENT_BINARY_DIR}/luaconf.h )
```
---
+ 编译各个平台plugin文件
1. 修改plugin的Makefile文件，xlua_build/CMakeLists.txt。添加编译选项@L19
```makefile
project(XLua)

# add c++11 feature for rapidjson
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++0x")
```

2. lua-5.4.x中luaconf.h文件名发生变化，需要修改脚本@L83
```makefile
#configure_file ( ${LUA_SRC_PATH}/luaconf.h.in ${CMAKE_CURRENT_BINARY_DIR}/luaconf.h )
configure_file ( ${LUA_SRC_PATH}/luaconf.h ${CMAKE_CURRENT_BINARY_DIR}/luaconf.h )
```

3. 在对应平台生成脚本中添加-DLUAC_COMPATIBLE_FORMAT=ON，使其支持通用字节码

    a) xlua_build/make_android_lua54.sh@L13

```bash
function build() {
    ...
    #cmake -H. -B${BUILD_PATH} -DLUA_VERSION=5.4.3 -DANDROID_ABI=${ABI} -DCMAKE_TOOLCHAIN_FILE=${NDK}/build/cmake/android.toolchain.cmake -DANDROID_NATIVE_API_LEVEL=${API} -DANDROID_TOOLCHAIN=clang -DANDROID_TOOLCHAIN_NAME=${TOOLCHAIN_ANME}
    cmake -DLUAC_COMPATIBLE_FORMAT=ON -H. -B${BUILD_PATH} -DLUA_VERSION=5.4.3 -DANDROID_ABI=${ABI} -DCMAKE_TOOLCHAIN_FILE=${NDK}/build/cmake/android.toolchain.cmake -DANDROID_NATIVE_API_LEVEL=${API} -DANDROID_TOOLCHAIN=clang -DANDROID_TOOLCHAIN_NAME=${TOOLCHAIN_ANME}
    ...
}
```
&emsp;&emsp;&emsp;b) xlua_build/make_ios_lua54.sh@L2 
```bash
#cmake -DLUA_VERSION=5.4.3 -DCMAKE_TOOLCHAIN_FILE=../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_VERSION=5.4.3 -DCMAKE_TOOLCHAIN_FILE=../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../
```
&emsp;&emsp;&emsp;c) xlua_build/make_win_lua54.bat@L2，L9
```powershell
#cmake -DLUA_VERSION=5.4.3 -G "Visual Studio 16 2019" -A x64 ..
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_VERSION=5.4.3 -G "Visual Studio 16 2019" -A x64 ..
```
```powershell
#cmake -DLUA_VERSION=5.4.3 -G "Visual Studio 16 2019" -A Win32 ..
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_VERSION=5.4.3 -G "Visual Studio 16 2019" -A Win32 ..
```
            