namespace XLua.Extend {
    public class LuaBinding_Update : LuaBinding {
        private LuaFunction update;
        protected override void Awake() {
            base.Awake();
            update = bindInstance.GetInPath<LuaFunction>( "update" );
        }

        void Update() {
            update.Call( bindInstance );
        }
    }
}
