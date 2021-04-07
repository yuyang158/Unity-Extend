using System;
using System.Collections.Generic;
using System.Linq;
using ListView;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extend.UI.Editor {
	public class UIViewTreeItem : TreeViewItem {
		public UIViewConfiguration.Configuration Configuration { get; }

		public UIViewTreeItem(UIViewConfiguration.Configuration configuration) : base(configuration.GetHashCode()) {
			Configuration = configuration;
		}

		public override string displayName {
			get => Configuration?.Name;
			set { }
		}
	}

	public class UIViewDelegate : IListViewDelegate<UIViewTreeItem> {
		private readonly UIViewConfiguration configurationContext;
		private readonly SerializedObject serializedObject;
		private int selectedIndex = -1;

		public UIViewDelegate(UIViewConfiguration context) {
			configurationContext = context;
			serializedObject = new SerializedObject(context);
		}

		public MultiColumnHeader Header => new MultiColumnHeader(new MultiColumnHeaderState(new[] {
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Name"), width = 10},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("UI View"), width = 20, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Background Fx"), width = 10, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Full Screen"), width = 5, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Attach Layer"), width = 10, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Transition"), width = 10, canSort = false}
		}));

		public List<TreeViewItem> GetData() {
			return configurationContext.Configurations.Select(configuration => new UIViewTreeItem(configuration)).Cast<TreeViewItem>().ToList();
		}

		public List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending) {
			var items = GetData();
			items.Sort((a, b) => ( isAscending ? -1 : 1 ) * string.Compare(a.displayName, b.displayName, StringComparison.Ordinal));
			return items;
		}

		private static readonly string[] columnIndexToFieldName = {"Name", "UIView", "BackgroundFx", "FullScreen", "AttachLayer", "Transition"};

		public void Draw(Rect rect, int columnIndex, UIViewTreeItem data, bool selected) {
			var index = Array.IndexOf(configurationContext.Configurations, data.Configuration);
			if( index == -1 )
				return;
			if( selected ) {
				selectedIndex = index;
			}

			var configurations = serializedObject.FindProperty("m_configurations");
			var element = configurations.GetArrayElementAtIndex(index);

			var fieldName = columnIndexToFieldName[columnIndex];
			var prop = element.FindPropertyRelative(fieldName);

			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(rect, prop, GUIContent.none);
		}

		public void OnItemClick(int id) {
		}

		public void OnContextClick() {
		}

		public void Add() {
			var configurations = serializedObject.FindProperty("m_configurations");
			configurations.InsertArrayElementAtIndex(configurations.arraySize);
			serializedObject.ApplyModifiedProperties();
		}

		public void Remove() {
			var configurations = serializedObject.FindProperty("m_configurations");
			if( selectedIndex >= 0 && selectedIndex < configurations.arraySize ) {
				configurations.DeleteArrayElementAtIndex(selectedIndex);
				serializedObject.ApplyModifiedProperties();
			}

			selectedIndex = -1;
		}

		public void Save() {
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(configurationContext);
			AssetDatabase.SaveAssets();
		}

		public void Revert() {
			serializedObject.UpdateIfRequiredOrScript();
		}
	}

	public class UIViewEditorWindow : EditorWindow {
		private static UIViewEditorWindow window;
		private static ListView<UIViewTreeItem> listView;

		private UIViewDelegate _delegate;
		private bool refreshFlag;

		[MenuItem("Window/UIView Window")]
		private static void OpenWindow() {
			if( window ) {
				window.Close();
				window = null;
				return;
			}

			window = GetWindow<UIViewEditorWindow>();
			window.titleContent = new GUIContent("UI View List");
			window.Show();
		}

		private void OnEnable() {
			const string path = "Assets/Resources/" + UIViewConfiguration.FILE_PATH + ".asset";
			var uiViewConfiguration = AssetDatabase.LoadAssetAtPath<UIViewConfiguration>(path);
			if( !uiViewConfiguration ) {
				uiViewConfiguration = CreateInstance<UIViewConfiguration>();
				AssetDatabase.CreateAsset(uiViewConfiguration, path);
			}

			_delegate = new UIViewDelegate(uiViewConfiguration);
			listView = new ListView<UIViewTreeItem>(_delegate);
			listView.Refresh();
		}

		private void OnGUI() {
			ButtonsGUI();
			var controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			if( refreshFlag ) {
				listView?.Refresh();
			}

			listView?.OnGUI(controlRect);
			refreshFlag = false;
		}

		private void ButtonsGUI() {
			GUILayout.BeginHorizontal();
			if( GUILayout.Button("Add") ) {
				_delegate.Add();
				refreshFlag = true;
			}

			if( GUILayout.Button("Remove") ) {
				_delegate.Remove();
				refreshFlag = true;
			}

			if( GUILayout.Button("Revert") ) {
				_delegate.Revert();
				refreshFlag = true;
			}

			if( GUILayout.Button("Save") ) {
				_delegate.Save();
			}

			GUILayout.EndHorizontal();
		}
	}
}