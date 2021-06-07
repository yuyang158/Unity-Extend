using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Extend.UI.Editor.RelationGraph {
	public class UIViewSourcePort : Port {
		public UIViewSourcePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type)
			: base(portOrientation, portDirection, portCapacity, type) {
			var connectorListener = new DefaultEdgeConnectorListener();
			m_EdgeConnector = new EdgeConnector<Edge>(connectorListener);
			this.AddManipulator(m_EdgeConnector);
		}

		public override void Connect(Edge edge) {
			base.Connect(edge);
			var sourceNode = edge.output.node as UIViewNode;
			var linkNode = edge.input.node as UIViewNode;
			sourceNode.Configuration.Relations ??= new UIViewConfiguration.Configuration.UIViewRelation[0];
			if( Array.Exists(sourceNode.Configuration.Relations, relation => relation.RelationViewGuid == linkNode.Configuration.ViewGuid) )
				return;
			ArrayUtility.Add(ref sourceNode.Configuration.Relations, new UIViewConfiguration.Configuration.UIViewRelation {
				Method = UIViewConfiguration.Configuration.PreloadMethod.AssetBundle,
				RelationViewGuid = linkNode.Configuration.ViewGuid
			});
		}

		public override void Disconnect(Edge edge) {
			base.Disconnect(edge);
			var linkNode = edge.input.node as UINextLinkNode;
			var sourceNode = edge.output.node as UIViewNode;
			ArrayUtility.RemoveAt(ref sourceNode.Configuration.Relations,
				Array.FindIndex(sourceNode.Configuration.Relations,
					relation => relation.RelationViewGuid == linkNode.Configuration.ViewGuid));
		}
	}
}