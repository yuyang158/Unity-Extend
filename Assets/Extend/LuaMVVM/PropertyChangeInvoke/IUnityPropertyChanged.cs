using Extend.LuaUtil;

namespace Extend.LuaMVVM.PropertyChangeInvoke {
	public interface IUnityPropertyChanged {
		event PropertyChangedAction OnPropertyChanged;

		object ProvideCurrentValue();
	}
}