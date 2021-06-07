using UnityEditor.Experimental.GraphView;

namespace Extend.UI.Editor.RelationGraph {
	public class UISourceNode : UIViewNode {
		public Port OutputPort { get; private set; }
		
		public UISourceNode(UIViewConfiguration.Configuration configuration) 
			: base(configuration) {
			Initialize();
		}

		private void Initialize() {
			var port = new UIViewSourcePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(UIViewSourcePort)) {
				portName = "next"
			};
			outputContainer.Add(port);
			capabilities = Capabilities.Selectable | Capabilities.Collapsible;
			var position = GetPosition();
			position.x = 300;
			position.y = 300;
			SetPosition(position);
			RefreshExpandedState();
			RefreshPorts();

			OutputPort = port;
		}
	}
}