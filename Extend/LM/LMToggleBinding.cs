using UnityEngine.UI;

namespace XLua.Extend.LM {
    public class LMToggleBinding : LMBooleanBinding {
        public Toggle toggle;

        private void Awake() {
            if( !toggle ) {
                toggle = GetComponent<Toggle>();
            }
        }

        protected override void ChangeBoolean( bool b ) {
            toggle.isOn = b;
        }
    }
}