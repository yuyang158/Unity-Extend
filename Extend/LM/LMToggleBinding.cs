using UnityEngine.UI;

namespace XLua.Extend.LM {
    public class LMToggleBinding : LMBooleanBinding {
        public Toggle toggle;
        void Awake() {
            if( !toggle ) {
                toggle = GetComponent<Toggle>();
            }
        }

        public override void ChangeBoolean( bool b ) {
            toggle.isOn = b;
        }
    }
}