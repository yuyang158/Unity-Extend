mkdir -p build_ios_54 && cd build_ios_54
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_VERSION=5.4.3 -DLUA_USE_IOS=ON -DCMAKE_TOOLCHAIN_FILE=../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../
cd ..
cmake --build build_ios_54 --config Release
mkdir -p plugin_lua54/Plugins/iOS/
cp build_ios_54/Release-iphoneos/libxlua.a plugin_lua54/Plugins/iOS/libxlua.a 
cp build_ios_54/luv/deps/libuv/Release-iphoneos/libuv_a.a plugin_lua54/Plugins/iOS/libuv_a.a 

