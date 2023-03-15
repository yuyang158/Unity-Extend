@echo off
set ROOT_NAME=%~n1
set TARGET_EXT=.bytes
if exist %ROOT_NAME% rd /s /q %ROOT_NAME%
mkdir %ROOT_NAME%
setlocal enabledelayedexpansion

pushd %cd%
cd /d "%1">nul 2>nul || echo cmd was running error
set cur_dir=%cd%
popd

set /a num = 0

echo start copy lua

xcopy %1 %~dp0%ROOT_NAME%\ /s /e /y /q /exclude:%~dp0exclude.txt

echo start build lua
for /r %~dp0%ROOT_NAME%\ %%i in (*.lua) do (
    set rel=%%i & set "rel=!rel:%~dp0=!"
    set rel_tar=%%i%TARGET_EXT% & set "rel=!rel:%~dp0=!"
    luac -o !rel_tar! !rel!
    set /a num += 1
)
echo bulid num:%num%
echo start delete origin lua
for /r %~dp0%ROOT_NAME%\ %%i in (*.lua) do del %%i /f /q
echo lua build all done
exit 0