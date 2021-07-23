NDK=/home/goh-client/xlua_build/android-ndk-r21d

if [ ! -d "$NDK" ]; then
    echo "Please set ANDROID_NDK environment to the root of NDK."
    exit 1
fi

function build() {
    API=$1
    ABI=$2
    TOOLCHAIN_ANME=$3
    BUILD_PATH=build54.Android.${ABI}
    cmake -DLUAC_COMPATIBLE_FORMAT=ON -H. -B${BUILD_PATH} -DLUA_VERSION=5.4.3 -DANDROID_ABI=${ABI} -DCMAKE_TOOLCHAIN_FILE=${NDK}/build/cmake/android.toolchain.cmake -DANDROID_NATIVE_API_LEVEL=${API} -DANDROID_TOOLCHAIN=clang -DANDROID_TOOLCHAIN_NAME=${TOOLCHAIN_ANME}
    cmake --build ${BUILD_PATH} --config Release
    mkdir -p plugin_lua54/Plugins/Android/libs/${ABI}/
    cp ${BUILD_PATH}/libxlua.so plugin_lua54/Plugins/Android/libs/${ABI}/libxlua.so
}

build android-21 armeabi-v7a arm-linux-androideabi-4.9
build android-21 arm64-v8a  arm-linux-androideabi-clang
build android-21 x86 x86-4.9