mkdir build64_54 & pushd build64_54
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_COMPAT_MATHLIB=ON -DEMMY_LUA_VERSION=54 -DEMMY_USE_LUA_SOURCE=ON -DLUA_VERSION=5.4.3 -G "Visual Studio 17 2022" -A x64 ..
popd
cmake --build build64_54 --config Release
md plugin_lua54\Plugins\x86_64
copy /Y build64_54\Release\xlua.dll plugin_lua54\Plugins\x86_64\xlua.dll

mkdir build32_54 & pushd build32_54
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_COMPAT_MATHLIB=ON -DEMMY_LUA_VERSION=54 -DEMMY_USE_LUA_SOURCE=ON -DLUA_VERSION=5.4.3 -G "Visual Studio 17 2022" -A Win32 ..
popd
cmake --build build32_54 --config Release
md plugin_lua54\Plugins\x86
copy /Y build32_54\Release\xlua.dll plugin_lua54\Plugins\x86\xlua.dll

pause