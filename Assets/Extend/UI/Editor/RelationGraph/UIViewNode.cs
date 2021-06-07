using UnityEditor.Experimental.GraphView;

namespace Extend.UI.Editor.RelationGraph {
	public abstract class UIViewNode : Node {
		public UIViewConfiguration.Configuration Configuration { get; private set; }
		
		protected UIViewNode(UIViewConfiguration.Configuration configuration) {
			Configuration = configuration;
			title = configuration.Name;
		}
	}
}