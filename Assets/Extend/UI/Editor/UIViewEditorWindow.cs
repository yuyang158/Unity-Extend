﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ListView;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;

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
		private const string m_closeButtonName = "ButtonClosePanel";

		public UIViewDelegate(UIViewConfiguration context) {
			configurationContext = context;
			serializedObject = new SerializedObject(context);
		}

		public string SearchText { get; set; }

		public MultiColumnHeader Header => new MultiColumnHeader(new MultiColumnHeaderState(new[] {
			new MultiColumnHeaderState.Column {headerContent = new GUIContent(EditorGUIUtility.FindTexture("console.erroricon")), width = 1},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Name"), width = 10},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("UI View"), width = 20, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Background Fx"), width = 10, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Full Screen"), width = 5, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Attach Layer"), width = 10, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Transition"), width = 10, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Close"), width = 10, canSort = false},
			// new MultiColumnHeaderState.Column {headerContent = new GUIContent("Frame Rate"), width = 10, canSort = false},
			new MultiColumnHeaderState.Column {headerContent = new GUIContent("Close Button Path"), width = 10, canSort = false}
		}));

		public List<TreeViewItem> GetData() {
			return configurationContext.Configurations
				.Where(configuration =>
					string.IsNullOrEmpty(SearchText) || configuration.Name.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) != -1)
				.Select(configuration => new UIViewTreeItem(configuration))
				.Cast<TreeViewItem>()
				.ToList();
		}

		public List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending) {
			var items = GetData();
			items.Sort((a, b) => ( isAscending ? -1 : 1 ) * string.Compare(a.displayName, b.displayName, StringComparison.Ordinal));
			return items;
		}

		private static readonly string[] columnIndexToFieldName = {
			"Name",
			"UIView",
			"BackgroundFx",
			"FullScreen",
			"AttachLayer",
			"Transition",
			"CloseMethod",
			// "FrameRate",
			"CloseButtonPath"
		};

		private static string FindNodeWithName(Transform nodeToSearch, string name) {
			if( nodeToSearch.name == name ) {
				return name;
			}

			for( int i = 0; i < nodeToSearch.childCount; i++ ) {
				var t = nodeToSearch.GetChild(i);
				var path = FindNodeWithName(t, name);
				if( !string.IsNullOrEmpty(path) ) {
					if( nodeToSearch.parent == null ) {
						return path;
					}

					path = $"{nodeToSearch.name}/{path}";
					return path;
				}
			}

			return string.Empty;
		}

		public void Draw(Rect rect, int columnIndex, UIViewTreeItem data, bool selected) {
			var index = Array.IndexOf(configurationContext.Configurations, data.Configuration);
			if( index == -1 )
				return;
			if( selected ) {
				selectedIndex = index;
			}

			var configurations = serializedObject.FindProperty("m_configurations");
			if( configurations == null ) {
				return;
			}

			var element = configurations.GetArrayElementAtIndex(index);
			if( columnIndex == 0 ) {
				var error = string.Empty;
				do {
					var sameNameCount = configurationContext.Configurations.Count(configuration => configuration.Name == data.Configuration.Name);
					if( sameNameCount > 1 ) {
						error = "Name duplicate";
						break;
					}

					if( data.Configuration.UIView is {GUIDValid: false} ) {
						error = "UI View can not be empty";
						break;
					}

					if( data.Configuration.CloseMethod == CloseOption.Button ) {
						var path = AssetDatabase.GUIDToAssetPath(data.Configuration.UIView.AssetGUID);
						var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
						if( string.IsNullOrEmpty(data.Configuration.CloseButtonPath) || !go.transform.Find(data.Configuration.CloseButtonPath) ) {
							data.Configuration.CloseButtonPath = FindNodeWithName(go.transform, m_closeButtonName);
							if( string.IsNullOrEmpty(data.Configuration.CloseButtonPath) ) {
								error = $"Can`t find node with name {m_closeButtonName}";
								break;
							}

							var button = go.transform.Find(data.Configuration.CloseButtonPath).GetComponent<Button>();
							if( !button ) {
								error = "Need component Button : " + data.Configuration.CloseButtonPath;
								data.Configuration.CloseButtonPath = "";
								break;
							}

							serializedObject.UpdateIfRequiredOrScript();
						}
					}
				} while( false );

				if( !string.IsNullOrEmpty(error) ) {
					EditorGUI.LabelField(rect, new GUIContent(EditorGUIUtility.FindTexture("console.erroricon"), error));
				}

				return;
			}

			var fieldName = columnIndexToFieldName[columnIndex - 1];
			var prop = element.FindPropertyRelative(fieldName);
			if( fieldName == "CloseButtonPath" ) {
				if( data.Configuration.CloseMethod == CloseOption.Button ) {
					EditorGUI.LabelField(rect, data.Configuration.CloseButtonPath);
				}
			}
			else if( fieldName == "FrameRate" ) {
				if(data.Configuration.FullScreen)
					prop.intValue = EditorGUI.IntSlider(rect, prop.intValue, 15, 60);
			}
			else {
				EditorGUIUtility.labelWidth = 1;
				EditorGUI.PropertyField(rect, prop, GUIContent.none);
			}
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
			configurationContext.Save();
		}

		public void ApplyChange() {
			serializedObject.ApplyModifiedProperties();
		}

		public void ToLua() {
			using(var luaFileStream = new FileStream("Lua/UIViewConfiguration.lua", FileMode.Create, FileAccess.Write))
			using( var writer = new StreamWriter(luaFileStream) ) {
				writer.WriteLine("local Configuration = CS.Extend.UI.UIViewConfiguration.Configuration");
				writer.WriteLine("local AssetReference = CS.Extend.Asset.AssetReference");
				writer.WriteLine("local UILayer = CS.Extend.UI.UILayer");
				writer.WriteLine("local CloseOption = CS.Extend.UI.CloseOption");
				writer.WriteLine("local c");

				writer.WriteLine("local configurations = {}");
				foreach( UIViewConfiguration.Configuration configuration in configurationContext.Configurations ) {
					writer.WriteLine("c = Configuration()");
					writer.WriteLine($"c.Name = \"{configuration.Name}\"");
					writer.WriteLine($"c.UIView = AssetReference(\"{configuration.UIView.AssetGUID}\")");
					if( configuration.BackgroundFx != null ) {
						writer.WriteLine($"c.BackgroundFx = AssetReference(\"{configuration.BackgroundFx.AssetGUID}\")");
					}
					writer.WriteLine($"c.FullScreen = {configuration.FullScreen.ToString().ToLower()}");

					if( configuration.Transition != null ) {
						writer.WriteLine($"c.Transition = AssetReference(\"{configuration.Transition.AssetGUID}\")");
					}
					writer.WriteLine($"c.AttachLayer = UILayer.{configuration.AttachLayer}");
					writer.WriteLine($"c.CloseMethod = CloseOption.{configuration.CloseMethod}");
					writer.WriteLine($"c.CloseButtonPath = \"{configuration.CloseButtonPath}\"");
					writer.WriteLine($"configurations.{configuration.Name} = c");
				}
				
				writer.WriteLine("return configurations");
			}
		}
	}

	public class UIViewEditorWindow : EditorWindow {
		private static UIViewEditorWindow window;
		private static ListView<UIViewTreeItem> m_listView;
		private int m_currentSelection;

		private UIViewDelegate _delegate;
		private bool refreshFlag;
		private SearchField m_searchField;

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

		private void OnEnterEditorMode(PlayModeStateChange change) {
			if( change == PlayModeStateChange.EnteredEditMode ) {
				Reset();
				Repaint();
			}
		}

		private void OnEnable() {
			Reset();
			EditorApplication.playModeStateChanged += OnEnterEditorMode;
		}

		private void Reset() {
			var uiViewConfiguration = UIViewConfiguration.Load();
			_delegate = new UIViewDelegate(uiViewConfiguration);
			m_listView = new ListView<UIViewTreeItem>(_delegate);
			m_listView.Refresh();
			m_searchField = new SearchField();
		}

		private void OnDisable() {
			_delegate = null;
			EditorApplication.playModeStateChanged -= OnEnterEditorMode;
		}

		private void OnGUI() {
			ButtonsGUI();
			var controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight),
				GUILayout.ExpandWidth(true));
			var searchText = m_searchField.OnGUI(controlRect, _delegate.SearchText);
			controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			if( _delegate.SearchText != searchText ) {
				refreshFlag = true;
				_delegate.SearchText = searchText;
			}

			if( refreshFlag ) {
				m_listView?.Refresh();
			}

			EditorGUI.BeginChangeCheck();
			m_listView?.OnGUI(controlRect);
			if( EditorGUI.EndChangeCheck() ) {
				_delegate.ApplyChange();
			}

			refreshFlag = false;
		}

		private void ButtonsGUI() {
			GUILayout.BeginHorizontal();
			if( GUILayout.Button("New") ) {
				_delegate.Add();
				refreshFlag = true;
			}

			if( GUILayout.Button("Remove") ) {
				_delegate.Remove();
				refreshFlag = true;
			}

			if( GUILayout.Button("Revert") ) {
				Reset();
			}

			if( GUILayout.Button("Save") ) {
				_delegate.Save();
			}

			if( GUILayout.Button("To Lua") ) {
				_delegate.Save();
				_delegate.ToLua();
			}

			GUILayout.EndHorizontal();
		}
	}
}