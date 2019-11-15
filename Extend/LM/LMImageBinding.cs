using UnityEngine;
using UnityEngine.UI;

namespace Extend.LM {
    public class LMImageBinding : LuaMVVMBinding {
        public Image image;
        public override void Change( object value ) {
            image.sprite = value as Sprite;
        }

        private void Awake() {
            if( !image ) {
                image = GetComponent<Image>();
            }
        }
    }
}
