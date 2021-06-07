using UnityEditor.Experimental.GraphView;

namespace Extend.UI.Editor.RelationGraph {
	public class UINextLinkNode : UIViewNode {
		private void Initialize() {
			var port = UIRelationGraphView.CreatePortInstance(this, Direction.Input);
			port.portName = "source";
			inputContainer.Add(port);

			capabilities = Capabilities.Selectable | Capabilities.Collapsible | Capabilities.Deletable;
			var position = GetPosition();
			SetPosition(position);
			RefreshExpandedState();
			RefreshPorts();

			InputPort = port;
		}
		
		public Port InputPort { get; private set; }

		public UINextLinkNode(UIViewConfiguration.Configuration configuration) : base(configuration) {
			Initialize();
		}
	}
}