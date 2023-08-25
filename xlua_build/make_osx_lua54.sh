mkdir -p build_osx && cd build_osx
cmake -DLUAC_COMPATIBLE_FORMAT=ON -DLUA_VERSION=5.4.3 -GXcode ../
cd ..
cmake --build build_osx --config Release
mkdir -p plugin_lua54/Plugins/xlua.bundle/Contents/MacOS/
cp build_osx/Release/xlua.bundle/Contents/MacOS/xlua plugin_lua54/Plugins/xlua.bundle/Contents/MacOS/xlua

