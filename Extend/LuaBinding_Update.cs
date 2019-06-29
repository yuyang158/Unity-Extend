namespace XLua.Extend {
    [CSharpCallLua]
    public class LuaBinding_Update : LuaBinding {
        private UnityFunction update;
        protected override void Awake() {
            base.Awake();
            update = bindInstance.GetInPath<UnityFunction>( "update" );
        }

        void Update() {
            update( bindInstance );
        }
    }
}
