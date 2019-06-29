using UnityEngine;
using UnityEngine.UI;

namespace XLua.Extend.LM {
    public class LMImageBinding : LuaMVVMBinding {
        public override void Change( object value ) {
            image.sprite = value as Sprite;
        }

        private Image image;
        void Awake() {
            image = GetComponent<Image>();
        }
    }
}
