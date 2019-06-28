using UnityEngine.UI;

namespace XLua.Extend.LM {
    public class LMToggleBinding : LMBooleanBinding {
        private Toggle toggle;
        void Awake() {
            toggle = GetComponent<Toggle>();
        }

        public override void ChangeBoolean( bool b ) {
            toggle.isOn = b;
        }
    }
}