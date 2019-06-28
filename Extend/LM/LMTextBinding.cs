using UnityEngine.UI;

namespace XLua.Extend.LM {
    public class LMTextBinding : LuaMVVMBinding {
        private Text text;
        private void Awake() {
            text = GetComponent<Text>();
        }

        public override void Change( object value ) {
            text.text = value.ToString();
        }
    }
}
