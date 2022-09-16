using System;
using System.Collections.Generic;
using System.Linq;
using Extend.Common;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extend.UI.Editor.RelationGraph {
	public class UIRelationGraphView : GraphView {
		public class UIViewMenuWindowProvider : ScriptableObject, ISearchWindowProvider {
			public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
				var entries = new List<SearchTreeEntry> {
					new SearchTreeGroupEntry(new GUIContent("Create Node"))
				};

				var names = Enum.GetNames(typeof(UILayer));
				var viewConfiguration = UIViewConfiguration.GlobalInstance;
				for( int i = 0; i < names.Length - 2; i++ ) {
					entries.Add(new SearchTreeGroupEntry(new GUIContent(names[i])) {level = 1});
					var layer = (UILayer)Enum.Parse(typeof(UILayer), names[i]);
					entries.AddRange(from configuration in viewConfiguration.Configurations 
						where configuration.AttachLayer == layer 
						select new SearchTreeEntry(new GUIContent(configuration.Name)) {level = 2, userData = configuration});
				}

				return entries;
			}

			public event Func<UIViewConfiguration.Configuration, bool> CreateNodeEvent;

			public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context) {
				var success = CreateNodeEvent?.Invoke(entry.userData as UIViewConfiguration.Configuration);
				return success != null && success.Value;
			}
		}


		public UIRelationGraphView() {
			styleSheets.Add(Resources.Load<StyleSheet>("NarrativeGraph"));
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new FreehandSelector());

			var grid = new GridBackground();
			Insert(0, grid);
			grid.StretchToParentSize();

			var provider = ScriptableObject.CreateInstance<UIViewMenuWindowProvider>();
			provider.CreateNodeEvent += configuration => {
				foreach( var existNode in m_nodes ) {
					if( existNode.Configuration == configuration )
						return false;
				}

				var node = new UINextLinkNode(configuration);
				AddNewNode(node);
				return true;
			};
			nodeCreationRequest += context => {
				if( m_sourceNode == null )
					return;
				SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
			};

			graphViewChanged += change => {
				if( change.elementsToRemove == null )
					return change;
				foreach( var element in change.elementsToRemove ) {
					if( element is UINextLinkNode linkNode ) {
						m_nodes.RemoveSwap(linkNode);
						RefreshPosition();
					}
					else if( element is Edge edge ) {
						m_edges.RemoveSwap(edge);
					}
				}

				return change;
			};
		}

		private readonly List<UIViewNode> m_nodes = new List<UIViewNode>();
		private readonly List<Edge> m_edges = new List<Edge>();
		private UISourceNode m_sourceNode;

		public void ChangeSourceNode(UIViewConfiguration.Configuration configuration) {
			foreach( var existNode in m_nodes ) {
				existNode.Clear();
				RemoveElement(existNode);
			}

			foreach( var edge in m_edges ) {
				RemoveElement(edge);
			}

			m_edges.Clear();
			m_nodes.Clear();
			if( configuration == null )
				return;

			var node = new UISourceNode(configuration);
			m_sourceNode = node;
			AddNewNode(node);

			if( configuration.Relations == null || configuration.Relations.Length == 0 )
				return;

			foreach( var relation in configuration.Relations ) {
				var relateConfiguration = UIViewConfiguration.GlobalInstance.FindWithGuid(relation.RelationViewGuid);
				var relationNode = new UINextLinkNode(relateConfiguration);
				AddNewNode(relationNode);

				var edge = new Edge {input = relationNode.InputPort, output = m_sourceNode.OutputPort};
				edge.input.Connect(edge);
				edge.output.Connect(edge);
				
				m_edges.Add(edge);
				AddElement(edge);
			}
		}

		private void AddNewNode(UIViewNode node) {
			m_nodes.Add(node);
			AddElement(node);
			RefreshPosition();
		}

		private void RefreshPosition() {
			const int baseY = 350;
			const float height = 100;
			var offset = -( m_nodes.Count - 1 ) * 0.5f * height + baseY;

			for( int i = 1; i < m_nodes.Count; i++ ) {
				var existNode = m_nodes[i];
				var position = existNode.GetPosition();
				position.y = offset + ( i - 1 ) * height;
				position.x = 500;
				existNode.SetPosition(position);
			}
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
			var availablePorts = ports.ToList();
			for( int i = 0; i < availablePorts.Count; i++ ) {
				var port = availablePorts[i];
				if( port.GetType() == startPort.GetType() ) {
					availablePorts.RemoveSwapAt(i);
				}
			}

			return availablePorts;
		}


		public static Port CreatePortInstance(Node node, Direction nodeDirection,
			Port.Capacity capacity = Port.Capacity.Single) {
			return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(Port));
		}
	}
}