using UnityEngine.UI;

namespace XLua.Extend.LM {
    public class LMTextBinding : LuaMVVMBinding {
        public Text text;
        private void Awake() {
            if( !text ) {
                text = GetComponent<Text>();
            }
        }

        public override void Change( object value ) {
            text.text = value.ToString();
        }
    }
}
