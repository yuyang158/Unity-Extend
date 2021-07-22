using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Extend.UI.Editor.RelationGraph {
	public class DefaultEdgeConnectorListener : IEdgeConnectorListener {
		private GraphViewChange m_GraphViewChange;
		private List<Edge> m_EdgesToCreate;
		private List<GraphElement> m_EdgesToDelete;

		public DefaultEdgeConnectorListener() {
			this.m_EdgesToCreate = new List<Edge>();
			this.m_EdgesToDelete = new List<GraphElement>();
			this.m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
		}

		public void OnDropOutsidePort(Edge edge, Vector2 position) {
		}

		public void OnDrop(GraphView graphView, Edge edge) {
			this.m_EdgesToCreate.Clear();
			this.m_EdgesToCreate.Add(edge);
			this.m_EdgesToDelete.Clear();
			if( edge.input.capacity == Port.Capacity.Single ) {
				foreach( Edge connection in edge.input.connections ) {
					if( connection != edge )
						this.m_EdgesToDelete.Add((GraphElement)connection);
				}
			}

			if( edge.output.capacity == Port.Capacity.Single ) {
				foreach( Edge connection in edge.output.connections ) {
					if( connection != edge )
						this.m_EdgesToDelete.Add((GraphElement)connection);
				}
			}

			if( this.m_EdgesToDelete.Count > 0 )
				graphView.DeleteElements((IEnumerable<GraphElement>)this.m_EdgesToDelete);
			List<Edge> edgesToCreate = this.m_EdgesToCreate;
			if( graphView.graphViewChanged != null )
				edgesToCreate = graphView.graphViewChanged(this.m_GraphViewChange).edgesToCreate;
			foreach( Edge edge1 in edgesToCreate ) {
				graphView.AddElement((GraphElement)edge1);
				edge.input.Connect(edge1);
				edge.output.Connect(edge1);
			}
		}
	}
}