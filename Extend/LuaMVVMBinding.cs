using UnityEngine;

namespace XLua.Extend {
    public abstract class LuaMVVMBinding : MonoBehaviour {
        public string path;

        protected virtual void OnEnable() {
            LuaMVVM.Instance.RegisterBinding( this );
        }

        protected virtual void OnDisable() {
            LuaMVVM.Instance.UnreigsterBinding( this );
        }

        public abstract void Change( object value );
    }

    public abstract class LMBooleanBinding : LuaMVVMBinding {
        public sealed override void Change( object value ) {
            bool b = (bool)value;
            ChangeBoolean( b );
        }

        public abstract void ChangeBoolean( bool v );
    }

    public abstract class LMFloatBinding : LuaMVVMBinding {
        public override void Change( object value ) {
            float f = (float)value;
            ChangeFloat( f );
        }

        public abstract void ChangeFloat( float v );
    }

    public abstract class LMIntegerBinding : LuaMVVMBinding {
        public override void Change( object value ) {
            long l = (long)value;
            ChangeInteger( l );
        }
        public abstract void ChangeInteger( long v );
    }
}