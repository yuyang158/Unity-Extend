mkdir -p build_ios_54 && cd build_ios_54
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_VERSION=5.4.3 -DCMAKE_TOOLCHAIN_FILE=../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../
cd ..
cmake --build build_ios_54 --config Release
mkdir -p plugin_lua54/Plugins/iOS/
cp build_ios_54/Release-iphoneos/libmoon.a plugin_lua54/Plugins/iOS/libmoon.a 

