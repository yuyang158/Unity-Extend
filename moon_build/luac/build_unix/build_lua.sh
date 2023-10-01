cd $(dirname "$0") || exit 1
SHELL_FOLDER=$(pwd)
ROOT_PATH=$1
ROOT_NAME=Lua

if [ ! -d $ROOT_PATH ]
then
  echo "build_lua.sh: Input should be a directory @"$ROOT_PATH
  exit 1
fi
  
ROOT_NAME=$(basename "$ROOT_PATH")
if [ -d $ROOT_NAME ]
then
  rm -rf ./$ROOT_NAME
fi

cp -rf $ROOT_PATH .
echo "start copy lua"
find $ROOT_NAME -name ".svn" | xargs rm -rf

function travers_files()
{
  #1st param, the dir name
  for file in `ls $1`;
  do
    if [ -d "$1/$file" ]; then
      travers_files "$1/$file"
    else
      
      if [ ${file##*.} == lua ]
      then
        #echo "$1/$file.bytes"
        ./luac.a -o "$1/$file.bytes" "$1/$file"
      fi
    fi
  done
}
echo "start build lua" 
travers_files $ROOT_NAME
echo "start delete origin lua"
find $ROOT_NAME -name "*.lua" | xargs rm -f
echo "lua build all done"
exit 0