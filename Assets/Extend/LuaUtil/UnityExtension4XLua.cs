﻿using UnityEngine;

namespace Extend.LuaUtil {
    public static class UnityExtension4XLua {
        public static Sprite LoadSprite( string path ) {
            return Resources.Load<Sprite>( path );
        }
    }
}