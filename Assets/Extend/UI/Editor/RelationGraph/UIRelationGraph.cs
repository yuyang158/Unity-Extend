using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extend.UI.Editor.RelationGraph {
	public class UIRelationGraph : EditorWindow {
		public static UIRelationGraph ShowWindow() {
			var w = GetWindow<UIRelationGraph>();
			w.titleContent = new GUIContent("UI Relation");
			w.Show();
			return w;
		}
		
		public static  UIRelationGraph Instance { get; private set; }

		public void ChangeSourceNode(UIViewConfiguration.Configuration configuration) {
			m_graphView.ChangeSourceNode(configuration);
		}
		
		private UIRelationGraphView m_graphView;
		public event Action Closed;

		private void OnEnable() {
			ConstructGraphView();
			Instance = this;
		}

		private void OnDisable() {
			Instance = null;
		}

		private void ConstructGraphView() {
			m_graphView = new UIRelationGraphView {
				name = "UI Relation Graph",
			};
			m_graphView.StretchToParentSize();
			rootVisualElement.Add(m_graphView);
		}

		private void OnDestroy() {
			Closed?.Invoke();
		}

		private void Save() {
		}
	}
}